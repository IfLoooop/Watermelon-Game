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
        #endregion
        
        #region Properties
        public bool CanBeActivated { get; private set; }
        public bool IsActive { get; private set; }
        public KeyCode KeyToActivate { get; }
        #endregion

        #region Constrcutor
        public SkillData(TextMeshProUGUI _TextMeshPro, SpriteRenderer _ButtonSpriteRenderer, SpriteRenderer _SkillIconSpriteRenderer, KeyCode _KeyToActivate, Skill _Skill)
        {
            this.textMeshPro = _TextMeshPro;
            this.buttonSpriteRenderer = _ButtonSpriteRenderer;
            this.skillIconSpriteRenderer = _SkillIconSpriteRenderer;
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
            this.buttonSpriteRenderer.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Makes this skill unselectable
        /// </summary>
        public void DisableSkill()
        {
            this.CanBeActivated = false;
            this.buttonSpriteRenderer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        public void ActivateSkill()
        {
            this.IsActive = true;
            this.skillIconSpriteRenderer.color = this.skillIconSpriteRenderer.color.WithAlpha(1);
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
            this.skillIconSpriteRenderer.color = this.skillIconSpriteRenderer.color.WithAlpha(0.5f);
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