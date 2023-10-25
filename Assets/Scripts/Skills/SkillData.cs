using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillData
    {
        #region Fields
        private readonly SkillReferences skillReferences;
        #endregion

        #region Fields
        private readonly Skill skill;
        private uint currentPointsRequirement;
        #endregion
        
        #region Properties
        public uint CurrentPointsRequirement
        {
            get => this.currentPointsRequirement;
            set
            {
                this.currentPointsRequirement = value;
                this.SetSkillPointRequirementText();
            }
        }
        public bool CanBeActivated { get; private set; }
        public bool IsActive { get; private set; }
        public KeyCode KeyToActivate { get; }
        #endregion

        #region Constrcutor
        public SkillData(SkillReferences _SkillReferences, KeyCode _KeyToActivate, Skill _Skill, uint _PointRequirement)
        {
            this.skillReferences = _SkillReferences;
            this.KeyToActivate = _KeyToActivate;
            this.skill = _Skill;
            this.CurrentPointsRequirement = _PointRequirement;
        }
        #endregion

        #region Methos
        /// <summary>
        /// Sets the visible skill point requirements in <see cref="skillReferences"/>
        /// </summary>
        private void SetSkillPointRequirementText()
        {
            this.skillReferences.PointCost.text = string.Concat(this.CurrentPointsRequirement, "P");
        }

        /// <summary>
        /// Makes this skill selectable
        /// </summary>
        public void EnableSkill()
        {
            this.CanBeActivated = true;
            this.skillReferences.ButtonImage.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Makes this skill unselectable
        /// </summary>
        public void DisableSkill()
        {
            this.CanBeActivated = false;
            this.skillReferences.ButtonImage.gameObject.SetActive(false);
            this.DeactivateSkill(true);
        }

        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        public void ActivateSkill()
        {
            this.IsActive = true;
            this.skillReferences.SkillIconImage.color = this.skillReferences.SkillIconImage.color.WithAlpha(1);
            FruitSpawnerAim.Instance.AllowAimRotation(true);
            FruitSpawner.SetActiveSkill(this.skill);
        }

        /// <summary>
        /// Deactivates the currently active skills
        /// </summary>
        /// <param name="_OnlyVisuals">If false, also deactivates the skill on the <see cref="FruitBehaviour"/></param>
        public void DeactivateSkill(bool _OnlyVisuals)
        {
            this.IsActive = false;
            this.skillReferences.SkillIconImage.color = this.skillReferences.SkillIconImage.color.WithAlpha(0.5f);
            FruitSpawnerAim.Instance.AllowAimRotation(false);
            
            if (!_OnlyVisuals)
            {
                FruitSpawner.DeactivateSkill();   
            }
        }
        #endregion
    }
}