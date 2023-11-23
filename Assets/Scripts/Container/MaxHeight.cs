using System;
using System.Collections;
using JetBrains.Annotations;
using Mirror;
using OPS.AntiCheat.Field;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Container
{
    /// <summary>
    /// Contains logic for the game over conditions
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D), typeof(Animation), typeof(GodRayFlicker))]
    internal sealed class MaxHeight : NetworkBehaviour
    {
        #region Inspector Fields
#if DEBUG || DEVELOPMENT_BUILD
        [Header("Development")]
        [Tooltip("Disables the loosing condition (Editor only)")]
        [Sirenix.OdinInspector.ShowInInspector] private static bool disableCountDown;
#endif
        [Header("References")]
        [Tooltip("The container for this MaxHeight object")]
        [SerializeField] private ContainerBounds container;
        
        [Header("Settings")]
        [Tooltip("Total duration in seconds of the countdown")]
        [SerializeField] private ProtectedUInt32 countdownTime = 8;
        [Tooltip("Duration in seconds the god ray stays visible before deactivating itself again")]
        [SerializeField] private float godRayDuration = 1.5f;
        #endregion
        
        #region Fields
        /// <summary>
        /// Detects only <see cref="Fruits.Fruit"/>s
        /// </summary>
        private BoxCollider2D trigger;
        
        /// <summary>
        /// <see cref="Animation"/> that indicates, a <see cref="Fruits.Fruit"/> has crossed the height limit
        /// </summary>
        private Animation borderLineAnimation;
        /// <summary>
        /// <see cref="Animation"/> that plays after a <see cref="Fruits.Fruit"/> has been to long above the height limit
        /// </summary>
        private Animation countdownAnimation;
        
        /// <summary>
        /// Displays the current countdown
        /// </summary>
        private TextMeshProUGUI countdownText;
        /// <summary>
        /// The current value of the countdown
        /// </summary>
        private ProtectedUInt32 currentCountdownTime;
        
        /// <summary>
        /// Disables the <see cref="GodRayFlicker.godRay"/> when enabled
        /// </summary>
        private GodRayFlicker godRayFlicker;
        /// <summary>
        /// <see cref="EnableFlicker"/>
        /// </summary>
        [CanBeNull] private IEnumerator enableFlicker;
        /// <summary>
        /// Time in seconds, before the <see cref="godRayFlicker"/> <see cref="Component"/> is enabled
        /// </summary>
        private WaitForSeconds timeBeforeStart;
        #endregion

        #region Properties
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// <see cref="disableCountDown"/>
        /// </summary>
        public static bool DisableCountDown { get => disableCountDown; set => disableCountDown = value; }
#endif
        #endregion
        
        #region Events
        /// <summary>
        /// Is called when <see cref="currentCountdownTime"/> reaches 0
        /// </summary>
        public static event Action OnGameOver;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.trigger = base.GetComponent<BoxCollider2D>();
            this.borderLineAnimation = base.GetComponentInChildren<SpriteRenderer>().gameObject.GetComponent<Animation>();
            this.countdownAnimation = base.GetComponent<Animation>();
            this.countdownText = base.GetComponentInChildren<TextMeshProUGUI>();
            this.currentCountdownTime = this.countdownTime;
            this.godRayFlicker = base.GetComponent<GodRayFlicker>();
            this.timeBeforeStart = new WaitForSeconds(this.godRayDuration);
        }

        private void OnEnable()
        {
            FruitBehaviour.OnUpgradeToGoldenFruit += this.EnableGodRay;
        }

        private void OnDisable()
        {
            FruitBehaviour.OnUpgradeToGoldenFruit -= this.EnableGodRay;
        }

        private void OnTriggerEnter2D(Collider2D _Other)
        {
            if (!this.countdownAnimation.enabled)
            {
                Debug.Log("OnTriggerEnter2D");
                this.countdownAnimation.enabled = true;
            }
        }

        private void OnTriggerStay2D(Collider2D _Other)
        {
            if (!this.borderLineAnimation.isPlaying)
            {
                this.borderLineAnimation.Play();
            }
        }

        private void OnTriggerExit2D(Collider2D _Other)
        {
            var _fruitInTrigger = this.trigger.IsTouchingLayers(LayerMaskController.FruitMask);
            if (!_fruitInTrigger)
            {
                this.Reset();
            }
        }
        
        /// <summary>
        /// Is called at the end of <see cref="countdownAnimation"/>
        /// </summary>
        //[Client]
        public void CountDown()
        {
            this.currentCountdownTime--;
            this.RpcCountdown();
            //this.CmdCountdown();
        }

        //[Command(requiresAuthority = false)]
        private void CmdCountdown(NetworkConnectionToClient _Sender = null)
        {
            Debug.Log("CmdCountdown");
            //this.RpcCountdown();
        }
        
        //[ClientRpc]
        private void RpcCountdown()
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (DisableCountDown)
            {
                return;
            }
#endif
            // if (_ClientConnectionId != this.container.ConnectionId)
            // {
            //     return;
            // }
            
            Debug.Log($"RpcCountdown: {base.name}");
            
            
            this.countdownText.text = this.currentCountdownTime.ToString();

            if (this.currentCountdownTime == 5)
            {
                this.countdownText.enabled = true;
            }
            else if (this.currentCountdownTime == 0)
            {
                this.Reset();
                OnGameOver?.Invoke();
            }

            if (this.countdownText.enabled)
            {
                AudioPool.PlayClip(AudioClipName.Countdown);
            }
        }
        
        /// <summary>
        /// Resets all values to their default state
        /// </summary>
        private void Reset()
        {
            this.currentCountdownTime = this.countdownTime;
            this.countdownText.enabled = false;
            this.countdownAnimation.enabled = false;
            this.countdownAnimation.Rewind();
        }
        
        /// <summary>
        /// Sets the <see cref="GodRayFlicker.godRay"/> <see cref="GameObject"/> active
        /// </summary>
        [Client]
        private void EnableGodRay()
        {
            this.CmdEnableGodRay();
        }

        /// <summary>
        /// <see cref="EnableGodRay"/>
        /// </summary>
        /// <param name="_Sender"><see cref="FruitBehaviour.clientConnectionId"/></param>
        [Command(requiresAuthority = false)]
        private void CmdEnableGodRay(NetworkConnectionToClient _Sender = null)
        {
            this.RpcEnableGodRay(_Sender!.connectionId);
        }
        
        /// <summary>
        /// <see cref="EnableGodRay"/>
        /// </summary>
        /// <param name="_ClientConnectionId"><see cref="FruitBehaviour.clientConnectionId"/></param>
        [ClientRpc]
        private void RpcEnableGodRay(int _ClientConnectionId)
        {
            if (_ClientConnectionId != this.container.ConnectionId)
            {
                return;
            }
            
            if (this.enableFlicker != null)
            {
                this.StopCoroutine(this.enableFlicker);
            }
            
            this.godRayFlicker.enabled = false;
            this.godRayFlicker.EnableGodRay();

            this.enableFlicker = this.EnableFlicker();
            this.StartCoroutine(this.enableFlicker);
        }
        
        /// <summary>
        /// Enables <see cref="godRayFlicker"/> after <see cref="timeBeforeStart"/>
        /// </summary>
        /// <returns></returns>
        private IEnumerator EnableFlicker()
        {
            yield return this.timeBeforeStart;
            this.godRayFlicker.enabled = true;
            this.enableFlicker = null;
        }
        #endregion
    }
}