using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Utility.Pools
{
    /// <summary>
    /// Contains method to play pooled <see cref="ParticleSystem"/>
    /// </summary>
    internal sealed class ParticlePool : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("All possible particles to instantiate")]
        [SerializeField] private List<ParticleWrapper> particleWrappers;
        
        [Header("Debug")]
        [Tooltip("Contains the GameObjects that play the Particle")]
        [ShowInInspector] private Dictionary<ParticleName, ObjectPool<ParticleWrapper>> particlePool = new();
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="ParticlePool"/>
        /// </summary>
        private static ParticlePool instance;
        #endregion

        #region Properties
        /// <summary>
        /// Explosion particles with text inside them
        /// </summary>
        public ParticleGroup TextExplosions;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;

            foreach (var _particleWrapper in this.particleWrappers)
            {
                this.particlePool.Add(_particleWrapper.ParticleName, new ObjectPool<ParticleWrapper>(_particleWrapper, this.transform, 1));
            }
            
            this.InitGroups();
        }

        /// <summary>
        /// Plays a random particle from the given <see cref="ParticleGroup"/> at the given position
        /// </summary>
        /// <param name="_Group">A group from <see cref="ParticlePool"/></param>
        /// <param name="_Position">The position to spawn the particle at</param>
        public static void PlayRandomParticle(Func<ParticlePool, ParticleGroup> _Group, Vector3 _Position)
        {
            PlayParticle(_Group.Invoke(instance), _Position);
        }
        
        /// <summary>
        /// Plays the particle with the given <see cref="ParticleName"/> in <see cref="particlePool"/> at the given position <br/>
        /// <i>Assumes the <see cref="ParticleSystem"/> is set to <see cref="ParticleSystem.MainModule.playOnAwake"/></i>
        /// </summary>
        /// <param name="_ParticleName">The particle to play</param>
        /// <param name="_Position">The position to spawn the particle at</param>
        public static void PlayParticle(ParticleName _ParticleName, Vector3 _Position)
        {
            var _particleWrapper = instance.particlePool[_ParticleName].Get(_Position);
            
            _particleWrapper.Invoke(nameof(_particleWrapper.ReturnToPool), _particleWrapper.ParticleSystem.main.duration);
        }
        
        /// <summary>
        /// Returns the given <see cref="ParticleWrapper"/> back to <see cref="particlePool"/>
        /// </summary>
        /// <param name="_ParticleWrapper">The <see cref="ParticleWrapper"/> to return to <see cref="particlePool"/></param>
        public static void ReturnToPool(ParticleWrapper _ParticleWrapper)
        {
            instance.particlePool[_ParticleWrapper.ParticleName].Return(_ParticleWrapper);
        }

        /// <summary>
        /// Removes the given <see cref="ParticleWrapper"/> from <see cref="ObjectPool{T}.objectPool"/>
        /// </summary>
        /// <param name="_ParticleWrapper">The <see cref="ParticleWrapper"/> to remove from <see cref="ObjectPool{T}.objectPool"/></param>
        public static void RemovePoolObject(ParticleWrapper _ParticleWrapper)
        {
            instance.particlePool[_ParticleWrapper.ParticleName].Remove(_ParticleWrapper);
        }

        /// <summary>
        /// Use this method to initialize all groups
        /// </summary>
        private void InitGroups()
        {
            this.TextExplosions = new ParticleGroup(new []
            {
                ParticleName.Bang,
                ParticleName.Blam,
                ParticleName.Boom,
                ParticleName.Crack,
                ParticleName.Crash,
                ParticleName.Critical,
                ParticleName.Hit,
                ParticleName.KaPow,
                ParticleName.Omg,
                ParticleName.Poof,
                ParticleName.Pop,
                ParticleName.Pow,
                ParticleName.Smack,
                ParticleName.Smooch,
                ParticleName.Whammm,
                ParticleName.Wow,
                ParticleName.Wtf,
                ParticleName.Zap
            });
        }
        #endregion
    }
}