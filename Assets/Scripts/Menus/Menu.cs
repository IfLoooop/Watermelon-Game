using Watermelon_Game.Menus.MainMenus;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// All possible main menus for <see cref="MenuController"/>
    /// </summary>
    internal enum Menu
    {
        /// <summary>
        /// <see cref="MenuContainer"/>
        /// </summary>
        MenuContainer,
        /// <summary>
        /// <see cref="SingleplayerMenu"/>
        /// </summary>
        Singleplayer,
        /// <summary>
        /// <see cref="MultiplayerMenu"/>
        /// </summary>
        Multiplayer,
        /// <summary>
        /// <see cref="LobbyJoin"/>
        /// </summary>
        LobbyJoin,
        /// <summary>
        /// <see cref="LobbyCreate"/>
        /// </summary>
        LobbyCreate,
        /// <summary>
        /// <see cref="LobbyPassword"/>
        /// </summary>
        LobbyPassword,
        /// <summary>
        /// <see cref="LobbyHost"/>
        /// </summary>
        LobbyHost,
        /// <summary>
        /// <see cref="LobbyConnect"/>
        /// </summary>
        LobbyConnect
    }
}