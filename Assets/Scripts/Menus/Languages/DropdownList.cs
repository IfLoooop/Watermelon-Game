using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon_Game.Menus.Languages
{
    /// <summary>
    /// Contains logic for the dropdown menu
    /// </summary>
    internal sealed class DropdownList : MonoBehaviour, IPointerExitHandler
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the LanguageController of this Dropdown list")]
        [SerializeField] private LanguageController languageController;
        #endregion
        
        #region Methods
        public void OnPointerExit(PointerEventData _EventData)
        {
            this.languageController.Hide();
        }
        #endregion
    }
}