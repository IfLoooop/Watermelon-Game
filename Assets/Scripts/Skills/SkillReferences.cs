using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [Tooltip("Reference to a TMP component that displays the point cost of a skill")]
        [SerializeField] private TextMeshProUGUI pointCost;
        [Tooltip("Reference to an Image component, that displays the button that needs to be pressed, to activate the skill")]
        [SerializeField] private Image buttonImage;
        [Tooltip("Reference to the animation that plays when the skill points requirements increase")]
        [SerializeField] private Animation skillPointsIncrease;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="skillIconImage"/>
        /// </summary>
        public Image SkillIconImage => this.skillIconImage;
        /// <summary>
        /// <see cref="pointCost"/>
        /// </summary>
        public TextMeshProUGUI PointCost => this.pointCost;
        /// <summary>
        /// <see cref="buttonImage"/>
        /// </summary>
        public Image ButtonImage => this.buttonImage;
        /// <summary>
        /// <see cref="skillPointsIncrease"/>
        /// </summary>
        public Animation SkillPointsIncrease => this.skillPointsIncrease;

        #endregion
    }
}