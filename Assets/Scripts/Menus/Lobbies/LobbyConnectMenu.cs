using System.Collections;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Networking;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu to connect to a steam lobby
    /// </summary>
    internal sealed class LobbyConnectMenu : MenuBase
    {
        #region Inspector Fields
        [Tooltip("Reference to the LobbyJoinMenu")]
        [PropertyOrder(1)][SerializeField] private LobbyJoinMenu lobbyJoinMenu;

        [Header("Settings")]
        [Tooltip("Connection to the lobby will be closed after this amount")]
        [PropertyOrder(2)][SerializeField] private ProtectedFloat timeoutInSeconds = 10;
        #endregion

        #region Fields
        /// <summary>
        /// Will be true after the connection button was pressed -> <see cref="Connect"/>
        /// </summary>
        private ProtectedBool connecting;
        /// <summary>
        /// Stores the <see cref="Timeout"/> coroutine
        /// </summary>
        [CanBeNull] private IEnumerator timeout;
        #endregion
        
        #region Properties
        /// <summary>
        /// Indicates whether this menu is currently open or not
        /// </summary>
        public ProtectedBool IsOpen { get; private set; }
        #endregion
        
        #region Methods
        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            this.IsOpen = true;
            this.connecting = false;
            this.lobbyJoinMenu.KeepOpen(true);
            this.timeout = this.Timeout();
            base.StartCoroutine(this.timeout);
            return base.Open(_CurrentActiveMenu);
        }

        public override void Close(bool _PlaySound)
        {
            this.Cancel();
        }

        /// <summary>
        /// Connects the client to the steam lobby
        /// </summary>
        public void Connect()
        {
            this.connecting = true;
            this.Reset();
            CustomNetworkManager.Connect();
            base.Close(false);
            MenuController.CloseCurrentMenu(true);
        }

        /// <summary>
        /// Resets the values to their initial state
        /// </summary>
        private void Reset()
        {
            base.StopCoroutine(this.timeout);
            this.timeout = null;
            this.IsOpen = false;
            this.lobbyJoinMenu.KeepOpen(false);
            LobbyJoinMenu.AllowJoinButtonInteraction(true);
        }
        
        /// <summary>
        /// Cancels the connection attempt
        /// </summary>
        public void Cancel()
        {
            if (!this.IsOpen)
            {
                return;
            }
            if (this.connecting)
            {
                return;
            }
            
            this.Reset();
            SteamLobby.LeaveLobby();
            CustomNetworkManager.CancelConnectionAttempt();
            base.Close(true);
        }

        /// <summary>
        /// Cancels the connection to the lobby after <see cref="timeoutInSeconds"/>
        /// </summary>
        /// <returns></returns>
        private IEnumerator Timeout()
        {
            var _waitTime = new WaitForSeconds(1);
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < this.timeoutInSeconds; i++)
            {
                yield return _waitTime;
            }
            
            this.Cancel();
        }
        #endregion
    }
}