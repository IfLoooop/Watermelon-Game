using UnityEngine;

namespace Watermelon_Game.Audio
{
    /// <summary>
    /// Wrapper <see cref="GameObject"/> for for the pool objects of the <see cref="AudioPool"/>
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    internal sealed class AudioWrapper : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// <see cref="AudioSource"/> that plays the <see cref="AudioClip"/>
        /// </summary>
        public AudioSource AudioSource { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            this.AudioSource = base.GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            AudioPool.RemovePoolObject(this);
        }

        /// <summary>
        /// <see cref="AudioPool.ReturnToPool"/>
        /// </summary>
        public void ReturnToPool()
        {
            AudioPool.ReturnToPool(this);
        }
        #endregion
    }
}