using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Watermelon_Game.Menus.Dropdown
{
    /// <summary>
    /// Hides the dropdown list on mouse exit
    /// </summary>
    internal sealed class DropdownTemplate : MonoBehaviour, IPointerExitHandler
    {
        #region Inspector Fields
        [FormerlySerializedAs("dropdown")]
        [Header("References")]
        [Tooltip("Reference to the Dropdown component")]
        [SerializeField] private DropdownBase dropdownBase;
        #endregion
        
        #region Methods
        public void OnPointerExit(PointerEventData _EventData)
        {
            this.dropdownBase.Hide();
        }
        #endregion
    }
}