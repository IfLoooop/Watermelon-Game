using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Audio;

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

        #region Events
        /// <summary>
        /// Is called whenever the <see cref="Close"/>-Method of <see cref="MenuBase"/> is called <br/>
        /// <b>Parameter:</b> The instance of the <see cref="MenuBase"/> that was closed
        /// </summary>
        public static event Action<MenuBase> OnMenuClose; 
        #endregion
        
        #region Methods
        /// <summary>
        /// Closes the given <see cref="MenuBase"/> and opens this one
        /// </summary>
        /// <param name="_CurrentActiveMenu">The <see cref="MenuBase"/> to close</param>
        /// <returns>This <see cref="MenuBase"/></returns>
        public virtual MenuBase Open([CanBeNull] MenuBase _CurrentActiveMenu)
        {
            if (_CurrentActiveMenu != null)
            {
                if (_CurrentActiveMenu.menu != this.menu)
                {
                    _CurrentActiveMenu!.ForceClose(true);
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
        /// <param name="_PlaySound">Should the menu sound be played</param>
        public virtual void Close(bool _PlaySound)
        {
            if (_PlaySound)
            {
                AudioPool.PlayClip(AudioClipName.MenuPopup);
            }
            
            base.transform.localScale = Vector3.zero;
            OnMenuClose?.Invoke(this);
        }

        /// <summary>
        /// Use this to force close a menu and all its sub menus
        /// </summary>
        /// <param name="_PlaySound">Should the menu sound be played</param>
        public virtual void ForceClose(bool _PlaySound)
        {
            this.Close(_PlaySound);
        }
        
        /// <summary>
        /// Is called at the end of the <see cref="menuPopup"/> animation
        /// </summary>
        public virtual void OnAnimationFinished() { }
        #endregion
    }
}