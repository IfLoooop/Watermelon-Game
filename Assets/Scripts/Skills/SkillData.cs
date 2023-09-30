using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillData
    {
        #region Fields
        private readonly TextMeshProUGUI textMeshPro;
        private readonly SpriteRenderer enabledSpriteRenderer;
        private readonly SpriteRenderer activeSpriteRenderer;
        #endregion

        #region Fields
        private readonly Skill skill;
        #endregion
        
        #region Properties
        public bool CanBeActivated { get; private set; }
        public bool IsActive { get; private set; }
        public KeyCode KeyToActivate { get; }
        #endregion

        #region Constrcutor
        public SkillData(TextMeshProUGUI _TextMeshPro, SpriteRenderer _EnabledSpriteRenderer, SpriteRenderer _ActiveSpriteRenderer, KeyCode _KeyToActivate, Skill _Skill)
        {
            this.textMeshPro = _TextMeshPro;
            this.enabledSpriteRenderer = _EnabledSpriteRenderer;
            this.activeSpriteRenderer = _ActiveSpriteRenderer;
            this.KeyToActivate = _KeyToActivate;
            this.skill = _Skill;
        }
        #endregion

        #region Methos
        /// <summary>
        /// Sets the visible skill point requirements in <see cref="textMeshPro"/>
        /// </summary>
        /// <param name="_Value">How much points are needed to use this skill</param>
        public void SetSkillPointRequirements(uint _Value)
        {
            this.textMeshPro.text = string.Concat(_Value, "P");
        }

        /// <summary>
        /// Makes this skill selectable
        /// </summary>
        public void EnableSkill()
        {
            this.CanBeActivated = true;
            this.enabledSpriteRenderer.color = this.enabledSpriteRenderer.color.WithAlpha(1f);
        }
        
        /// <summary>
        /// Makes this skill unselectable
        /// </summary>
        public void DisableSkill()
        {
            this.CanBeActivated = false;
            this.enabledSpriteRenderer.color = this.enabledSpriteRenderer.color.WithAlpha(.5f);
        }

        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        public void ActivateSkill()
        {
            this.IsActive = true;
            this.activeSpriteRenderer.enabled = true;
            SkillController.Instance.SetAimRotation(true);
            FruitSpawner.SetActiveSkill(this.skill);
        }

        /// <summary>
        /// Deactivates the currently active skills
        /// </summary>
        /// <param name="_OnlyVisuals">If false, also deactivates the skill on the <see cref="FruitBehaviour"/></param>
        public void DeactivateSkill(bool _OnlyVisuals)
        {
            this.IsActive = false;
            this.activeSpriteRenderer.enabled = false;
            SkillController.Instance.SetAimRotation(false);
            FruitSpawner.ResetAimRotation();
            if (!_OnlyVisuals)
            {
                FruitSpawner.DeactivateSkill();   
            }
        }
        #endregion
    }
}