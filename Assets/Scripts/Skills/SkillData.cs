using TMPro;
using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillData
    {
        #region Fields
        private readonly TextMeshProUGUI textMeshPro;
        private readonly SpriteRenderer buttonSpriteRenderer;
        private readonly SpriteRenderer skillIconSpriteRenderer;
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
        public SkillData(TextMeshProUGUI _TextMeshPro, SpriteRenderer _ButtonSpriteRenderer, SpriteRenderer _SkillIconSpriteRenderer, KeyCode _KeyToActivate, Skill _Skill, uint _PointRequirement)
        {
            this.textMeshPro = _TextMeshPro;
            this.buttonSpriteRenderer = _ButtonSpriteRenderer;
            this.skillIconSpriteRenderer = _SkillIconSpriteRenderer;
            this.KeyToActivate = _KeyToActivate;
            this.skill = _Skill;
            this.CurrentPointsRequirement = _PointRequirement;
        }
        #endregion

        #region Methos
        /// <summary>
        /// Sets the visible skill point requirements in <see cref="textMeshPro"/>
        /// </summary>
        private void SetSkillPointRequirementText()
        {
            this.textMeshPro.text = string.Concat(this.CurrentPointsRequirement, "P");
        }

        /// <summary>
        /// Makes this skill selectable
        /// </summary>
        public void EnableSkill()
        {
            this.CanBeActivated = true;
            this.buttonSpriteRenderer.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Makes this skill unselectable
        /// </summary>
        public void DisableSkill()
        {
            this.CanBeActivated = false;
            this.buttonSpriteRenderer.gameObject.SetActive(false);
            this.DeactivateSkill(true);
        }

        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        public void ActivateSkill()
        {
            this.IsActive = true;
            this.skillIconSpriteRenderer.color = this.skillIconSpriteRenderer.color.WithAlpha(1);
            FruitSpawnerAim.Instance.SetAimRotation(true);
            FruitSpawner.SetActiveSkill(this.skill);
        }

        /// <summary>
        /// Deactivates the currently active skills
        /// </summary>
        /// <param name="_OnlyVisuals">If false, also deactivates the skill on the <see cref="FruitBehaviour"/></param>
        public void DeactivateSkill(bool _OnlyVisuals)
        {
            this.IsActive = false;
            this.skillIconSpriteRenderer.color = this.skillIconSpriteRenderer.color.WithAlpha(0.5f);
            FruitSpawnerAim.Instance.SetAimRotation(false);
            FruitSpawner.ResetAimRotation();
            if (!_OnlyVisuals)
            {
                FruitSpawner.DeactivateSkill();   
            }
        }
        #endregion
    }
}