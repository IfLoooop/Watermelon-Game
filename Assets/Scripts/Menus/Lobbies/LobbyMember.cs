using OPS.AntiCheat.Field;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Contains logic to kick a player from a lobby
    /// </summary>
    internal sealed class LobbyMember : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Displays the name of the player")]
        [SerializeField] private new TextMeshProUGUI name;
        [Tooltip("Button to kick the player")]
        [SerializeField] private Button kickButton;
        #endregion

        #region Fields
        /// <summary>
        /// The id if the lobby, the member is in right now
        /// </summary>
        private ProtectedUInt64 lobbyId;
        #endregion
        
        #region Properties
        /// <summary>
        /// The steam id of the player to kick
        /// </summary>
        public ProtectedUInt64 SteamId { get; private set; }
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the data for one lobby member
        /// </summary>
        /// <param name="_SteamId">The steam id of the lobby member</param>
        /// <param name="_Username">The username of the lobby member</param>
        /// <param name="_Active">Sets the active state of the GameObject</param>
        /// <returns>The <see cref="LobbyMember"/> whose data was just set</returns>
        public LobbyMember SetMemberData(ulong _SteamId, string _Username, bool _Active)
        {
            base.gameObject.SetActive(_Active);
            return this.SetMemberData(_SteamId, _Username);
        }
        
        /// <summary>
        /// Sets the data for one lobby member
        /// </summary>
        /// <param name="_SteamId">The steam id of the lobby member</param>
        /// <param name="_Username">The username of the lobby member</param>
        /// <returns>The <see cref="LobbyMember"/> whose data was just set</returns>
        public LobbyMember SetMemberData(ulong _SteamId, string _Username)
        {
            this.SteamId = _SteamId;
            this.name.text = _Username;

            if (_SteamId == SteamManager.SteamID.m_SteamID)
            {
                this.kickButton.interactable = false;
            }

            return this;
        }
        
        /// <summary>
        /// Sets this GameObject active
        /// </summary>
        public void SetActive()
        {
            base.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Kick the player with this <see cref="SteamId"/> from the lobby
        /// </summary>
        public void KickLobbyMember()
        {
            // Only host is allowed to kick
            if (!SteamLobby.IsHost.Value.Value)
            {
                return;
            }
            // Will be true for the host, so hosts can't kick themself
            if (this.SteamId == SteamManager.SteamID.m_SteamID)
            {
                return;
            }
            
            SteamLobby.SendPlayerKickMessage(System.Text.Encoding.UTF8.GetBytes(this.SteamId.ToString()));
        }
        #endregion
    }
}