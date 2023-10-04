using JetBrains.Annotations;
using UnityEngine;

namespace Watermelon_Game.Menu
{
    internal sealed class MenuController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private StatsMenu statsMenu;
        [SerializeField] private SettingsMenu settingsMenu;
        #endregion

        #region Fields
        [CanBeNull] private MenuBase currentActiveMenu;
        #endregion

        #region Methods

        private void Awake()
        {
            this.InitializeMenu(this.statsMenu);
            this.InitializeMenu(this.settingsMenu);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                this.OpenMenu(this.statsMenu);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                this.OpenMenu(this.settingsMenu);
            }
        }

        private void OnApplicationQuit()
        {
            this.statsMenu.Save();
        }

        /// <summary>
        /// Needed to set the "Instance" properties in the Menu class
        /// </summary>
        /// <param name="_Menu">The menu to initialize</param>
        private void InitializeMenu(MenuBase _Menu)
        {
            _Menu.gameObject.SetActive(true);
            _Menu.gameObject.SetActive(false);
        }
        
        private void OpenMenu(MenuBase _Menu)
        {
            if (this.currentActiveMenu == null)
            {
                this.currentActiveMenu = _Menu;
            }
            
            this.currentActiveMenu!.Open_Close();
            
            if (this.currentActiveMenu.Menu != _Menu.Menu)
            {
                _Menu.Open_Close();
                this.currentActiveMenu = _Menu;   
            }
        }
        #endregion
    }
}