using UnityEngine;

namespace Watermelon_Game.Utility.Pools
{
    /// <summary>
    /// Contains logic to group <see cref="ParticleName"/> together
    /// </summary>
    internal sealed class ParticleGroup
    {
        #region Properties
        /// <summary>
        /// Every <see cref="ParticleName"/> in this group
        /// </summary>
        public ParticleName[] Group { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="ParticleGroup"/>
        /// </summary>
        /// <param name="_Group"><see cref="Group"/></param>
        public ParticleGroup(ParticleName[] _Group)
        {
            this.Group = _Group;
        }
        #endregion

        #region Operators
        /// <summary>
        /// <see cref="GetRandom"/>
        /// </summary>
        /// <param name="_Group"><see cref="ParticleGroup"/></param>
        /// <returns><see cref="ParticleName"/></returns>
        public static implicit operator ParticleName(ParticleGroup _Group) => _Group.GetRandom(); 
        #endregion
        
        #region Methods
        /// <summary>
        /// Returns a random <see cref="ParticleName"/> from <see cref="Group"/>
        /// </summary>
        /// <returns>A random <see cref="ParticleName"/> from <see cref="Group"/></returns>
        public ParticleName GetRandom() => this.Group[Random.Range(0, this.Group.Length - 1)];
        #endregion
    }
}