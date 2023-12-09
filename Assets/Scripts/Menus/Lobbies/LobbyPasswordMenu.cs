using System;
using System.Collections;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Audio;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility.Pools;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu for entering a password to join a lobby
    /// </summary>
    internal sealed class LobbyPasswordMenu : MenuBase
    {
        #region Inspector Fields
        [Tooltip("Password inputfield")]
        [PropertyOrder(1)][SerializeField] private TMP_InputField inputField;
        [Tooltip("Reference to the submit button")]
        [PropertyOrder(1)][SerializeField] private Button submitButton;
        [Tooltip("Reference to the button that shows/hides the password")]
        [PropertyOrder(1)][SerializeField] private Button hideButton;
        [Tooltip("Hide Password Sprite")]
        [PropertyOrder(1)][SerializeField] private Sprite hideSprite;
        [Tooltip("Show Password Sprite")]
        [PropertyOrder(1)][SerializeField] private Sprite showSprite;
        
        [Tooltip("AnimationCurve for the strength of the shake")]
        [PropertyOrder(2)][SerializeField] private AnimationCurve curve;
        [Tooltip("The duration in seconds of the shake effect")]
        [PropertyOrder(2)][SerializeField] private float shakeDuration = .375f;
        #endregion

        #region Fields
        /// <summary>
        /// The id of the lobby to join
        /// </summary>
        private ProtectedUInt64 lobbyId;
    
        /// <summary>
        /// Initial position of this <see cref="LobbyPasswordMenu"/>
        /// </summary>
        private Vector3 startPosition;
        /// <summary>
        /// Reference to the running <see cref="Shake"/> coroutine
        /// </summary>
        [CanBeNull] private IEnumerator shake;
        #endregion
        
        #region Properties
        /// <summary>
        /// Indicates whether this <see cref="LobbyPasswordMenu"/> is currently open or not
        /// </summary>
        public ProtectedBool IsOpen { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Called when the <see cref="LobbyPasswordMenu"/> is being closed
        /// </summary>
        public static event Action OnLobbyPasswordMenuClose;
        #endregion
        
        #region Methods
        /// <summary>
        /// Opens the <see cref="LobbyPasswordMenu"/>
        /// </summary>
        /// <param name="_LobbyId">The id of the lobby to enter the password for</param>
        public void Open(ulong _LobbyId)
        {
            this.IsOpen = true;
            this.lobbyId = _LobbyId;
            this.startPosition = base.transform.position;
            base.Open(null);
            this.inputField.Select();
            this.submitButton.interactable = true;
        }
        
        public override void Close(bool _PlaySound)
        {
            this.IsOpen = false;
            this.inputField.text = string.Empty;

            if (this.shake != null)
            {
                base.StopCoroutine(this.shake);
                this.FinalizeShake();
            }
            
            base.Close(_PlaySound);
            this.SwitchContentType(TMP_InputField.ContentType.Standard);
            OnLobbyPasswordMenuClose?.Invoke();
        }
        
        /// <summary>
        /// Hides/shows the text input
        /// </summary>
        public void Hide()
        {
            this.SwitchContentType(this.inputField.contentType);
            this.inputField.ForceLabelUpdate();
        }

        /// <summary>
        /// Switches the <see cref="TMP_InputField.contentType"/> of <see cref="inputField"/>
        /// </summary>
        /// <param name="_ContentType">The <see cref="TMP_InputField.ContentType"/> to switch from</param>
        private void SwitchContentType(TMP_InputField.ContentType _ContentType)
        {
            if (_ContentType == TMP_InputField.ContentType.Standard)
            {
                this.inputField.contentType = TMP_InputField.ContentType.Password;
                this.hideButton.image.sprite = this.showSprite;
            }
            else
            {
                this.inputField.contentType = TMP_InputField.ContentType.Standard;
                this.hideButton.image.sprite = this.hideSprite;
            }
        }
        
        /// <summary>
        /// Submits the text input
        /// </summary>
        /// <param name="_SubmitThroughButton">Indicates that the button was used to submit</param>
        public void Submit(bool _SubmitThroughButton)
        {
            if (_SubmitThroughButton || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                this.submitButton.interactable = false;
                SteamLobby.JoinLobbyAsync(this.lobbyId, this.inputField.text);
                // TODO: Show waiting indicator
            }
        }
        
        /// <summary>
        /// Call this when the attempt to enter a lobby failed for whatever reason
        /// </summary>
        public void EnterAttemptFailed()
        {
            if (this.IsOpen && this.shake == null)
            {
                AudioPool.PlayClip(AudioClipName.InputError);
                this.shake = Shake();
                base.StartCoroutine(this.shake);
            }
        }

        /// <summary>
        /// Shakes the <see cref="LobbyPasswordMenu"/>
        /// </summary>
        /// <returns></returns>
        private IEnumerator Shake()
        {
            var _elapsedTime = 0f;

            while (_elapsedTime < this.shakeDuration)
            {
                _elapsedTime += Time.deltaTime;
                base.transform.position = this.startPosition + Random.insideUnitSphere * this.curve.Evaluate(_elapsedTime / this.shakeDuration);
                yield return null;
            }
            
            this.FinalizeShake();
        }

        /// <summary>
        /// Resets the values needed for <see cref="Shake"/> to their default state
        /// </summary>
        private void FinalizeShake()
        {
            base.transform.position = this.startPosition;
            this.shake = null;
            this.submitButton.interactable = true;
        }
        #endregion
    }
}