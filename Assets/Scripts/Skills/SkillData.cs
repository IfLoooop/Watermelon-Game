using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Points;

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

            SkillController.OnSkillActivated += SkillActivated;
        }

        ~SkillData()
        {
            SkillController.OnSkillActivated -= SkillActivated;
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
            this.skillReferences.EnableSkill();
        }
        
        /// <summary>
        /// Makes this skill unselectable
        /// </summary>
        public void DisableSkill()
        {
            this.CanBeActivated = false;
            this.skillReferences.DisableSkill();
            this.DeactivateSkill();
        }

        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        private void ActivateSkill()
        {
            this.IsActive = true;
            this.skillReferences.ActivateSkill();
        }

        /// <summary>
        /// Deactivates the currently active skills
        /// </summary>
        public void DeactivateSkill()
        {
            this.IsActive = false;
            this.skillReferences.DeactivateSkill();
        }

        /// <summary>
        /// Plays the <see cref="SkillReferences.SkillPointsIncrease"/> <see cref="Animation"/>
        /// </summary>
        public void PlayAnimation()
        {
            this.skillReferences.SkillPointsIncrease.Play();
        }

        /// <summary>
        /// Is called on <see cref="SkillController.OnSkillActivated"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skills.Skill"/> that was activated</param>
        private void SkillActivated(Skill? _Skill)
        {
            // Deactivate skill
            if (_Skill is null)
            {
                this.DeactivateSkill();
                if (PointsController.CurrentPoints >= this.currentPointsRequirement)
                {
                    this.skillReferences.MouseButtonImage.gameObject.SetActive(true);
                    this.skillReferences.MouseWheelImage.gameObject.SetActive(false);
                }
            }
            // Currently selected skill
            else if (_Skill == this.Skill)
            {
                this.ActivateSkill();
            }
            else
            {
                this.DeactivateSkill();
                if (PointsController.CurrentPoints >= this.currentPointsRequirement)
                {
                    this.skillReferences.MouseWheelImage.gameObject.SetActive(true);
                    this.skillReferences.MouseButtonImage.gameObject.SetActive(false);
                }
            }
        }
        #endregion
    }
}