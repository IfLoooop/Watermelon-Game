using System;
using UnityEngine;

namespace Watermelon_Game.Utility.Pools
{
    /// <summary>
    /// Add this to the top most <see cref="GameObject"/> that contains the <see cref="UnityEngine.ParticleSystem"/>
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    [Serializable]
    internal sealed class ParticleWrapper : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Settings")]
        [Tooltip("Identifier of the particle in this ParticleWrapper")]
        [SerializeField] private ParticleName particleName;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="Watermelon_Game.Utility.Pools.ParticleName"/>
        /// </summary>
        public ParticleName ParticleName => this.particleName;
        /// <summary>
        /// <see cref="UnityEngine.ParticleSystem"/>
        /// </summary>
        public ParticleSystem ParticleSystem { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            this.ParticleSystem = base.GetComponent<ParticleSystem>();
        }
        
        private void OnDestroy()
        {
            ParticlePool.RemovePoolObject(this);
        }

        /// <summary>
        /// <see cref="ParticlePool.ReturnToPool"/>
        /// </summary>
        public void ReturnToPool()
        {
            ParticlePool.ReturnToPool(this);
        }
        #endregion
    }
}