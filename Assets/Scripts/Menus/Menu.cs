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
        /// <see cref="JoinLobbyMenu"/>
        /// </summary>
        JoinLobbyMenu,
        /// <summary>
        /// <see cref="CreateLobbyMenu"/>
        /// </summary>
        CreateLobbyMenu,
        /// <summary>
        /// <see cref="LobbyPasswordMenu"/>
        /// </summary>
        LobbyPasswordMenu,
        /// <summary>
        /// <see cref="LobbyHostMenu"/>
        /// </summary>
        LobbyHostMenu
    }
}