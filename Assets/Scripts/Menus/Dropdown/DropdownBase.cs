using System;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon_Game.Menus.Dropdown
{
    /// <summary>
    /// Base dropdown class
    /// </summary>
    internal class DropdownBase : TMP_Dropdown
    {
        #region Fields
        /// <summary>
        /// The currently active <see cref="Toggle"/>
        /// </summary>
        protected string CurrentActiveToggle = string.Empty;
        #endregion

        #region Methods
        /// <summary>
        /// Is called on the "Item" GameObject on "OnValueChanged" in the dropdown
        /// </summary>
        /// <param name="_Toggle">The <see cref="Toggle"/> that was selected</param>
        // ReSharper disable once UnusedMemberInSuper.Global
        public virtual void OnToggleChange(Toggle _Toggle)
        {
            this.OnToggleChange(_Toggle.name, _Toggle.isOn);
        }
        
        /// <summary>
        /// Sets <see cref="CurrentActiveToggle"/> to the given <see cref="Toggle"/>
        /// </summary>
        /// <param name="_ToggleName">The name of the <see cref="Toggle"/></param>
        /// <param name="_IsOn">Indicates whether the <see cref="Toggle"/> is currently on or not</param>
        /// <param name="_CustomLogic">Custom logic to run at the end of this method</param>
        protected void OnToggleChange(string _ToggleName, bool _IsOn, [CanBeNull] Action _CustomLogic = null)
        {
            var _isNotTheCurrentlyActiveToggle = string.IsNullOrWhiteSpace(this.CurrentActiveToggle) || !_ToggleName.Contains(this.CurrentActiveToggle);
            if (_isNotTheCurrentlyActiveToggle && _IsOn)
            {
                this.CurrentActiveToggle = base.options.First(_OptionData => _ToggleName.Contains(_OptionData.text)).text;
                this.Hide();
                
                _CustomLogic?.Invoke();
            }
        }
        
        /// <summary>
        /// Disables the dropdown menu
        /// </summary>
        public new void Hide()
        {
            base.Hide();
            EventSystem.current.SetSelectedGameObject(null);
            base.OnDeselect(null);
        }
        #endregion
    }
}