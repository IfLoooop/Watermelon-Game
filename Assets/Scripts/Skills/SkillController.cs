using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Menu;
using Watermelon_Game.Points;
using Watermelon_Game.Web;
using static Watermelon_Game.Web.WebSettings;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillController : MonoBehaviour, IWebSettings
    {
        #region Inspector Fields

#if UNITY_EDITOR
        [SerializeField] private bool forceEnableSkills;
#endif
        [Header("References")]
        [SerializeField] private GameObject power;
        [SerializeField] private GameObject evolve;
        [SerializeField] private GameObject destroy;
        [Header("Settings")]
        [SerializeField] private uint powerPointsRequirement = 20;
        [SerializeField] private uint evolvePointsRequirement = 50;
        [SerializeField] private uint destroyPointsRequirement = 50;
        [SerializeField] private float shootForceMultiplier = 100;
        [SerializeField] private float powerSkillForce = 30000f;
        [SerializeField] private float powerSkillMass = 200f;
        [SerializeField] private float skillPointIncrease = .1f;
        #endregion

        #region Fields
        private SkillData powerSkill;
        private SkillData evolveSkill;
        private SkillData destroySkill;
        private AudioSource audioSource;

        private readonly WaitForSeconds massResetWaitTime = new(2);
        #endregion
        
        #region Properties
        public static SkillController Instance { get; private set; }
        public ReadOnlyDictionary<Skill, SkillData> SkillPointRequirementsMap { get; private set; }
        public float ShootForceMultiplier => this.shootForceMultiplier;
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;
            
            this.powerSkill = InitializeSkill(this.power, KeyCode.Alpha1, Skill.Power, this.powerPointsRequirement);
            this.evolveSkill = InitializeSkill(this.evolve, KeyCode.Alpha2, Skill.Evolve, this.evolvePointsRequirement);
            this.destroySkill = InitializeSkill(this.destroy, KeyCode.Alpha3, Skill.Destroy, this.destroyPointsRequirement);
            this.audioSource = this.GetComponent<AudioSource>();

            this.SkillPointRequirementsMap = new ReadOnlyDictionary<Skill, SkillData>(new Dictionary<Skill, SkillData>
            {
                { Skill.Power, this.powerSkill },
                { Skill.Evolve, this.evolveSkill },
                { Skill.Destroy, this.destroySkill }
            });
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (this.forceEnableSkills)
            {
                this.powerSkill.EnableSkill();
                this.evolveSkill.EnableSkill();
                this.destroySkill.EnableSkill();   
            }
#endif
        }

        private void Update()
        {
            this.SkillInput(this.powerSkill);
            this.SkillInput(this.evolveSkill);
            this.SkillInput(this.destroySkill);
        }

        private static SkillData InitializeSkill(GameObject _GameObject, KeyCode _KeyToActivate, Skill _Skill, uint _PointRequirements)
        {
            var _textMeshPro = _GameObject.GetComponentInChildren<TextMeshProUGUI>();
            var _spriteRenderers = _GameObject.GetComponentsInChildren<SpriteRenderer>();
            
            _spriteRenderers[0].gameObject.SetActive(false);
            
            return new SkillData(_textMeshPro, _spriteRenderers[0], _spriteRenderers[1], _KeyToActivate, _Skill, _PointRequirements);
        }

        public void ApplyWebSettings()
        {
            TrySetValue(nameof(this.powerPointsRequirement), ref this.powerPointsRequirement);
            TrySetValue(nameof(this.evolvePointsRequirement), ref this.evolvePointsRequirement);
            TrySetValue(nameof(this.destroyPointsRequirement), ref this.destroyPointsRequirement);
            TrySetValue(nameof(this.skillPointIncrease), ref this.skillPointIncrease);
        }
        
        public void PointsChanged(uint _CurrentPoints)
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
        /// Handles the input for when a skill button is pressed
        /// </summary>
        /// <param name="_SkillToActivate">The <see cref="Skill"/> button that was pressed</param>
        private void SkillInput(SkillData _SkillToActivate)
        {
            if (Input.GetKeyDown(_SkillToActivate.KeyToActivate) && _SkillToActivate.CanBeActivated)
            {
                this.audioSource.Play();
                
                if (!_SkillToActivate.IsActive)
                {
                    this.DeactivateActiveSkill(false);
                    
                    _SkillToActivate.ActivateSkill();
                }
                else
                {
                    _SkillToActivate.DeactivateSkill(false);
                    FruitSpawner.ResetAimRotation();
                }
            }
        }

        /// <summary>
        /// Deactivates the currently active skill
        /// </summary>
        /// <param name="_OnlyVisuals">If false, also deactivates the skill on the <see cref="FruitBehaviour"/></param>
        public void DeactivateActiveSkill(bool _OnlyVisuals)
        {
            this.powerSkill.DeactivateSkill(_OnlyVisuals);
            this.evolveSkill.DeactivateSkill(_OnlyVisuals);
            this.destroySkill.DeactivateSkill(_OnlyVisuals);
        }
        
        /// <summary>
        /// Shoots the fruit with enhanced force and mass
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to set the mass of</param>
        /// <param name="_Direction">The direction to shoot the fruit in</param>
        public void Skill_Power(FruitBehaviour _FruitBehaviour, Vector2 _Direction)
        {
            _FruitBehaviour.SetMass(false, this.powerSkillMass);
            _FruitBehaviour.Rigidbody2D.AddForce(_Direction * this.powerSkillForce, ForceMode2D.Impulse);
            StartCoroutine(this.ResetMass(_FruitBehaviour));
            
            GameOverMenu.Instance.AddSkillCount(Skill.Power);
            StatsMenu.Instance.AddSkillCount(Skill.Power);
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
                _FruitBehaviour.SetMass(true, 0);
            }
        }

        public void Skill_Evolve(FruitBehaviour _FruitBehaviour)
        {
            // TODO: Combine this method with the "FruitCollision()"-method in "GameController.cs" 
            
            var _position = _FruitBehaviour.transform.position;
            var _fruitIndex = (int)Enum.GetValues(typeof(Fruit.Fruit)).Cast<Fruit.Fruit>().FirstOrDefault(_Fruit => _Fruit == _FruitBehaviour.Fruit);
                    
            PointsController.Instance.AddPoints((Fruit.Fruit)_fruitIndex);
            GameController.Instance.FruitCollection.PlayEvolveSound();
            
            _FruitBehaviour.Destroy();
            
            // Nothing has to be spawned after a melon is evolved
            if (_fruitIndex != (int)Fruit.Fruit.Melon)
            {
                var _fruit = GameController.Instance.FruitCollection.Fruits[_fruitIndex + 1].Fruit;
                var _fruitBehaviour = FruitBehaviour.SpawnFruit(_position, _fruit, true);
                _fruitBehaviour.Evolve();
            }
            
            GameOverMenu.Instance.AddSkillCount(Skill.Evolve);
            StatsMenu.Instance.AddSkillCount(Skill.Evolve);
        }

        public void Skill_Destroy(FruitBehaviour _FruitBehaviour)
        {
            _FruitBehaviour.Destroy();
            GameController.Instance.FruitCollection.PlayEvolveSound();
            GameOverMenu.Instance.AddSkillCount(Skill.Destroy);
            StatsMenu.Instance.AddSkillCount(Skill.Destroy);
        }

        public void SkillUsed(Skill _Skill)
        {
            var _skill = SkillPointRequirementsMap[_Skill];

            PointsController.Instance.SubtractPoints(_skill.CurrentPointsRequirement);

            _skill.CurrentPointsRequirement += (uint)(_skill.CurrentPointsRequirement * this.skillPointIncrease);
        }

        public void ResetSkillPointsRequirement()
        {
            this.powerSkill.CurrentPointsRequirement = this.powerPointsRequirement;
            this.evolveSkill.CurrentPointsRequirement = this.evolvePointsRequirement;
            this.destroySkill.CurrentPointsRequirement = this.destroyPointsRequirement;
        }
        #endregion
    }
}