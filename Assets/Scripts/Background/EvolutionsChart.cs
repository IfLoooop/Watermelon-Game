using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Singletons;

namespace Watermelon_Game.Background
{
    /// <summary>
    /// Handles logic for the evolutions chart
    /// </summary>
    internal sealed class EvolutionsChart : PersistantGameModeTransition<EvolutionsChart>
    {
        #region Inspector Fields
        [Tooltip("Button to open the chart")]
        [SerializeField] private Button open;
        [Tooltip("Button to close the chart")]
        [SerializeField] private Button close;
        [Tooltip("Animation to open the chart (for multiplayer)")]
        [SerializeField] private AnimationClip openAnimation;
        [Tooltip("Animation to close the chart (for multiplayer)")]
        [SerializeField] private AnimationClip closeAnimation;
        #endregion
        
        #region Methods
        /// <summary>
        /// Opens the chart
        /// </summary>
        public void Open()
        {
            this.open.interactable = false;
            this.close.interactable = true;
            base.Animation.Play(this.openAnimation.name);
        }

        /// <summary>
        /// Closes the chart
        /// </summary>
        public void Close()
        {
            this.close.interactable = false;
            this.open.interactable = true;
            base.Animation.Play(this.closeAnimation.name);
        }
        #endregion
    }
}