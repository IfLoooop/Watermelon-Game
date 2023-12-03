using OPS.AntiCheat.Field;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Holds the data for one lobby
    /// </summary>
    internal readonly struct LobbyData
    {
        #region Properties
        /// <summary>
        /// Id of the lobby
        /// </summary>
        public ProtectedUInt64 LobbyId { get; }
        /// <summary>
        /// Name of the lobby
        /// </summary>
        public ProtectedString LobbyName { get; }
        /// <summary>
        /// Indicates whether this lobby is password protected
        /// </summary>
        public ProtectedBool RequiresPassword { get; }
        /// <summary>
        /// Indicates whether the button for this lobby is interactable or not
        /// </summary>
        public ProtectedBool Interactable { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// When downloading a new set of lobby entries <br/>
        /// <i><see cref="Interactable"/> will be true</i>
        /// </summary>
        /// <param name="_LobbyId"><see cref="LobbyId"/></param>
        /// <param name="_LobbyName"><see cref="LobbyName"/></param>
        /// <param name="_RequiresPassword"><see cref="RequiresPassword"/></param>
        public LobbyData(ulong _LobbyId, string _LobbyName, bool _RequiresPassword)
        {
            this.LobbyId = _LobbyId;
            this.LobbyName = _LobbyName;
            this.RequiresPassword = _RequiresPassword;
            this.Interactable = true;
        }

        /// <summary>
        /// For updating the <see cref="Interactable"/> state of an existing set of lobby entries
        /// </summary>
        /// <param name="_LobbyData"><see cref="LobbyData"/></param>
        /// <param name="_Interactable"><see cref="Interactable"/></param>
        public LobbyData(LobbyData _LobbyData, bool _Interactable)
        {
            this.LobbyId = _LobbyData.LobbyId;
            this.LobbyName = _LobbyData.LobbyName;
            this.RequiresPassword = _LobbyData.RequiresPassword;
            this.Interactable = _Interactable;
        }
        #endregion
    }
}