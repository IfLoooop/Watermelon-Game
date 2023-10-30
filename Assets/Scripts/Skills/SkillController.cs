using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Points;
using Watermelon_Game.Utility;
using Watermelon_Game.Web;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillController : SerializedMonoBehaviour
    {
        #region WebSettings
        [Header("WebSettings")]
        [Tooltip("Points required to use he power skill")]
        [ShowInInspector] private static uint powerPointsRequirement = 20;
        [Tooltip("Points required to use he evolve skill")]
        [ShowInInspector] private static uint evolvePointsRequirement = 50;
        [Tooltip("Points required to use he destroy skill")]
        [ShowInInspector] private static uint destroyPointsRequirement = 50;
        [Tooltip("Point requirement increase in % after every skill use (Individual for each skill)")]
        [ShowInInspector] private static float skillPointIncrease = .1f;
        #endregion
        
        #region Inspector Fields
#if UNITY_EDITOR
        [Header("Development")]
        [Tooltip("If true, all skills are enabled, regardless of the points (Only for Development)")]
        [SerializeField] private bool forceEnableSkills;
#endif
        [Header("References")]
        [Tooltip("Reference to the power skill GameObject in the scene")]
        [SerializeField] private GameObject power;
        [Tooltip("Reference to the evolve skill GameObject in the scene")]
        [SerializeField] private GameObject evolve;
        [Tooltip("Reference to the destroy skill GameObject in the scene")]
        [SerializeField] private GameObject destroy;
        
        [Header("Settings")]
        [Tooltip("Fruit drop/shoot force multiplier when a skill is used")]
        [SerializeField] private float shootForceMultiplier = 100;
        [Tooltip("Fruit drop/shoot force when the power skill is used")]
        [SerializeField] private float powerSkillForce = 30000f;
        [Tooltip("Mass of the fruit when the power skill is used")]
        [SerializeField] private float powerSkillMass = 200f;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="SkillController"/>
        /// </summary>
        private static SkillController instance;
        
        /// <summary>
        /// Contains data for the power skill
        /// </summary>
        private SkillData powerSkill;
        /// <summary>
        /// Contains data for the evolve skill
        /// </summary>
        private SkillData evolveSkill;
        /// <summary>
        /// Contains data for the destroy skill
        /// </summary>
        private SkillData destroySkill;
        /// <summary>
        /// Maps a <see cref="Skill"/> to its data
        /// </summary>
        private ReadOnlyDictionary<Skill, SkillData> skillMap;
        
        /// <summary>
        /// Time in seconds to wait before the mass of aa <see cref="FruitBehaviour"/> is reset
        /// </summary>
        private readonly WaitForSeconds massResetWaitTime = new(2);
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="powerPointsRequirement"/>
        /// </summary>
        public static uint PowerPointsRequirement => powerPointsRequirement;
        /// <summary>
        /// <see cref="evolvePointsRequirement"/>
        /// </summary>
        public static uint EvolvePointsRequirement => evolvePointsRequirement;
        /// <summary>
        /// <see cref="destroyPointsRequirement"/>
        /// </summary>
        public static uint DestroyPointsRequirement => destroyPointsRequirement;
        /// <summary>
        /// <see cref="skillPointIncrease"/>
        /// </summary>
        public static float SkillPointIncrease => skillPointIncrease;
        /// <summary>
        /// <see cref="shootForceMultiplier"/>
        /// </summary>
        public static float ShootForceMultiplier => instance.shootForceMultiplier;
        #endregion

        #region Events
        /// <summary>
        /// Is called when a <see cref="Skill"/> is activated <br/>
        /// <b>Parameter:</b> The <see cref="Skill"/> that was activated <br/>
        /// <i>Is nullable, but the <see cref="Skill"/> will never be null (Needed for <see cref="FruitBehaviour.SetActiveSkill"/>)</i>
        /// </summary>
        public static event Action<Skill?> OnSkillActivated;
        /// <summary>
        /// Is called after a <see cref="Skill"/> was used <br/>
        /// <b>Parameter:</b> The points that were required to activate the <see cref="Skill"/>
        /// </summary>
        public static event Action<uint> OnSkillUsed;  
        #endregion
        
        #region Methods
        /// <summary>
        /// Needs to be called with <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void SubscribeToWebSettings()
        {
            WebSettings.OnApplyWebSettings += ApplyWebSettings;
        }

        private void OnDestroy()
        {
            WebSettings.OnApplyWebSettings -= ApplyWebSettings;
        }
        
        /// <summary>
        /// Tries to set the values from the web settings
        /// </summary>
        private static void ApplyWebSettings()
        {
            var _callerType = typeof(SkillController);
            WebSettings.TrySetValue(nameof(PowerPointsRequirement), ref powerPointsRequirement, _callerType);
            WebSettings.TrySetValue(nameof(EvolvePointsRequirement), ref evolvePointsRequirement, _callerType);
            WebSettings.TrySetValue(nameof(DestroyPointsRequirement), ref destroyPointsRequirement, _callerType);
            WebSettings.TrySetValue(nameof(SkillPointIncrease), ref skillPointIncrease, _callerType);
        }

        private void OnEnable()
        {
            GameController.OnResetGameStarted += this.ResetSkillPointsRequirement;
            PointsController.OnPointsChanged += this.PointsChanged;
            FruitBehaviour.OnSkillUsed += this.SkillUsed;
        }

        private void OnDisable()
        {
            GameController.OnResetGameStarted -= this.ResetSkillPointsRequirement;
            PointsController.OnPointsChanged -= this.PointsChanged;
            FruitBehaviour.OnSkillUsed -= this.SkillUsed;
        }

        private void Awake()
        {
            instance = this;
            
            this.powerSkill = InitializeSkill(this.power, KeyCode.Alpha1, Skill.Power, powerPointsRequirement);
            this.evolveSkill = InitializeSkill(this.evolve, KeyCode.Alpha2, Skill.Evolve, evolvePointsRequirement);
            this.destroySkill = InitializeSkill(this.destroy, KeyCode.Alpha3, Skill.Destroy, destroyPointsRequirement);

            this.skillMap = new ReadOnlyDictionary<Skill, SkillData>(new Dictionary<Skill, SkillData>
            {
                { Skill.Power, this.powerSkill },
                { Skill.Evolve, this.evolveSkill },
                { Skill.Destroy, this.destroySkill }
            });
        }

        /// <summary>
        /// Sets all needed value for a <see cref="SkillData"/> (<see cref="Skill"/>)
        /// </summary>
        /// <param name="_GameObject">The <see cref="GameObject"/>, the <see cref="Skill"/> represent in the scene</param>
        /// <param name="_KeyToActivate">The <see cref="KeyCode"/> to activate the <see cref="Skill"/></param>
        /// <param name="_Skill">The type of the <see cref="Skill"/></param>
        /// <param name="_PointRequirements">The points, required to activate the <see cref="Skill"/></param>
        /// <returns><see cref="SkillData"/></returns>
        private static SkillData InitializeSkill(GameObject _GameObject, KeyCode _KeyToActivate, Skill _Skill, uint _PointRequirements)
        {
            var _skillReferences = _GameObject.GetComponent<SkillReferences>();
            
            return new SkillData(_skillReferences, _KeyToActivate, _Skill, _PointRequirements);
        }
        
#if UNITY_EDITOR
        private void Start()
        {
            if (this.forceEnableSkills)
            {
                this.powerSkill.EnableSkill();
                this.evolveSkill.EnableSkill();
                this.destroySkill.EnableSkill();   
            }
        }
#endif

        private void Update()
        {
            this.SkillInput(this.powerSkill);
            this.SkillInput(this.evolveSkill);
            this.SkillInput(this.destroySkill);
        }
        
        /// <summary>
        /// Handles the input for when a skill button is pressed
        /// </summary>
        /// <param name="_SkillToActivate">The <see cref="Skill"/> button that was pressed</param>
        private void SkillInput(SkillData _SkillToActivate)
        {
            if (Input.GetKeyDown(_SkillToActivate.KeyToActivate) && _SkillToActivate.CanBeActivated)
            {
                AudioPool.PlayClip(AudioClipName.SkillSelect);
                
                if (!_SkillToActivate.IsActive)
                {
                    this.DeactivateActiveSkills();
                    
                    _SkillToActivate.ActivateSkill();
                    OnSkillActivated?.Invoke(_SkillToActivate.Skill);
                }
                else
                {
                    _SkillToActivate.DeactivateSkill();
                    OnSkillActivated?.Invoke(null);
                }
            }
        }
        
        /// <summary>
        /// Enables/disables the skills, depending on the given <see cref="_CurrentPoints"/>
        /// </summary>
        /// <param name="_CurrentPoints"><see cref="PointsController.currentPoints"/></param>
        private void PointsChanged(uint _CurrentPoints)
        {
#if UNITY_EDITOR
            if (this.forceEnableSkills)
            {
                return;
            }
#endif
            if (_CurrentPoints >= this.powerSkill.CurrentPointsRequirement)
            {
                this.powerSkill.EnableSkill();
            }
            else
            {
                this.powerSkill.DisableSkill();
            }
            
            if (_CurrentPoints >= this.evolveSkill.CurrentPointsRequirement)
            {
                this.evolveSkill.EnableSkill();
            }
            else
            {
                this.evolveSkill.DisableSkill();
            }
            
            if (_CurrentPoints >= this.destroySkill.CurrentPointsRequirement)
            {
                this.destroySkill.EnableSkill();
            }
            else
            {
                this.destroySkill.DisableSkill();
            }
        }
        
        /// <summary>
        /// Deactivates the currently active skill
        /// </summary>
        private void DeactivateActiveSkills()
        {
            this.powerSkill.DeactivateSkill();
            this.evolveSkill.DeactivateSkill();
            this.destroySkill.DeactivateSkill();
        }
        
        /// <summary>
        /// Shoots the fruit with enhanced force and mass
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to set the mass of</param>
        /// <param name="_Direction">The direction to shoot the fruit in</param>
        public static void Skill_Power(FruitBehaviour _FruitBehaviour, Vector2 _Direction)
        {
            _Direction *= instance.powerSkillForce;
            _FruitBehaviour.SetMass(instance.powerSkillMass, Operation.Set);
            _FruitBehaviour.AddForce(_Direction, ForceMode2D.Impulse);
            instance.StartCoroutine(instance.ResetMass(_FruitBehaviour));
        }

        /// <summary>
        /// Resets the <see cref="Rigidbody2D.mass"/> of the given <see cref="FruitBehaviour"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to set the mass of</param>
        /// <returns>Waits for the duration in <see cref="massResetWaitTime"/></returns>
        private IEnumerator ResetMass(FruitBehaviour _FruitBehaviour)
        {
            yield return this.massResetWaitTime;
            
            if (_FruitBehaviour != null)
            {
                _FruitBehaviour.SetMass(0, Operation.Set);
            }
        }

        /// <summary>
        /// Evolves the <see cref="FruitBehaviour"/> with the given <see cref="HashCode"/>
        /// </summary>
        /// <param name="_FruitHashcode">The <see cref="HashCode"/> of the <see cref="FruitBehaviour"/> to evolve</param>
        public static void Skill_Evolve(int _FruitHashcode)
        {
            var _fruit = FruitController.GetFruit(_FruitHashcode)!;
            var _position = _fruit.transform.position;
            
            FruitController.Evolve(_position, _fruit);
        }

        /// <summary>
        /// Destroys the <see cref="FruitBehaviour"/> with the given <see cref="HashCode"/>
        /// </summary>
        /// <param name="_FruitHashcode">The <see cref="HashCode"/> of the <see cref="FruitBehaviour"/> to destroy</param>
        public static void Skill_Destroy(int _FruitHashcode)
        {
            FruitController.GetFruit(_FruitHashcode)!.DestroyFruit();
        }

        /// <summary>
        /// <see cref="FruitBehaviour.OnSkillUsed"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skill"/> that was used</param>
        private void SkillUsed(Skill? _Skill)
        {
            var _skill = skillMap[_Skill!.Value];
            
            OnSkillUsed?.Invoke(_skill.CurrentPointsRequirement);
            
            _skill.PlayAnimation();
            _skill.CurrentPointsRequirement += (uint)(_skill.CurrentPointsRequirement * skillPointIncrease);
            
            this.DeactivateActiveSkills();
        }
        
        /// <summary>
        /// Reset the skill points requirements of all <see cref="Skill"/>s, back to their initial values -> <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetSkillPointsRequirement()
        {
            this.powerSkill.CurrentPointsRequirement = powerPointsRequirement;
            this.evolveSkill.CurrentPointsRequirement = evolvePointsRequirement;
            this.destroySkill.CurrentPointsRequirement = destroyPointsRequirement;
        }
        #endregion
    }
}