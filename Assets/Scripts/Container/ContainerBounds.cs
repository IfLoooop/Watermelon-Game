using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Menus;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Container
{
    /// <summary>
    /// Handles all logic for one container
    /// </summary>
    internal sealed class ContainerBounds : GameModeTransition
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Inside bounds of the container")]
        [SerializeField] private RectTransform bounds;
        [Tooltip("Border collider adjacent to the other container")]
        [SerializeField] private BoxCollider2D border; // TODO: Don't deactivate (set to another layer), otherwise normal fruits will be able to change the container 
        [Tooltip("Animation to player during a SinglePlayer transition")]
        [SerializeField] private AnimationClip singlePlayerTransition;
        [Tooltip("Animation to player during a MultiPlayer transition")]
        [SerializeField] private AnimationClip multiPlayerTransition;

        [Header("Settings")]
        [Tooltip("Y-position of the FruitSpawner")]
        [ShowInInspector] private static ProtectedFloat fruitSpawnerHeight = 16;
        
        [Header("Debug")]
        [Tooltip("The connection Id of the player that is assigned to this container")]
        [ShowInInspector][ReadOnly] private ProtectedInt32? connectionId;
        #endregion

        #region Fields
        /// <summary>
        /// <see cref="Animation"/> component to play, on <see cref="ExitMenu.OnGameModeTransition"/>
        /// </summary>
        private Animation gameModeTransition;
        /// <summary>
        /// Reference to the <see cref="FruitSpawner"/> that is assigned to this container
        /// </summary>
        [CanBeNull] private FruitSpawner fruitSpawner;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="connectionId"/>
        /// </summary>
        public ProtectedInt32? ConnectionId => this.connectionId;
        /// <summary>
        /// Starting position of the <see cref="FruitSpawner"/>
        /// </summary>
        public ProtectedVector2 StartingPosition { get; private set; } = new Vector2(0, fruitSpawnerHeight);
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.gameModeTransition = base.GetComponent<Animation>();
        }

        private void Start()
        {
            this.bounds.GetComponent<Canvas>().worldCamera = CameraUtils.Camera;
        }

        protected override void Transition(GameMode _GameMode)
        {
            base.Transition(_GameMode);
            
            if (this.fruitSpawner != null)
            {
                this.fruitSpawner.GameModeTransitionStarted();
            }
            
            switch (_GameMode)
            {
                case GameMode.SinglePlayer:
                    this.gameModeTransition.Play(this.singlePlayerTransition.name);
                    this.border.enabled = true;
                    break;
                case GameMode.MultiPlayer:
                    this.gameModeTransition.Play(this.multiPlayerTransition.name);
                    this.border.enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Assigns this <see cref="ContainerBounds"/> to the given <see cref="FruitSpawner"/> 
        /// </summary>
        /// <param name="_FruitSpawner">The <see cref="FruitSpawner"/> to assign this <see cref="ContainerBounds"/> to</param>
        public void AssignToPlayer(FruitSpawner _FruitSpawner)
        {
            this.fruitSpawner = _FruitSpawner;
            this.connectionId = this.fruitSpawner!.SetContainerBounds(this);
        }

        /// <summary>
        /// Sets <see cref="fruitSpawner"/> and <see cref="connectionId"/> to null
        /// </summary>
        public void FreeContainer()
        {
            this.fruitSpawner = null;
            this.connectionId = null;
        }

        /// <summary>
        /// Sets the <see cref="StartingPosition"/> for the <see cref="FruitSpawner"/> <br/>
        /// <i>Call this after the container has been positioned correctly</i>
        /// </summary>
        public void SetStartingPosition()
        {
            this.StartingPosition = new Vector2(base.transform.position.x, fruitSpawnerHeight);
            
            if (this.fruitSpawner != null)
            {
                this.fruitSpawner.GameModeTransitionEnded();
            }
        }
        
        /// <summary>
        /// Returns true if the x and y components of point is a point inside <see cref="bounds"/>
        /// </summary>
        /// <param name="_Point">Point to test</param>
        /// <returns>True if the point lies within the specified rectangle</returns>
        public bool Contains(Vector2 _Point)
        {
            return this.bounds.rect.Contains(new Vector3(_Point.x, _Point.y) - this.bounds.position);
        }
        #endregion
    }
}