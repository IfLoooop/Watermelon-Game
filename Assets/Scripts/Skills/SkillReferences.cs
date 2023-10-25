using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Skills
{
    internal sealed class SkillReferences : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Image skillIconImage;
        [SerializeField] private TextMeshProUGUI pointCost;
        [SerializeField] private Image buttonImage;
        #endregion

        #region Properties
        public Image SkillIconImage => this.skillIconImage;
        public TextMeshProUGUI PointCost => this.pointCost;
        public Image ButtonImage => this.buttonImage;
        #endregion
    }
}