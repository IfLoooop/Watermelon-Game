using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Background
{
    /// <summary>
    /// Handles logic for the evolutions chart
    /// </summary>
    internal sealed class EvolutionsChart : GameModeTransition
    {
        #region Inspector Fields
        [Tooltip("Animation to open the chart (for multiplayer)")]
        [SerializeField] private AnimationClip openAnimation;
        [Tooltip("Animation to close the chart (for multiplayer)")]
        [SerializeField] private AnimationClip closeAnimation;
        #endregion

        #region Fields
        /// <summary>
        /// Indicates whether the chart is currently opened or closed (for multiplayer)
        /// </summary>
        private bool isOpen;
        #endregion
        
        #region Methods
        protected override void Transition(GameMode _GameMode)
        {
            base.Transition(_GameMode);

            if (_GameMode == GameMode.SinglePlayer)
            {
                this.isOpen = false;
            }
        }

        /// <summary>
        /// Opens/closes the chart (for multiplayer) 
        /// </summary>
        public void Open_Close()
        {
            if (this.isOpen)
            {
                this.isOpen = false;
                base.Animation.Play(this.closeAnimation.name);
            }
            else
            {
                this.isOpen = true;
                base.Animation.Play(this.openAnimation.name);
            }
        }
        #endregion
    }
}