using System;
using System.Collections;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;

namespace Watermelon_Game
{
    /// <summary>
    /// Contains general game and game-state logic
    /// </summary>
    internal sealed class GameController : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// <see cref="ResetReason"/>
        /// </summary>
        private ResetReason resetReason;
        #endregion
        
        #region Properties
        /// <summary>
        /// Timestamp in seconds, when the currently active game was started -> <see cref="Time"/>.<see cref="Time.time"/>
        /// </summary>
        public static float CurrentGameTimeStamp { get; private set; }
        /// <summary>
        /// Will be true when the application is about to be closed
        /// </summary>
        public static bool IsApplicationQuitting { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Is called every time a game starts
        /// </summary>
        public static event Action OnGameStart;
        /// <summary>
        /// Is called when the game is being reset -> <see cref="ResetGame"/>
        /// </summary>
        public static event Action OnResetGameStarted;
        /// <summary>
        /// Is called when <see cref="ResetGame"/> has finished
        /// </summary>
        public static event Action OnResetGameFinished;
        /// <summary>
        /// Is called <see cref="MaxHeight.OnGameOver"/> after <see cref="ResetGame"/> has finished
        /// </summary>
        public static event Action OnRestartGame;
        #endregion
        
        #region Methods
        private void OnEnable()
        {
            MaxHeight.OnGameOver += this.GameOver;
            MenuController.OnManualRestart += this.ManualRestart;
            OnResetGameFinished += this.GameReset;
            GameOverMenu.OnGameOverMenuClosed += this.StartGame;
            Application.quitting += this.ApplicationIsQuitting;
        }

        private void OnDisable()
        {
            MaxHeight.OnGameOver -= this.GameOver;
            MenuController.OnManualRestart -= this.ManualRestart;
            OnResetGameFinished -= this.GameReset;
            GameOverMenu.OnGameOverMenuClosed -= this.StartGame;
            Application.quitting -= this.ApplicationIsQuitting;
        }

        private void Start()
        {
            // TODO: Replace with menu logic
            StartGame();
        }
        
        /// <summary>
        /// Starts the game
        /// </summary>
        private void StartGame()
        {
            OnGameStart?.Invoke();
            CurrentGameTimeStamp = Time.time;
        }
        
        /// <summary>
        /// <see cref="MaxHeight.OnGameOver"/>
        /// </summary>
        private void GameOver()
        {
            this.resetReason = ResetReason.GameOver;
            base.StartCoroutine(ResetGame());
        }

        /// <summary>
        /// <see cref="MenuController.OnManualRestart"/> 
        /// </summary>
        private void ManualRestart()
        {
            this.resetReason = ResetReason.ManualRestart;
            base.StartCoroutine(ResetGame());
        }
        
        /// <summary>
        /// Resets the game to its initial state
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResetGame()
        {
            OnResetGameStarted?.Invoke();
            
            var _waitTime = new WaitForSeconds(.1f);
            var _fruits = FruitController.Fruits;
            
            // ReSharper disable once InconsistentNaming
            for (var i = _fruits.Count - 1; i >= 0; i--)
            {
                _fruits[i].DestroyFruit();
                
                yield return _waitTime;
            }
            
            OnResetGameFinished?.Invoke();
        }

        /// <summary>
        /// <see cref="OnResetGameFinished"/>
        /// </summary>
        private void GameReset()
        {
            switch (this.resetReason)
            {
                case ResetReason.GameOver:
                    OnRestartGame?.Invoke();
                    break;
                case ResetReason.ManualRestart:
                    this.StartGame();
                    break;
            }
        }
        
        /// <summary>
        /// <see cref="Application.quitting"/>
        /// </summary>
        private void ApplicationIsQuitting()
        {
            IsApplicationQuitting = true;
        }
        #endregion
    }
}