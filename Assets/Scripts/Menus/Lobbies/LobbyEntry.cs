using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Networking;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Displays the data for one lobby
    /// </summary>
    internal sealed class LobbyEntry : EnhancedScrollerCellView
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Displays the name of the user who hosted the lobby")]
        [SerializeField] private TextMeshProUGUI lobbyName;
        [Tooltip("Will be visible if the lobby requires a password")]
        [SerializeField] private Image requiresPassword;
        [Tooltip("Reference to the button to join the lobby")]
        [SerializeField] private Button joinButton;
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the data of this <see cref="LobbyEntry"/>
        /// </summary>
        /// <param name="_LobbiesDataIndex">Index in <see cref="LobbyJoinMenu.lobbyList"/>, this object holds the data of</param>
        public void SetData(int _LobbiesDataIndex)
        {
            base.dataIndex = _LobbiesDataIndex;
            this.RefreshCellView();
        }

        public override void RefreshCellView()
        {
            base.RefreshCellView();

            this.lobbyName.text = LobbyJoinMenu.LobbyList[base.dataIndex].LobbyName;
            this.requiresPassword.enabled = LobbyJoinMenu.LobbyList[base.dataIndex].RequiresPassword;
            this.joinButton.interactable = LobbyJoinMenu.LobbyList[base.dataIndex].Interactable;
        }

        /// <summary>
        /// Tries joining the lobby with the index of <see cref="EnhancedScrollerCellView.dataIndex"/> in <see cref="LobbyJoinMenu"/>.<see cref="LobbyJoinMenu.LobbyList"/>
        /// </summary>
        public void Join()
        {
            if (CustomNetworkManager.AttemptingToJoinALobby)
            {
                return;
            }
            
            var _lobbyData = LobbyJoinMenu.LobbyList[base.dataIndex];
            if (SteamLobby.CurrentLobbyId is {} _lobbyId)
            {
                if (_lobbyData.LobbyId == _lobbyId.m_SteamID)
                {
                    return;
                }
            }
            
            LobbyJoinMenu.AllowJoinButtonInteraction(false);
            
            if (_lobbyData.RequiresPassword)
            {
                LobbyJoinMenu.OpenPasswordMenu(_lobbyData.LobbyId);
            }
            else
            {
                SteamLobby.JoinLobby(_lobbyData.LobbyId);
            }
        }
        #endregion
    }
}