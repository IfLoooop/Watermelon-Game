using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon_Game.Menus.Utility
{
    /// <summary>
    /// Helper class to auto deselect a button after it has been clicked 
    /// </summary>
    internal sealed class ButtonDeselect : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// <see cref="Button"/>
        /// </summary>
        private Button button;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.button = base.GetComponent<Button>();
        }

        private void OnEnable()
        {
            this.button.onClick.AddListener(this.OnClick);
        }

        private void OnDisable()
        {
            this.button.onClick.RemoveListener(this.OnClick);
        }

        /// <summary>
        /// Deselects this <see cref="button"/> after it has been clicked
        /// </summary>
        private void OnClick()
        {
            if (EventSystem.current.currentSelectedGameObject == this.button.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        #endregion
    }
}