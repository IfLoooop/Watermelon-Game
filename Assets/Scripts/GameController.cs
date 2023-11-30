using System;
using System.Collections;
using System.Collections.Generic;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;
using Watermelon_Game.Menus.MainMenus;
using Watermelon_Game.Networking;
using Watermelon_Game.Singletons;
using Watermelon_Game.Utility;

namespace Watermelon_Game
{
    /// <summary>
    /// Contains general game and game-state logic
    /// </summary>
    internal sealed class GameController : PersistantMonoBehaviour<GameController>
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("All container in the scene")]
        [SceneObjectsOnly]
        [SerializeField] private List<ContainerBounds> containers;

#if UNITY_EDITOR
        [Header("Debug")]
        [Tooltip("The currently active GameMode")]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField][ReadOnly] private GameMode currentGameMode;
#endif
        #endregion
        
        #region Fields
        /// <summary>
        /// <see cref="ResetReason"/>
        /// </summary>
        private ResetReason resetReason;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="containers"/>
        /// </summary>
        public static List<ContainerBounds> Containers => Instance.containers;
        /// <summary>
        /// Indicates whether a game is currently running or over
        /// </summary>
        public static bool ActiveGame { get; private set; }
        /// <summary>
        /// Timestamp in seconds, when the currently active game was started -> <see cref="Time"/>.<see cref="Time.time"/> <br/>
        /// <i>Is reset on every <see cref="GameController"/>.<see cref="GameController.StartGame"/></i>
        /// </summary>
        public static ProtectedFloat CurrentGameTimeStamp { get; private set; }
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
        /// Is called when <see cref="ResetGame"/> has finished <br/>
        /// <b>Parameter:</b> <see cref="ResetReason"/>
        /// </summary>
        public static event Action<ResetReason> OnResetGameFinished;
        #endregion
        
        #region Methods

        protected override void Init()
        {
            base.Init();
            this.containers.ForEach(DontDestroyOnLoad);
            Application.targetFrameRate = 120;
        }
        
        private void OnEnable()
        {
            CustomNetworkManager.OnConnectionStopped += this.ConnectionStopped;
            MaxHeight.OnGameOver += this.GameOver;
            MenuController.OnManualRestart += this.ManualRestart;
            MenuController.OnRestartGame += StartGame;
            Application.quitting += this.ApplicationIsQuitting;

#if UNITY_EDITOR
            MainMenuBase.OnGameModeTransition += SetCurrentGameMode_EDITOR;
#endif
        }

        private void OnDisable()
        {
            CustomNetworkManager.OnConnectionStopped -= this.ConnectionStopped;
            MaxHeight.OnGameOver -= this.GameOver;
            MenuController.OnManualRestart -= this.ManualRestart;
            MenuController.OnRestartGame -= StartGame;
            Application.quitting -= this.ApplicationIsQuitting;
            
#if UNITY_EDITOR
            MainMenuBase.OnGameModeTransition -= SetCurrentGameMode_EDITOR;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sets <see cref="currentGameMode"/> to the given value <br/>
        /// <i>Only for debug purposes in editor</i>
        /// </summary>
        /// <param name="_GameMode">The <see cref="GameMode"/> to set <see cref="currentGameMode"/> to</param>
        private void SetCurrentGameMode_EDITOR(GameMode _GameMode)
        {
            this.currentGameMode = _GameMode;
        }  
#endif
        
        /// <summary>
        /// Starts the game
        /// </summary>
        public static void StartGame()
        {
            ActiveGame = true;
            OnGameStart?.Invoke();
            CurrentGameTimeStamp = Time.time;
        }
        
        /// <summary>
        /// <see cref="MaxHeight.OnGameOver"/>
        /// </summary>
        /// <param name="_ConnectionId">The connection id of the client that lost</param>
        private void GameOver(int _ConnectionId) // TODO: Use _ConnectionId
        {
            this.resetReason = ResetReason.GameOver;
            this.StartCoroutine(this.ResetGame());
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
            ActiveGame = false;
            OnResetGameStarted?.Invoke();
            
            var _waitTime = new WaitForSeconds(.1f);
            var _fruits = FruitController.Fruits;
            
            // ReSharper disable once InconsistentNaming
            for (var i = _fruits.Count - 1; i >= 0; i--)
            {
                if (_fruits[i] != null) // TODO: Null check shouldn't be necessary, but fruit is sometimes null for some reason
                {
                    _fruits[i].DestroyFruit();   
                }
                
                yield return _waitTime;
            }
            
            OnResetGameFinished?.Invoke(this.resetReason);
        }
        
        /// <summary>
        /// <see cref="Application.quitting"/>
        /// </summary>
        private void ApplicationIsQuitting()
        {
            IsApplicationQuitting = true;
        }

        /// <summary>
        /// Is called whenever the host or client connection has been stopped
        /// </summary>
        private void ConnectionStopped()
        {
            ActiveGame = false;
            this.resetReason = ResetReason.ManualRestart;
            OnResetGameFinished?.Invoke(this.resetReason);
        }
        #endregion
    }
}