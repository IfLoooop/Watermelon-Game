using UnityEngine;
using Watermelon_Game.Menus;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Background
{
    internal sealed class EvolutionsChart : GameModeTransition
    {
        #region Inspector Fields
        [Tooltip("Animation to player during a SinglePlayer transition")]
        [SerializeField] private AnimationClip singlePlayerTransition;
        [Tooltip("Animation to player during a MultiPlayer transition")]
        [SerializeField] private AnimationClip multiPlayerTransition;
        [Tooltip("Animation to open the chart (for multiplayer)")]
        [SerializeField] private AnimationClip openAnimation;
        [Tooltip("Animation to close the chart (for multiplayer)")]
        [SerializeField] private AnimationClip closeAnimation;
        #endregion

        #region Fields
        /// <summary>
        /// <see cref="Animation"/> component to play on <see cref="ExitMenu.OnGameModeTransition"/>
        /// </summary>
        private new Animation animation;
        /// <summary>
        /// Indicates whether the chart is currently opened or closed (for multiplayer)
        /// </summary>
        private bool isOpen;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.animation = base.GetComponent<Animation>();
        }
        
        protected override void Transition(GameMode _GameMode)
        {
            base.Transition(_GameMode);
            
            switch (_GameMode) // TODO: Play sound and make animation  better
            {
                case GameMode.SinglePlayer:
                    this.animation.Play(this.singlePlayerTransition.name);
                    break;
                case GameMode.MultiPlayer:
                    this.animation.Play(this.multiPlayerTransition.name);
                    break;
            }
        }

        /// <summary>
        /// Opens/closes the chart (for multiplayer) 
        /// </summary>
        private void Open_Close()
        {
            if (this.isOpen)
            {
                this.isOpen = false;
                this.animation.Play(this.closeAnimation.name);
            }
            else
            {
                this.isOpen = true;
                this.animation.Play(this.openAnimation.name);
            }
        }
        #endregion
    }
}