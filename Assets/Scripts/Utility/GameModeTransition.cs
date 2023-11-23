using UnityEngine;
using Watermelon_Game.Menus;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Handles the <see cref="ExitMenu"/>.<see cref="ExitMenu.OnGameModeTransition"/> event <br/>
    /// <i>Overwrite <see cref="Transition"/> to execute custom logic on transition</i>
    /// </summary>
    internal abstract class GameModeTransition : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// Should always be different from the incoming <see cref="GameMode"/> in <see cref="Transition"/> <br/>
        /// <b>Same value indicates that a <see cref="ExitMenu.OnGameModeTransition"/>-event has been missed and this GameObject might not be in sync with the current transition state anymore</b>
        /// </summary>
        private GameMode? currentGameMode;
        #endregion
        
        #region Methods
        protected virtual void OnEnable()
        {
            ExitMenu.OnGameModeTransition += Transition;
        }

        protected virtual void OnDisable()
        {
            ExitMenu.OnGameModeTransition -= Transition;
        }

        /// <summary>
        /// Is called on <see cref="ExitMenu.OnGameModeTransition"/> <br/>
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
        }
        #endregion
    }
}