using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Container;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Base class for every <see cref="Menus.Menu"/>
    /// </summary>
    [RequireComponent(typeof(Animation))]
    internal abstract class MenuBase : SerializedMonoBehaviour
    {
        #region Inspector Fieds
        [Header("References")]
        [Tooltip("Open animation for the menu")]
        [SerializeField] private Animation menuPopup;
        
        [Header("Settings")]
        [Tooltip("The type of the menu")]
        [SerializeField] private Menu menu;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="menu"/>
        /// </summary>
        public Menu Menu => this.menu;
        #endregion
        
        #region Methods
        /// <summary>
        /// Closes the given <see cref="MenuBase"/> and opens this one
        /// </summary>
        /// <param name="_CurrentActiveMenu">The <see cref="MenuBase"/> to close</param>
        /// <returns>This <see cref="MenuBase"/></returns>
        public virtual MenuBase Open([CanBeNull] MenuBase _CurrentActiveMenu)
        {
            // TODO: Not sure if needed
            // if (ContainerBounds.GetOwnContainer() is {} _container)
            // {
            //     var _rectPoint = CameraUtils.WorldPointToLocalPointInRectangle(MenuController.Canvas, _container.transform.position);
            //     var _rectTransform = (base.transform as RectTransform)!;
            //     _rectTransform.anchoredPosition = new Vector2(_rectPoint.x, _rectTransform.anchoredPosition.y);   
            // }
            
            if (_CurrentActiveMenu != null)
            {
                if (_CurrentActiveMenu.menu != this.menu)
                {
                    _CurrentActiveMenu!.Close();
                }
                else
                {
                    return this;
                }
            }
            else
            {
                AudioPool.PlayClip(AudioClipName.MenuPopup);
            }

            this.menuPopup.Play();
            return this;
        }

        /// <summary>
        /// Closes this menu
        /// </summary>
        /// <returns>Always returns null</returns>
        [CanBeNull]
        public virtual MenuBase Close()
        {
            AudioPool.PlayClip(AudioClipName.MenuPopup);
            base.transform.localScale = Vector3.zero;
            
            return null;
        }
        
        /// <summary>
        /// Is called at the end of the <see cref="menuPopup"/> animation
        /// </summary>
        public virtual void OnAnimationFinished() { }
        #endregion
    }
}