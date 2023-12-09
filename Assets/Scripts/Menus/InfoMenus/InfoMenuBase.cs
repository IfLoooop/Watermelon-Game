using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using Watermelon_Game.Audio;
using Watermelon_Game.Utility.Pools;

namespace Watermelon_Game.Menus.InfoMenus
{
    /// <summary>
    /// Base calls for all info menus
    /// </summary>
    [RequireComponent(typeof(LocalizeStringEvent))]
    internal abstract class InfoMenuBase : MenuBase
    {
        #region Constants
        /// <summary>
        /// Name of the info menu table
        /// </summary>
        private const string TABLE_NAME = "InfoMenu";
        #endregion
        
        #region Fields
        /// <summary>
        /// <see cref="LocalizeStringEvent"/>
        /// </summary>
        private LocalizeStringEvent localizeStringEvent;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.localizeStringEvent = base.GetComponent<LocalizeStringEvent>();
        }

        /// <summary>
        /// Closes this menu
        /// </summary>
        /// <param name="_PlaySound">Should the menu sound be played</param>
        /// <returns>Always returns null</returns>
        [CanBeNull]
        public new virtual InfoMenuBase Close(bool _PlaySound)
        {
            if (_PlaySound)
            {
                AudioPool.PlayClip(AudioClipName.MenuPopup);
            }
            
            base.transform.localScale = Vector3.zero;
            return null;
        }
        
        /// <summary>
        /// Sets the <see cref="LocalizeStringEvent.StringReference"/> of <see cref="localizeStringEvent"/> to the key that matches the given <see cref="InfoMessage"/>
        /// </summary>
        /// <param name="_Message">Must be the exact name as a key in the table: <see cref="TABLE_NAME"/></param>
        /// <returns>This <see cref="InfoMenuBase"/></returns>
        public InfoMenuBase SetMessage(InfoMessage _Message)
        {
            var _stringTable = LocalizationSettings.StringDatabase.GetTable(TABLE_NAME);
            if (_stringTable.SharedData.Entries.FirstOrDefault(_Entry => _Entry.Key == _Message.ToString()) is {} _sharedTableEntry)
            {
                this.localizeStringEvent.StringReference.SetReference(TABLE_NAME, _sharedTableEntry.Key);
            }
            return this;
        }
        #endregion
    }
}