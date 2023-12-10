using System;
using System.Collections;
using AssetKits.ParticleImage;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using OPS.AntiCheat.Prefs;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Singletons;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility.Pools;

namespace Watermelon_Game.Points
{
    /// <summary>
    /// Logic for everything to do with points
    /// </summary>
    internal sealed class PointsController : PersistantGameModeTransition<PointsController>
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the Multiplier component")]
        [SerializeField] private Multiplier multiplier;
        [Tooltip("Displays the points for the current game")]
        [SerializeField] private TextMeshProUGUI pointsText;
        [Tooltip("Displays the current melon slice amount")]
        [SerializeField] private TextMeshProUGUI melonSlicesText;
        [Tooltip("Animation that is played when a particle has reached the melon slice icon")]
        [SerializeField] private Animation melonSlicesPulse;
        
        [Header("Settings")]
        [Tooltip("Time in seconds to wait, between each update of \"pointsAmount\", when the points change")]
        [SerializeField] private float pointsWaitTime = .05f;
        #endregion

        #region Constants
        /// <summary>
        /// <see cref="PlayerPrefs"/> key for <see cref="melonSlices"/>
        /// </summary>
        private const string MELON_SLICES_KEY = "MelonSlices";
        /// <summary>
        /// Max amount for <see cref="ParticleImage.rateOverTime"/>
        /// </summary>
        private const float PARTICLE_IMAGE_RATE = 50f;
        #endregion
        
        #region Fields
        /// <summary>
        /// Particle effect to add the <see cref="currentPoints"/> to <see cref="melonSlices"/>
        /// </summary>
        private ParticleImage particleImage;
        /// <summary>
        /// Temporary storage for <see cref="currentPoints"/> while the points are being added to <see cref="melonSlices"/>
        /// </summary>
        private ProtectedFloat tmpPoints;
        /// <summary>
        /// The amount that is added to <see cref="melonSlices"/> per particle
        /// </summary>
        private ProtectedFloat amountPerParticle;
        /// <summary>
        /// The remaining amount to add to <see cref="melonSlices"/> <br/>
        /// <i>In case of fractions during <see cref="amountPerParticle"/> calculation</i>
        /// </summary>
        private ProtectedFloat remainder;
        
        /// <summary>
        /// The current points amount <br/>
        /// <i>Will be reset on <see cref="GameController"/><see cref="GameController.OnResetGameFinished"/></i>
        /// </summary>
        private ProtectedUInt32 currentPoints;
        /// <summary>
        /// Points that need to be added/subtracted from <see cref="currentPoints"/> (If != 0)
        /// </summary>
        private ProtectedUInt32 pointsDelta;
        
        /// <summary>
        /// In game currency
        /// </summary>
        private ProtectedUInt64 melonSlices;
        
        /// <summary>
        /// Adds/subtract the amount in <see cref="pointsDelta"/> from <see cref="currentPoints"/>
        /// </summary>
        [CanBeNull] private IEnumerator pointsCoroutine;
        /// <summary>
        /// <see cref="pointsWaitTime"/>
        /// </summary>
        private WaitForSeconds pointsWaitForSeconds;

        /// <summary>
        /// Index of the <see cref="AudioWrapper"/> in <see cref="AudioPool.assignedAudioWrappers"/>, for the <see cref="AudioClipName.MelonSliceAdd"/> <see cref="AudioClip"/>
        /// </summary>
        private int addMelonSliceIndex;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="currentPoints"/>
        /// </summary>
        public static ProtectedUInt32 CurrentPoints => Instance.currentPoints;
        #endregion

        #region Events
        /// <summary>
        /// Is called when <see cref="currentPoints"/> value changes <br/>
        /// <b>Parameter:</b> The value of <see cref="currentPoints"/>
        /// </summary>
        public static event Action<uint> OnPointsChanged; 
        #endregion
        
        #region Methods
        protected override void Init()
        {
            base.Init();
            this.particleImage = base.GetComponentInChildren<ParticleImage>();
            this.pointsWaitForSeconds = new WaitForSeconds(this.pointsWaitTime);
            this.addMelonSliceIndex = AudioPool.CreateAssignedAudioWrapper(AudioClipName.MelonSliceAdd, base.transform);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            GameController.OnResetGameFinished += this.GameEnded;
            FruitController.OnEvolve += AddPoints;
            FruitController.OnGoldenFruitCollision += AddPoints;
            SkillController.OnSkillUsed += this.SubtractPoints;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            GameController.OnResetGameFinished -= this.GameEnded;
            FruitController.OnEvolve -= AddPoints;
            FruitController.OnGoldenFruitCollision -= AddPoints;
            SkillController.OnSkillUsed -= this.SubtractPoints;
        }

        private void Start()
        {
            this.Load();
            this.melonSlicesText.text = this.melonSlices.ToString();
        }

        private void OnApplicationQuit()
        {
            this.Save();
        }

        /// <summary>
        /// Saves the value of <see cref="melonSlices"/>
        /// </summary>
        private void Save()
        {
            ProtectedPlayerPrefs.SetString(MELON_SLICES_KEY, this.melonSlices.ToString());
        }

        /// <summary>
        /// Loads the value of <see cref="melonSlices"/>
        /// </summary>
        private void Load()
        {
            this.melonSlices = ulong.Parse(ProtectedPlayerPrefs.GetString(MELON_SLICES_KEY, 0.ToString()));
            this.melonSlicesText.text = this.melonSlices.ToString();
        }
        
        /// <summary>
        /// Adds points to <see cref="currentPoints"/> depending on the given <see cref="Fruit"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to get the points for</param>
        private void AddPoints(Fruit _Fruit)
        {
            // TODO: Dirty fix
            // TODO: When a client is connected to a host, this is null when the clients golden fruit collides with another fruit
            if (this.multiplier == null)
            {
                return;
            }
            
            this.multiplier.StartMultiplier();

            var _points = (int)((int)_Fruit + this.multiplier.CurrentMultiplier);
            this.SetPoints(_points);
        }
        
        /// <summary>
        /// Subtracts the given amount from <see cref="currentPoints"/>
        /// </summary>
        /// <param name="_PointsToSubtract">The points to subtract from <see cref="currentPoints"/></param>
        private void SubtractPoints(uint _PointsToSubtract) 
        {
            this.SetPoints(-(int)_PointsToSubtract);
        }
        
        /// <summary>
        /// Adds the given points to <see cref="currentPoints"/> <br/>
        /// <i>Set to 0 to reset the points</i>
        /// </summary>
        /// <param name="_Points">The points to add/subtract to <see cref="currentPoints"/></param>
        private void SetPoints(int _Points)
        {
            if (_Points == 0)
            {
                this.currentPoints = 0;
                this.pointsDelta = 0;
            }
            else
            {
                this.currentPoints = (uint)Mathf.Clamp(this.currentPoints + _Points, 0, uint.MaxValue);
                
                if (this.pointsCoroutine == null)
                {
                    this.pointsCoroutine = SetPoints();
                    StartCoroutine(this.pointsCoroutine);
                }
            }
            
            OnPointsChanged?.Invoke(this.currentPoints);
        }

        /// <summary>
        /// Gradually increase/decreases the points over time
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetPoints()
        {
#if UNITY_EDITOR
            // That way the editor doesn't have to be restarted when the value of "pointsWaitTime" is adjusted
            this.pointsWaitForSeconds = new WaitForSeconds(this.pointsWaitTime);
#endif
            
            while (this.pointsDelta != this.currentPoints)
            {
                if (this.pointsDelta < this.currentPoints)
                {
                    this.pointsDelta++;
                }
                else if (this.pointsDelta > this.currentPoints)
                {
                    this.pointsDelta--;
                }
                
                this.pointsText.text = this.pointsDelta.ToString();
                
                yield return this.pointsWaitForSeconds;
            }
            
            this.pointsCoroutine = null;
        }
        
        /// <summary>
        /// Sets <see cref="melonSlices"/> and reset <see cref="currentPoints"/>
        /// </summary>
        /// <param name="_ResetReason">Not needed here</param>
        private void GameEnded(ResetReason _ResetReason)
        {
            if (this.currentPoints > 0)
            {
                if (this.pointsCoroutine != null)
                {
                    base.StopCoroutine(this.pointsCoroutine);
                    this.pointsCoroutine = null;
                    this.pointsText.text = this.currentPoints.ToString();
                }

                if (this.particleImage.isPlaying)
                {
                    this.particleImage.Stop();
                }
                
                this.tmpPoints += this.currentPoints;
                this.particleImage.rateOverTime = this.tmpPoints < PARTICLE_IMAGE_RATE ? this.tmpPoints : PARTICLE_IMAGE_RATE;
                this.amountPerParticle = this.tmpPoints / this.particleImage.rateOverTime;
                this.remainder = this.tmpPoints % this.particleImage.rateOverTime;

                this.SetPoints(0);
            
                this.particleImage.Play();   
            }
        }
        
        /// <summary>
        /// Adds <see cref="currentPoints"/> to <see cref="melonSlices"/> at the end of a game
        /// </summary>
        public void AddMelonSlices()
        {
            if (!AudioPool.IsAssignedClipPlaying(this.addMelonSliceIndex))
            {
                AudioPool.PlayAssignedClip(this.addMelonSliceIndex);
            }
            if (!this.melonSlicesPulse.isPlaying)
            {
                this.melonSlicesPulse.Play();    
            }
            
            this.tmpPoints = Mathf.Clamp(this.tmpPoints - this.amountPerParticle, 0, uint.MaxValue);
            
            if (this.melonSlices > ulong.MaxValue - this.amountPerParticle)
            {
                this.melonSlices = ulong.MaxValue;
            }
            else
            {
                
                this.melonSlices += (ulong)this.amountPerParticle;
            }

            this.melonSlicesText.text = this.melonSlices.ToString();
            if (this.tmpPoints >= this.currentPoints)
            {
                this.pointsText.text = ((uint)this.tmpPoints).ToString();   
            }
        }

        /// <summary>
        /// Adds the <see cref="remainder"/> to <see cref="melonSlices"/> when the last particle has finished
        /// </summary>
        public void LastParticleFinished()
        {
            this.amountPerParticle = this.remainder;
            this.AddMelonSlices();
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// <see cref="AddPoints"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to get the points for</param>
        /// <param name="_Multiplier">Multiplier for the added points</param>
        public static void AddPoints_DEVELOPMENT(Fruit _Fruit, float _Multiplier)
        {
            Instance.multiplier.StartMultiplier();

            var _points = (int)(((int)_Fruit + Instance.multiplier.CurrentMultiplier) * _Multiplier);
            Instance.SetPoints(_points);
        }
        
        /// <summary>
        /// <see cref="SubtractPoints"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_PointsToSubtract">The points to subtract from <see cref="currentPoints"/></param>
        /// <param name="_Multiplier">Multiplier for the subtracted points</param>
        public static void SubtractPoints_DEVELOPMENT(uint _PointsToSubtract, float _Multiplier)
        {
            Instance.SetPoints(-(int)(_PointsToSubtract * _Multiplier));
        }
#endif
        #endregion
    }
}