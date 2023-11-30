using System;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.MainMenus
{
    /// <summary>
    /// Base class for all menus that are able to change the <see cref="GameMode"/>
    /// </summary>
    internal abstract class MainMenuBase : MenuBase
    {
        #region Properties
        /// <summary>
        /// The currently active GameMode
        /// </summary>
        public static GameMode CurrentGameMode { get; protected set; } = GameMode.SinglePlayer;
        #endregion
        
        #region Events
        /// <summary>
        /// Is called everytime the game mode is switch to <see cref="GameMode.SinglePlayer"/> or <see cref="GameMode.MultiPlayer"/> <br/>
        /// <b>Parameter:</b> The <see cref="GameMode"/> that is being switched to
        /// </summary>
        public static event Action<GameMode> OnGameModeTransition;
        #endregion

        #region Methods
        /// <summary>
        /// Changes <see cref="CurrentGameMode"/> to the given <see cref="GameMode"/> and invokes <see cref="OnGameModeTransition"/>
        /// </summary>
        /// <param name="_GameMode">The <see cref="GameMode"/> to switch to</param>
        protected static void GameModeTransition(GameMode _GameMode)
        {
            OnGameModeTransition?.Invoke(_GameMode);
        }
        #endregion
    }
}