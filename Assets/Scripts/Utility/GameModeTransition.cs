using UnityEngine;
using Watermelon_Game.Menus.MainMenus;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Handles the <see cref="SingleplayerMenu"/>.<see cref="SingleplayerMenu.OnGameModeTransition"/> event <br/>
    /// <i>Overwrite <see cref="Transition"/> to execute custom logic on transition</i>
    /// </summary>
    [RequireComponent(typeof(Animation))]
    internal abstract class GameModeTransition : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Transitions")]
        [Tooltip("Animation to player during a SinglePlayer transition")]
        [SerializeField] private AnimationClip singlePlayerTransition;
        [Tooltip("Animation to player during a MultiPlayer transition")]
        [SerializeField] private AnimationClip multiPlayerTransition;
        #endregion
        
        #region Fields
        /// <summary>
        /// Should always be different from the incoming <see cref="GameMode"/> in <see cref="Transition"/> <br/>
        /// <b>Same value indicates that a <see cref="SingleplayerMenu.OnGameModeTransition"/>-event has been missed and this GameObject might not be in sync with the current transition state anymore</b>
        /// </summary>
        private GameMode? currentGameMode;
        #endregion

        #region Properties
        /// <summary>
        /// Animation component to play the animation clips through
        /// </summary>
        protected Animation Animation { get; private set; }
        #endregion
        
        #region Methods
        protected virtual void Awake()
        {
            this.Animation = base.GetComponent<Animation>();
        }

        protected virtual void OnEnable()
        {
            MainMenuBase.OnGameModeTransition += Transition;
        }

        protected virtual void OnDisable()
        {
            MainMenuBase.OnGameModeTransition -= Transition;
        }

        /// <summary>
        /// Is called on <see cref="SingleplayerMenu.OnGameModeTransition"/> <br/>
        /// <i>Prints a warning if the incoming <see cref="GameMode"/> is the same as <see cref="currentGameMode"/></i>
        /// </summary>
        /// <param name="_GameMode">The <see cref="GameMode"/> to transition to</param>
        protected virtual void Transition(GameMode _GameMode)
        {
            if (this.currentGameMode != null && this.currentGameMode.Value == _GameMode)
            {
                Debug.LogWarning($"The current GameMode for {base.gameObject.name} is the same as the incoming one");
            }

            this.currentGameMode = _GameMode;
            
            switch (_GameMode)
            {
                case GameMode.SinglePlayer:
                    this.Animation.Play(this.singlePlayerTransition.name);
                    break;
                case GameMode.MultiPlayer:
                    this.Animation.Play(this.multiPlayerTransition.name);
                    break;
            }
        }
        #endregion
    }
}