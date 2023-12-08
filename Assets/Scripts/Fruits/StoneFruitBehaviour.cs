using Mirror;
using UnityEngine;
using Watermelon_Game.Audio;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Main logic for all stone fruits
    /// </summary>
    internal sealed class StoneFruitBehaviour : NetworkBehaviour
    {
#pragma warning disable CS0109
        #region Inspector Fields
        [Header("References")]
        [Tooltip("SpriteRenderer that displays the fruit")]
        [SerializeField] private SpriteRenderer fruitSprite;
        [Tooltip("The SpriteMask component")]
        [SerializeField] private SpriteMask spriteMask;
        [Tooltip("Reference to the RigidBody2D component")]
        [SerializeField] private new Rigidbody2D rigidbody2D;
        #endregion
#pragma warning restore CS0109
        
        #region Methods
        private void OnDestroy()
        {
            if (!GameController.IsApplicationQuitting)
            {
                AudioPool.PlayClip(AudioClipName.FruitDestroy, true);
            }
        }

        /// <summary>
        /// Initializes all needed values
        /// </summary>
        /// <param name="_FruitPrefab">The <see cref="FruitPrefab"/> to get the values from</param>
        public void Init(FruitPrefab _FruitPrefab)
        {
            this.fruitSprite.sprite = _FruitPrefab.Sprite;
            this.spriteMask.sprite = _FruitPrefab.Sprite;
            base.transform.localScale = _FruitPrefab.Scale;
        }
        
        /// <summary>
        /// Shoots the fruit with increased force in  the given direction
        /// </summary>
        /// <param name="_Direction">The direction to shoot the fruit in</param>
        /// <param name="_ShootForce">Multiplier for the force with which the fruit is shot</param>
        public void Shoot(Vector2 _Direction, float _ShootForce)
        {
            this.rigidbody2D.AddForce(_Direction * (_ShootForce * this.rigidbody2D.mass), ForceMode2D.Impulse);
        }
        
        /// <summary>
        /// Destroys this <see cref="GameObject"/>
        /// </summary>
        public void DestroyFruit()
        {
            // TODO: Add a visual animation
            Destroy(base.gameObject);
            if (!base.isServer)
            {
                this.CmdDestroyFruit(base.gameObject);
            }
        }

        /// <summary>
        /// <see cref="DestroyFruit"/>
        /// </summary>
        /// <param name="_StoneFruit">The StoneFruit <see cref="GameObject"/> to destroy</param>
        [Command(requiresAuthority = false)]
        private void CmdDestroyFruit(GameObject _StoneFruit)
        {
            if (_StoneFruit != null)
            {
                NetworkServer.Destroy(_StoneFruit);
            }
        }
        #endregion
    }
}