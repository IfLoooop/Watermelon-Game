using System;
using System.Collections;
using JetBrains.Annotations;
using Mirror;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Networking;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;
using Watermelon_Game.Utility.Pools;

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
        [Tooltip("Animation that is played when a fruit touches the border")]
        [SerializeField] private Animation borderLineAnimation;
        [Tooltip("Reference to the background GameObject of the visible countdown")]
        [SerializeField] private GameObject countdownBackground;
        [Tooltip("Displays the current countdown")]
        [SerializeField] private TextMeshProUGUI countdownText;
        
        [Header("Settings")]
        [Tooltip("Total duration in seconds of the countdown")]
        [SerializeField] private ProtectedFloat countdownStartTime = 5f;
        [Tooltip("Grace time before the visible countdown starts")]
        [SerializeField] private ProtectedFloat countdownGraceTime = 3f;
        [Tooltip("Duration in seconds the god ray stays visible before deactivating itself again")]
        [SerializeField] private float godRayDuration = 1.5f;
        
        [Header("Debug")]
        [Tooltip("The current value of the countdown")]
        [SerializeField][ReadOnly] private ProtectedFloat countdown;
        #endregion
        
        #region Fields
        /// <summary>
        /// Detects only <see cref="Fruits.Fruit"/>s
        /// </summary>
        private BoxCollider2D trigger;
        /// <summary>
        /// <see cref="Animation"/> that plays after a <see cref="Fruits.Fruit"/> has been to long above the height limit
        /// </summary>
        private Animation countdownAnimation;
        
        /// <summary>
        /// Will be true, as long as a fruit is inside the trigger
        /// </summary>
        private ProtectedBool triggerEntered;
        /// <summary>
        /// Time in seconds of the first fruit that entered the trigger, during a continuous trigger activation <br/>
        /// <i>Can only be set when no fruit is currently inside the trigger</i>
        /// </summary>
        private ProtectedFloat enteredTimestamp;
        /// <summary>
        /// Indicates whether the visible countdown has started or not
        /// </summary>
        private ProtectedBool countdownStarted;
        
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
        /// Is called when <see cref="countdown"/> reaches 0 <br/>
        /// <b>Parameter:</b> The steam id of the client that lost
        /// </summary>
        public static event Action<ulong> OnGameOver;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.trigger = base.GetComponent<BoxCollider2D>();
            this.countdownAnimation = base.GetComponent<Animation>();
            this.countdown = this.countdownStartTime;
            
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
        
        private void OnTriggerStay2D(Collider2D _Other)
        {
            if (!this.triggerEntered)
            {
                this.triggerEntered = true;
                this.enteredTimestamp = Time.time;
            }
            if (!this.borderLineAnimation.isPlaying)
            {
                this.borderLineAnimation.Play();
            }
            else
            {
                if (Time.time >= this.enteredTimestamp + this.countdownGraceTime)
                {
                    if (!this.countdownStarted)
                    {
                        this.countdownStarted = true;
                        base.InvokeRepeating(nameof(this.CountDown), 0, 1);
                    }
                }
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
        [Client]
        private void CountDown()
        {
            if (!NetworkGameController.ClientHasJoinedLobby)
            {
                this.CmdCountdown(SteamManager.SteamID.m_SteamID);   
            }
        }

        /// <summary>
        /// <see cref="CountDown"/>
        /// </summary>
        /// <param name="_SteamId">The steam id of the player who triggered the countdown</param>
        [Command(requiresAuthority = false)]
        private void CmdCountdown(ulong _SteamId)
        {
            this.RpcCountdown(_SteamId);
        }
        
        /// <summary>
        /// <see cref="CountDown"/>
        /// </summary>
         /// <param name="_SteamId">The steam id of the player who triggered the countdown</param>
        [ClientRpc]
        private void RpcCountdown(ulong _SteamId)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (DisableCountDown)
            {
                return;
            }
#endif
            
            if (this.countdown <= 0)
            {
                // Needs to be set immediately, otherwise "CmdGameOver()" can be called multiple times when called by a client (not host) while waiting for the from the host
                this.countdown = this.countdownStartTime;
                this.Reset();
                this.CmdGameOver(_SteamId);
                return;
            }
            
            this.countdownBackground.transform.localScale = Vector3.zero;
            this.countdownText.text = this.countdown.ToString();
            this.countdownBackground.SetActive(true);
            this.countdownAnimation.Play();
            
            // Sound is only player for the local client
            if (this.container.PlayerContainer)
            {
                AudioPool.PlayClip(AudioClipName.Countdown);   
            }
            
            this.countdown--;
        }

        /// <summary>
        /// Tells the server this client has lost
        /// </summary>
        /// <param name="_SteamId">The steam id of the player that lost</param>
        /// <param name="_Sender">The local client</param>
        [Command(requiresAuthority = false)]
        private void CmdGameOver(ulong _SteamId, NetworkConnectionToClient _Sender = null)
        {
            this.TargetGameOver(_Sender, _SteamId);
        }

        /// <summary>
        /// Tell every client the game is over
        /// </summary>
        /// <param name="_Target">The local client</param>
        /// <param name="_SteamId">The steam id of the player that lost</param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetGameOver(NetworkConnectionToClient _Target, ulong _SteamId)
        {
            OnGameOver?.Invoke(_SteamId);
        }
        
        /// <summary>
        /// Resets all values to their default state
        /// </summary>
        [Client]
        private void Reset()
        {
            this.CmdReset();
        }

        /// <summary>
        /// <see cref="Reset"/>
        /// </summary>
        [Command(requiresAuthority = false)]
        private void CmdReset()
        {
            this.RpcReset();
        }
        
        /// <summary>
        /// <see cref="Reset"/>
        /// </summary>
        [ClientRpc]
        private void RpcReset()
        {
            base.CancelInvoke(nameof(CountDown));
            this.countdownAnimation.Stop();
            this.countdown = this.countdownStartTime;
            this.triggerEntered = false;
            this.countdownStarted = false;
            this.countdownBackground.SetActive(false);
        }
        
        /// <summary>
        /// Sets the <see cref="GodRayFlicker.godRay"/> <see cref="GameObject"/> active
        /// </summary>
        [Client]
        private void EnableGodRay()
        {
            if (this.enableFlicker != null)
            {
                return;
            }
            
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