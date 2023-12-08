using System;
using System.Collections;
using System.Collections.Generic;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
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
        #endregion
        
        #region Fields
        /// <summary>
        /// Indicates whether the <see cref="GameMode"/> should be switched on the next <see cref="ResetGame"/>
        /// </summary>
        private ProtectedBool switchGameMode;
        /// <summary>
        /// Indicates that the switch was not initiated by the menu buttons (e.g. friend invite/join)
        /// </summary>
        private ProtectedBool gameModeWasForced;
        /// <summary>
        /// <see cref="ResetReason"/>
        /// </summary>
        private ResetReason resetReason;
        #endregion
        
        #region Properties
        /// <summary>
        /// The currently active GameMode
        /// </summary>
        [ShowInInspector] public static GameMode CurrentGameMode { get; private set; } = GameMode.SinglePlayer;
        /// <summary>
        /// <see cref="containers"/>
        /// </summary>
        public static List<ContainerBounds> Containers => Instance.containers;
        /// <summary>
        /// Indicates whether a game is currently running or over
        /// </summary>
        public static ProtectedBool ActiveGame { get; private set; }
        /// <summary>
        /// Timestamp in seconds, when the currently active game was started -> <see cref="Time"/>.<see cref="Time.time"/> <br/>
        /// <i>Is reset on every <see cref="GameController"/>.<see cref="GameController.StartGame"/></i>
        /// </summary>
        public static ProtectedFloat CurrentGameTimeStamp { get; private set; }
        /// <summary>
        /// The steam id of the player that lost the game
        /// </summary>
        public static ProtectedUInt64 LoosingPlayer { get; private set; }
        /// <summary>
        /// Will be true when the application is about to be closed
        /// </summary>
        public static ProtectedBool IsApplicationQuitting { get; private set; }
#if UNITY_EDITOR
        /// <summary>
        /// Will be true when the editor is about to exit playmode
        /// </summary>
        public static ProtectedBool IsEditorApplicationQuitting { get; private set; }
#endif
        #endregion

        #region Events
        /// <summary>
        /// Is called everytime the game mode is switched to <see cref="GameMode.SinglePlayer"/> or <see cref="GameMode.MultiPlayer"/> <br/>
        /// <b>Parameter:</b> The <see cref="GameMode"/> that is being switched to <br/>
        /// <b>Parameter:</b> <see cref="gameModeWasForced"/>
        /// </summary>
        public static event Action<GameMode, bool> OnGameModeTransition;
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
            OnResetGameFinished += this.SwitchGameMode;
            CustomNetworkManager.OnConnectionStopped += this.ConnectionStopped;
            MaxHeight.OnGameOver += this.GameOver;
            Application.quitting += this.ApplicationIsQuitting;

#if UNITY_EDITOR
            EditorApplication.quitting += this.ApplicationIsQuitting;
#endif
        }

        private void OnDisable()
        {
            OnResetGameFinished -= this.SwitchGameMode;
            CustomNetworkManager.OnConnectionStopped -= this.ConnectionStopped;
            MaxHeight.OnGameOver -= this.GameOver;
            Application.quitting -= this.ApplicationIsQuitting;
            
#if UNITY_EDITOR
            EditorApplication.quitting -= this.ApplicationIsQuitting;
#endif
        }

        /// <summary>
        /// Switches the current <see cref="GameMode"/> to the given one
        /// </summary>
        /// <param name="_GameMode">The <see cref="GameMode"/> to switch to</param>
        /// <param name="_ForceSwitch"><see cref="gameModeWasForced"/></param>
        public static void SwitchGameMode(GameMode _GameMode, bool _ForceSwitch = false)
        {
            if (CurrentGameMode != _GameMode)
            {
                Instance.switchGameMode = true;
                Instance.gameModeWasForced = _ForceSwitch;
                CurrentGameMode = _GameMode;
                ManualRestart();   
            }
            else
            {
                if (_ForceSwitch)
                {
                    ManualRestart();
                    Instance.gameModeWasForced = true;
                }
            }
        }

        /// <summary>
        /// Invokes <see cref="OnGameModeTransition"/> when <see cref="switchGameMode"/> is true
        /// </summary>
        /// <param name="_ResetReason">Not needed here</param>
        private void SwitchGameMode(ResetReason _ResetReason)
        {
            if (switchGameMode)
            {
                this.switchGameMode = false;
                OnGameModeTransition?.Invoke(CurrentGameMode, this.gameModeWasForced);
            }
        }
        
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
        /// <param name="_SteamId">The steam id of the client that lost</param>
        private void GameOver(ulong _SteamId)
        {
            LoosingPlayer = _SteamId;
            this.resetReason = ResetReason.GameOver;
            this.StartCoroutine(this.ResetGame());
        }

        /// <summary>
        /// When clicking on the restart button
        /// </summary>
        public static void ManualRestart()
        {
            Instance.resetReason = ResetReason.ManualRestart;
            Instance.StartCoroutine(Instance.ResetGame());
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
            var _stoneFruits = FruitController.StoneFruits;
            
            // ReSharper disable once InconsistentNaming
            for (var i = _fruits.Count - 1; i >= 0; i--)
            {
                if (_fruits[i] != null) // TODO: Null check shouldn't be necessary, but fruit is sometimes null for some reason
                {
                    _fruits[i].DestroyFruit();   
                }
                
                yield return _waitTime;
            }

            // ReSharper disable once InconsistentNaming
            for (var i = _stoneFruits.Count - 1; i >= 0; i--)
            {
                if (_stoneFruits[i] != null) // TODO: Null check shouldn't be necessary, but fruit is sometimes null for some reason
                {
                    _stoneFruits[i].DestroyFruit();   
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
            
#if UNITY_EDITOR
            IsEditorApplicationQuitting = true;
#endif
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