using Mirror;
using UnityEngine;
using Watermelon_Game.Container;
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
        
        #region Methods
        private void Awake()
        {
            if (instance != null)
            {
                return;
            }
            
            instance = this;
        }

        [Client]
        public static void RestartGame()
        {
            instance.CmdRestartGame();
        }

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
                Debug.LogError($"memberWaitingForRestart:{memberWaitingForRestart}"); // TODO: Remove
            }
            // Singleplayer
            else
            {
                GameController.StartGame();
            }
        }
        
        [ClientRpc]
        private void RpcStartGame()
        {
            memberWaitingForRestart = 0;
            ContainerBounds.SetWaitingMessage(false);
            GameController.StartGame();
        }
        
        [TargetRpc]
        private void TargetWaitingForOtherPlayer(NetworkConnectionToClient _Target)
        {
            ContainerBounds.SetWaitingMessage(true);
        }
        #endregion
    }
}