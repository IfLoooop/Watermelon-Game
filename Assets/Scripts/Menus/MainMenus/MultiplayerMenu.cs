using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.MainMenus
{
    /// <summary>
    /// Main menu while in multiplayer mode
    /// </summary>
    internal sealed class MultiplayerMenu : MainMenuBase
    {
        #region Methods
        public void Restart()
        {
            MenuController.Restart();
        }
        
        /// <summary>
        /// Changes the <see cref="GameMode"/> to <see cref="GameMode.SinglePlayer"/>
        /// </summary>
        public void SwitchToSingleplayer()
        {
            CurrentGameMode = GameMode.SinglePlayer;
            GameModeTransition(CurrentGameMode);
        }

        public void JoinLobby()
        {
            
        }

        public void CreateLobby()
        {
            
        }
        #endregion
    }
}