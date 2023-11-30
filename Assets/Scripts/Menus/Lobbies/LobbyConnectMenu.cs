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
        #endregion

        #region Fields
        /// <summary>
        /// Will be true after the connection button was pressed -> <see cref="Connect"/>
        /// </summary>
        private bool connecting;
        #endregion
        
        #region Properties
        /// <summary>
        /// Indicates whether this menu is currently open or not
        /// </summary>
        public bool IsOpen { get; private set; }
        #endregion
        
        #region Methods
        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            this.IsOpen = true;
            this.connecting = false;
            this.lobbyJoinMenu.KeepOpen(true);
            return base.Open(_CurrentActiveMenu);
        }

        public override void Close(bool _PlaySound)
        {
            this.lobbyJoinMenu.KeepOpen(false);
            this.Cancel();
            base.Close(_PlaySound);
            this.IsOpen = false;
        }

        /// <summary>
        /// Connects the client to the steam lobby
        /// </summary>
        public void Connect()
        {
            CustomNetworkManager.Connect();
            this.connecting = true;
            base.Close(false);
            MenuController.CloseCurrentMenu(true);
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
            
            SteamLobby.LeaveLobby();
            CustomNetworkManager.CancelConnectionAttempt();
        }
        #endregion
    }
}