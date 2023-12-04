using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;
using Watermelon_Game.Points;
using Watermelon_Game.Singletons;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillController : PersistantMonoBehaviour<SkillController>
    {
        #region WebSettings
        [Header("WebSettings")]
        [Tooltip("Points required to use he power skill")]
        [ShowInInspector] private static ProtectedUInt32 powerPointsRequirement = 20;
        [Tooltip("Points required to use he evolve skill")]
        [ShowInInspector] private static ProtectedUInt32 evolvePointsRequirement = 50;
        [Tooltip("Points required to use he destroy skill")]
        [ShowInInspector] private static ProtectedUInt32 destroyPointsRequirement = 50;
        [Tooltip("Point requirement increase in % after every skill use (Individual for each skill)")]
        [ShowInInspector] private static ProtectedFloat skillPointIncrease = .1f;
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
        [SerializeField] private ProtectedFloat shootForceMultiplier = 100;
        [Tooltip("Fruit drop/shoot force when the power skill is used")]
        [SerializeField] private ProtectedFloat powerSkillForce = 30000f;
        [Tooltip("Mass of the fruit when the power skill is used")]
        [SerializeField] private ProtectedFloat powerSkillMass = 200f;
        [Tooltip("Delay in seconds between skill selection for the scroll wheel")]
        [SerializeField] private float scrollDelay = .1f;
        #endregion

        #region Fields
        /// <summary>
        /// Maps a <see cref="Skill"/> to its data
        /// </summary>
        private ReadOnlyDictionary<Skill, SkillData> skillMap;
        /// <summary>
        /// Index in <see cref="skillMap"/> of the last active skill
        /// </summary>
        private int lastActiveSkill;
        /// <summary>
        /// Timestamp when the last skill was selected
        /// </summary>
        private float lastSkillSelectionTimestamp;
        
        /// <summary>
        /// Time in seconds to wait before the mass of aa <see cref="FruitBehaviour"/> is reset
        /// </summary>
        private readonly WaitForSeconds massResetWaitTime = new(2);
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="powerPointsRequirement"/>
        /// </summary>
        public static ProtectedUInt32 PowerPointsRequirement => powerPointsRequirement;
        /// <summary>
        /// <see cref="evolvePointsRequirement"/>
        /// </summary>
        public static ProtectedUInt32 EvolvePointsRequirement => evolvePointsRequirement;
        /// <summary>
        /// <see cref="destroyPointsRequirement"/>
        /// </summary>
        public static ProtectedUInt32 DestroyPointsRequirement => destroyPointsRequirement;
        /// <summary>
        /// <see cref="skillPointIncrease"/>
        /// </summary>
        public static ProtectedFloat SkillPointIncrease => skillPointIncrease;
        /// <summary>
        /// <see cref="shootForceMultiplier"/>
        /// </summary>
        public static ProtectedFloat ShootForceMultiplier => Instance.shootForceMultiplier;
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
        /// <b>Parameter2:</b> The points that were required to activate the <see cref="Skill"/>
        /// </summary>
        public static event Action<uint> OnSkillUsed;  
        #endregion
        
        #region Methods
        // /// <summary>
        // /// Needs to be called with <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>
        // /// </summary>
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        // private static void SubscribeToWebSettings()
        // {
        //     WebSettings.OnApplyWebSettings += ApplyWebSettings;
        // }
        //
        // protected override void OnDestroy()
        // {
        //     base.OnDestroy();
        //     WebSettings.OnApplyWebSettings -= ApplyWebSettings;
        // }
        //
        // /// <summary>
        // /// Tries to set the values from the web settings
        // /// </summary>
        // private static void ApplyWebSettings()
        // {
        //     var _callerType = typeof(SkillController);
        //     WebSettings.TrySetValue(nameof(PowerPointsRequirement), ref powerPointsRequirement, _callerType);
        //     WebSettings.TrySetValue(nameof(EvolvePointsRequirement), ref evolvePointsRequirement, _callerType);
        //     WebSettings.TrySetValue(nameof(DestroyPointsRequirement), ref destroyPointsRequirement, _callerType);
        //     WebSettings.TrySetValue(nameof(SkillPointIncrease), ref skillPointIncrease, _callerType);
        // }

        private void OnEnable()
        {
            OnSkillActivated += SetActiveSkill;
            GameController.OnResetGameStarted += this.ResetSkillPointsRequirement;
            PointsController.OnPointsChanged += this.PointsChanged;
            FruitBehaviour.OnSkillUsed += this.SkillUsed;
        }

        private void OnDisable()
        {
            OnSkillActivated -= SetActiveSkill;
            GameController.OnResetGameStarted -= this.ResetSkillPointsRequirement;
            PointsController.OnPointsChanged -= this.PointsChanged;
            FruitBehaviour.OnSkillUsed -= this.SkillUsed;
        }

        protected override void Init()
        {
            base.Init();
            this.skillMap = new ReadOnlyDictionary<Skill, SkillData>(new Dictionary<Skill, SkillData>
            {
                { Skill.Power, InitializeSkill(this.power, KeyCode.Alpha1, Skill.Power, powerPointsRequirement) },
                { Skill.Evolve, InitializeSkill(this.evolve, KeyCode.Alpha2, Skill.Evolve, evolvePointsRequirement) },
                { Skill.Destroy, InitializeSkill(this.destroy, KeyCode.Alpha3, Skill.Destroy, destroyPointsRequirement) }
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
                this.skillMap.ForEach(_Skill => _Skill.Value.EnableSkill());
            }
        }
#endif
        
        private void Update() // TODO: Use InputController
        {
            if (!GameController.ActiveGame || MenuController.IsAnyMenuOpen)
            {
                return;
            }
            
            this.KeyboardInput();
            this.MouseInput();
        }

        /// <summary>
        /// Sets <see cref="lastActiveSkill"/> <see cref="OnSkillActivated"/>
        /// </summary>
        /// <param name="_Skill"></param>
        private void SetActiveSkill(Skill? _Skill)
        {
            if (_Skill != null)
            {
                this.lastActiveSkill = this.skillMap.FindIndex(_Kvp => _Kvp.Key == _Skill.Value);
            }
        }
        
        /// <summary>
        /// Handles keyboard input
        /// </summary>
        private void KeyboardInput()
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < this.skillMap.Count; i++)
            {
                if (Input.GetKeyDown(this.skillMap.ElementAt(i).Value.KeyToActivate))
                {
                    this.SelectSkill((uint)i);
                }
            }
        }

        /// <summary>
        /// Handles mouse input
        /// </summary>
        private void MouseInput()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                this.SelectSkill(0);
            }

            if (Time.time > this.lastSkillSelectionTimestamp + this.scrollDelay)
            {
                if (Input.mouseScrollDelta.y > 0)
                {
                    this.SelectSkill(+1);
                }
                else if (Input.mouseScrollDelta.y < 0)
                {
                    this.SelectSkill(-1);
                }   
            }
        }

        /// <summary>
        /// Selects the next skill that can be activated in the given direction, or none of no skill could be activated
        /// </summary>
        /// <param name="_Direction">
        /// <b>0:</b> Uses <see cref="lastActiveSkill"/> <br/>
        /// <b>-1:</b> Left <br/>
        /// <b>+1:</b> Right
        /// </param>
        private void SelectSkill(int _Direction)
        {
            var _index = this.lastActiveSkill;
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < this.skillMap.Count - 1; i++)
            {
                _index += _Direction;
                
                if (_index >= this.skillMap.Count)
                {
                    _index = 0;
                }
                else if (_index < 0)
                {
                    _index = this.skillMap.Count - 1;
                }

                if (this.skillMap.ElementAt(_index).Value.CanBeActivated)
                {
                    this.SelectSkill((uint)_index);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Selects the skill with the given index in <see cref="skillMap"/>
        /// </summary>
        /// <param name="_Index">Must be a valid index in <see cref="skillMap"/></param>
        private void SelectSkill(uint _Index)
        {
            var _index = (int)_Index;
            var _skillData = this.skillMap.ElementAt(_index).Value;
            
            if (_skillData.CanBeActivated)
            {
                this.lastSkillSelectionTimestamp = Time.time;
                this.lastActiveSkill = _index;
                AudioPool.PlayClip(AudioClipName.SkillSelect);
                
                if (!_skillData.IsActive)
                {
                    OnSkillActivated?.Invoke(_skillData.Skill);
                }
                else
                {
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
            foreach (var (_, _skill) in this.skillMap)
            {
                if (_CurrentPoints >= _skill.CurrentPointsRequirement)
                    _skill.EnableSkill();
                else
                    _skill.DisableSkill();
            }
        }
        
        /// <summary>
        /// Deactivates the currently active skill
        /// </summary>
        private void DeactivateActiveSkills()
        {
            foreach (var (_, _skill) in this.skillMap)
            {
                _skill.DeactivateSkill();
            }
        }
        
        /// <summary>
        /// Shoots the fruit with enhanced force and mass
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to set the mass of</param>
        /// <param name="_Direction">The direction to shoot the fruit in</param>
        public static void Skill_Power(FruitBehaviour _FruitBehaviour, Vector2 _Direction)
        {
            _Direction *= Instance.powerSkillForce;
            _FruitBehaviour.SetMass(Instance.powerSkillMass, Operation.Set);
            _FruitBehaviour.AddForce(_Direction, ForceMode2D.Impulse);
            Instance.StartCoroutine(Instance.ResetMass(_FruitBehaviour));
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
        /// <param name="_Authority">Indicates if the local client has authority over this fruit</param>
        /// <param name="_FruitHashcode">The <see cref="HashCode"/> of the <see cref="FruitBehaviour"/> to evolve</param>
        public static void Skill_Evolve(bool _Authority, int _FruitHashcode)
        {
            if (FruitController.GetFruit(_FruitHashcode) is {} _fruit)
            {
                var _position = _fruit.transform.position;
            
                FruitController.Evolve(_Authority, _position, _fruit);
            }
        }

        /// <summary>
        /// Destroys the <see cref="FruitBehaviour"/> with the given <see cref="HashCode"/>
        /// </summary>
        /// <param name="_FruitHashcode">The <see cref="HashCode"/> of the <see cref="FruitBehaviour"/> to destroy</param>
        public static void Skill_Destroy(int _FruitHashcode)
        {
            FruitController.GetFruit(_FruitHashcode)?.DestroyFruit();
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
            this.skillMap[Skill.Power].CurrentPointsRequirement = powerPointsRequirement;
            this.skillMap[Skill.Evolve].CurrentPointsRequirement = evolvePointsRequirement;
            this.skillMap[Skill.Destroy].CurrentPointsRequirement = destroyPointsRequirement;
        }
        #endregion
    }
}