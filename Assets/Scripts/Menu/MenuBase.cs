using UnityEngine;

namespace Watermelon_Game.Menu
{
    internal class MenuBase : MonoBehaviour
    {
        #region Fieds
        [SerializeField] private Menu menu;
        #endregion

        #region Properties
        public Menu Menu => this.menu;
        #endregion

        #region Methods
        public void Open_Close()
        {
            var _active = this.gameObject.activeSelf;
            this.gameObject.SetActive(!_active);
        }
        #endregion
    }
}