using Mirror;
using Watermelon_Game.Container;
using Watermelon_Game.Menus;
using Watermelon_Game.Menus.Lobbies;

namespace Watermelon_Game
{
    /// <summary>
    /// Contains network game logic
    /// </summary>
    internal sealed class NetworkGameController : NetworkBehaviour
    {
        #region Fields
        /// <summary>
        /// Singleton of <see cref="NetworkGameController"/>
        /// </summary>
        private static NetworkGameController instance;
        
        /// <summary>
        /// Number of lobby members who are currently waiting for the game to restart
        /// </summary>
        private static uint memberWaitingForRestart;
        #endregion

        #region Properties
        /// <summary>
        /// Will be true when a client joins a lobby and the game has to be restarted
        /// </summary>
        public static bool ClientHasJoinedLobby { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            GameController.OnResetGameFinished += this.StartGameOnAllClients;
        }
        
        private void OnDisable()
        {
            GameController.OnResetGameFinished -= this.StartGameOnAllClients;
        }

        /// <summary>
        /// Restarts immediately if singleplayer, otherwise waits for all player to exit the game over menu
        /// </summary>
        [Client]
        public static void RestartGame()
        {
            instance.CmdRestartGame();
        }

        /// <summary>
        /// <see cref="RestartGame"/>
        /// </summary>
        /// <param name="_Sender">The client who called this</param>
        [Command(requiresAuthority = false)]
        private void CmdRestartGame(NetworkConnectionToClient _Sender = null)
        {
            // Multiplayer
            if (LobbyHostMenu.LobbyMembers.Count > 1)
            {
                if (++memberWaitingForRestart == LobbyHostMenu.LobbyMembers.Count)
                {
                    this.RpcStartGame();
                }
                else
                {
                    this.TargetWaitingForOtherPlayer(_Sender);
                }
            }
            // Singleplayer
            else
            {
                this.RpcStartGame();
            }
        }
        
        /// <summary>
        /// Restarts the game on all connected clients
        /// </summary>
        [Server]
        public static void RestartAndStartOnAllClients()
        {
            if (GameController.ActiveGame)
            {
                ClientHasJoinedLobby = true;
                GameController.ManualRestart();
            }
            else
            {
                instance.RpcStartGame();   
            }
        }
        
        /// <summary>
        /// Starts the game on all connected clients
        /// </summary>
        /// <param name="_ResetReason">Not needed here</param>
        [Server]
        private void StartGameOnAllClients(ResetReason _ResetReason)
        {
            if (base.isServer)
            {
                if (ClientHasJoinedLobby)
                {
                    ClientHasJoinedLobby = false;
                    this.RpcStartGame();
                }   
            }
        }
        
        /// <summary>
        /// Restarts the game on all clients
        /// </summary>
        [ClientRpc]
        private void RpcStartGame()
        {
            if (!GameController.ActiveGame)
            {
                memberWaitingForRestart = 0;
                ContainerBounds.SetWaitingMessage(false);
                MenuController.CloseCurrentMenu(true);
                MenuController.CloseMenuPopup();
                GameController.StartGame();   
            }
        }
        
        /// <summary>
        /// Displays a message to the waiting client
        /// </summary>
        /// <param name="_Target"></param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetWaitingForOtherPlayer(NetworkConnectionToClient _Target)
        {
            ContainerBounds.SetWaitingMessage(true);
        }
        #endregion
    }
}