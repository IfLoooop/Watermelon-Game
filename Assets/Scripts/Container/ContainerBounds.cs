using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Utility;
using Watermelon_Game.Utility.Pools;

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
        [Tooltip("Trigger of MaxHeight")]
        [SerializeField] private BoxCollider2D maxHeightTrigger;
        [Tooltip("Displays a message while waiting for the other player")]
        [SerializeField] private TextMeshProUGUI waitingMessage;
        [Tooltip("The target position to shoot the StoneFruit at")]
        [SerializeField] private Transform stoneFruitTarget;

        [Header("Settings")]
        [Tooltip("Y-position of the FruitSpawner")]
        [ShowInInspector] private static ProtectedFloat fruitSpawnerHeight = 17;
        
        [Header("Debug")]
        [Tooltip("The connection Id of the player that is assigned to this container")]
        [ShowInInspector][ReadOnly] private ProtectedInt32? connectionId;
        #endregion
        
        #region Fields
        /// <summary>
        /// The container that is assigned to the local player
        /// </summary>
        private static ContainerBounds instance;
        /// <summary>
        /// Reference to the <see cref="FruitSpawner"/> that is assigned to this container <br/>
        /// <b>Will only be set for the local client, for every other client this will be null!</b>
        /// </summary>
        [CanBeNull] private FruitSpawner fruitSpawner;
        #endregion
        
        #region Properties
        /// <summary>
        /// True if this container is assigned to the local client <br/>
        /// False if it assigned to another client
        /// </summary>
        public ProtectedBool PlayerContainer { get; private set; }
        /// <summary>
        /// <see cref="connectionId"/>
        /// </summary>
        public ProtectedInt32? ConnectionId => this.connectionId;
        /// <summary>
        /// Starting position of the <see cref="FruitSpawner"/>
        /// </summary>
        public ProtectedVector2 StartingPosition { get; private set; } = new Vector2(0, fruitSpawnerHeight);
        /// <summary>
        /// <see cref="stoneFruitTarget"/>
        /// </summary>
        public Transform StoneFruitTarget => this.stoneFruitTarget;
        #endregion
        
        #region Methods
        private void Start()
        {
            if (!GameController.Containers.Contains(this))
            {
                Destroy(base.gameObject);
            }
            
            this.bounds.GetComponent<Canvas>().worldCamera = CameraUtils.Camera;
        }

        protected override void Transition(GameMode _GameMode, bool _ForceSwitch)
        {
            if (this.fruitSpawner != null)
            {
                this.fruitSpawner.GameModeTransitionStarted();
            }
            
            base.Transition(_GameMode, _ForceSwitch);
            
            AudioPool.PlayClip(AudioClipName.FruitDestroy);
        }

        /// <summary>
        /// Assigns this <see cref="ContainerBounds"/> to the given <see cref="FruitSpawner"/> <br/>
        /// <i>Use for client container</i>
        /// </summary>
        /// <param name="_FruitSpawner">The <see cref="FruitSpawner"/> to assign this <see cref="ContainerBounds"/> to</param>
        public void AssignToPlayer(FruitSpawner _FruitSpawner)
        {
            instance = this;
            this.fruitSpawner = _FruitSpawner;
            this.connectionId = this.fruitSpawner!.SetContainerBounds(this);
            this.PlayerContainer = true;
            this.maxHeightTrigger.enabled = true;
        }
        
        /// <summary>
        /// Sets <see cref="fruitSpawner"/> and <see cref="connectionId"/> to null
        /// </summary>
        public void FreeContainer()
        {
            this.fruitSpawner = null;
            this.connectionId = null;
            this.PlayerContainer = false;
            this.maxHeightTrigger.enabled = false;
        }

        /// <summary>
        /// Sets the <see cref="StartingPosition"/> for the <see cref="FruitSpawner"/> <br/>
        /// <i>
        /// Call this after the container has been positioned correctly <br/>
        /// Currently called at the end of the transition animation
        /// </i>
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
        /// Returns true if the x and y components of point is a point inside <see cref="bounds"/> <br/>
        /// <i>Will always use the player container</i>
        /// </summary>
        /// <param name="_Point">Point to test</param>
        /// <returns>True if the point lies within the specified rectangle</returns>
        public static bool Contains(Vector2 _Point)
        {
            return GameController.ActiveGame && instance.bounds.rect.Contains(new Vector3(_Point.x, _Point.y) - instance.bounds.position);
        }
        
        /// <summary>
        /// Enables/disables <see cref="waitingMessage"/> for the <see cref="PlayerContainer"/>
        /// </summary>
        /// <param name="_Value">True for enable, false for disable</param>
        public static void SetWaitingMessage(bool _Value)
        {
            instance.waitingMessage.gameObject.SetActive(_Value);
        }
        #endregion
    }
}