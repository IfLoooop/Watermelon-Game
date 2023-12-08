using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Skills
{
    /// <summary>
    /// Contains references for a <see cref="Skill"/> in the scene
    /// </summary>
    internal sealed class SkillReferences : MonoBehaviour
    {
        #region Inspector Fields
        [Tooltip("Reference to the Image component of a skill")]
        [SerializeField] private Image skillIconImage;
        [Tooltip("Glow for when the Skill is activated")]
        [SerializeField] private GameObject glow;
        [Tooltip("Reference to a TMP component that displays the point cost of a skill")]
        [SerializeField] private TextMeshProUGUI pointCost;
        [Tooltip("Container for the input icons")]
        [SerializeField] private GameObject input;
        [Tooltip("Image component that displays the mouse button")]
        [SerializeField] private Image mouseButtonImage;
        [Tooltip("Image component that displays the mouse wheel")]
        [SerializeField] private Image mouseWheelImage;
        [Tooltip("Reference to the animation that plays when the skill points requirements increase")]
        [SerializeField] private Animation skillPointsIncrease;
        #endregion

        #region Fields
        /// <summary>
        /// Original scale of this skill icon
        /// </summary>
        private Vector3 skillIconScale;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="pointCost"/>
        /// </summary>
        public TextMeshProUGUI PointCost => this.pointCost;
        /// <summary>
        /// <see cref="mouseButtonImage"/>
        /// </summary>
        public Image MouseButtonImage => this.mouseButtonImage;
        /// <summary>
        /// <see cref="mouseWheelImage"/>
        /// </summary>
        public Image MouseWheelImage => this.mouseWheelImage;
        /// <summary>
        /// <see cref="skillPointsIncrease"/>
        /// </summary>
        public Animation SkillPointsIncrease => this.skillPointsIncrease;
        #endregion

        #region Methods
        private void Awake()
        {
            this.skillIconScale = this.skillIconImage.transform.localScale;
        }

        /// <summary>
        /// Makes this skill selectable
        /// </summary>
        public void EnableSkill()
        {
            this.skillIconImage.color = this.skillIconImage.color.WithAlpha(1);
            this.input.SetActive(true);
            this.mouseButtonImage.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Makes this skill selectable
        /// </summary>
        public void DisableSkill()
        {
            this.skillIconImage.color = this.skillIconImage.color.WithAlpha(.5f);
            this.input.SetActive(false);
            this.mouseButtonImage.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Sets this as the currently active skill
        /// </summary>
        public void ActivateSkill()
        {
            this.glow.SetActive(true);
            this.mouseWheelImage.gameObject.SetActive(true);
            this.mouseButtonImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivates the currently active skills
        /// </summary>
        public void DeactivateSkill()
        {
            this.skillIconImage.transform.localScale = this.skillIconScale;
            this.glow.SetActive(false);
            this.mouseWheelImage.gameObject.SetActive(false);
            this.mouseButtonImage.gameObject.SetActive(true);
        }
        #endregion
    }
}