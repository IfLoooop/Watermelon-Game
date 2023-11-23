using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Contains exit and restart logic
    /// </summary>
    internal sealed class ExitMenu : MenuBase
    {
        #region Inspector Fields
        [Tooltip("Multiplayer TMP component")]
        [SerializeField] private TextMeshProUGUI multiplayer;
        [Tooltip("Singleplayer TMP component")]
        [SerializeField] private TextMeshProUGUI singleplayer;
        
        [Header("Debug")]
        [Tooltip("The currently active GameMode")]
        [SerializeField][ReadOnly] private GameMode currentGameMode = GameMode.SinglePlayer;
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
        /// Exits the game
        /// </summary>
        public void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }

        /// <summary>
        /// Calls <see cref="OnGameModeTransition"/> and switches <see cref="currentGameMode"/> to the currently active one
        /// </summary>
        public void GameModeTransition()
        {
            switch (this.currentGameMode)
            {
                case GameMode.SinglePlayer:
                    OnGameModeTransition?.Invoke(GameMode.MultiPlayer);
                    this.currentGameMode = GameMode.MultiPlayer;
                    this.singleplayer.gameObject.SetActive(true);
                    this.multiplayer.gameObject.SetActive(false);
                    break;
                case GameMode.MultiPlayer:
                    OnGameModeTransition?.Invoke(GameMode.SinglePlayer);
                    this.currentGameMode = GameMode.SinglePlayer;
                    this.multiplayer.gameObject.SetActive(true);
                    this.singleplayer.gameObject.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// TODO: Use different animation
        /// Needed for the "Popup"-Animation
        /// </summary>
        private void DisableSetScrollPosition() { }
        #endregion
    }
}