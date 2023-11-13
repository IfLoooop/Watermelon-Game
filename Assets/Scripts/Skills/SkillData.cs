using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Skills
{
    /// <summary>
    /// Contains data for a <see cref="Skills.Skill"/>
    /// </summary>
    internal sealed class SkillData
    {
        #region Fields
        private readonly SkillReferences skillReferences;
        #endregion

        #region Fields
        /// <summary>
        /// The currently needed points to activate this <see cref="Skill"/>
        /// </summary>
        private ProtectedUInt32 currentPointsRequirement;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="Skill"/>
        /// </summary>
        public Skill Skill { get; }
        /// <summary>
        /// The <see cref="KeyCode"/> that needs to be pressed, to activate this <see cref="Skill"/>
        /// </summary>
        public KeyCode KeyToActivate { get; }
        /// <summary>
        /// <see cref="currentPointsRequirement"/>
        /// </summary>
        public ProtectedUInt32 CurrentPointsRequirement
        {
            get => this.currentPointsRequirement;
            set
            {
                this.currentPointsRequirement = value;
                this.SetSkillPointRequirementText();
            }
        }
        /// <summary>
        /// Whether this <see cref="Skill"/> can be activated or not
        /// </summary>
        public ProtectedBool CanBeActivated { get; private set; }
        /// <summary>
        /// Indicates if this <see cref="Skill"/> is currently active or not
        /// </summary>
        public ProtectedBool IsActive { get; private set; }
        #endregion
        
        #region Constrcutor>
        /// <param name="_SkillReferences"><see cref="SkillReferences"/></param>
        /// <param name="_KeyToActivate"><see cref="KeyToActivate"/></param>
        /// <param name="_Skill"><see cref="Skill"/></param>
        /// <param name="_PointsRequirement"><see cref="currentPointsRequirement"/></param>
        public SkillData(SkillReferences _SkillReferences, KeyCode _KeyToActivate, Skill _Skill, uint _PointsRequirement)
        {
            this.skillReferences = _SkillReferences;
            this.KeyToActivate = _KeyToActivate;
            this.Skill = _Skill;
            this.CurrentPointsRequirement = _PointsRequirement;
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
            this.DeactivateSkill();
        }

        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        public void ActivateSkill()
        {
            this.IsActive = true;
            this.skillReferences.SkillIconImage.color = this.skillReferences.SkillIconImage.color.WithAlpha(1);
        }

        /// <summary>
        /// Deactivates the currently active skills
        /// </summary>
        public void DeactivateSkill()
        {
            this.IsActive = false;
            this.skillReferences.SkillIconImage.color = this.skillReferences.SkillIconImage.color.WithAlpha(0.5f);
        }

        /// <summary>
        /// Plays the <see cref="SkillReferences.SkillPointsIncrease"/> <see cref="Animation"/>
        /// </summary>
        public void PlayAnimation()
        {
            this.skillReferences.SkillPointsIncrease.Play();
        }
        #endregion
    }
}