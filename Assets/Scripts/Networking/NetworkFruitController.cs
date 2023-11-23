using Mirror;
using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Fruits;

namespace Watermelon_Game.Networking
{
    /// <summary>
    /// <see cref="FruitController"/> for network behaviour
    /// </summary>
    internal sealed class NetworkFruitController : NetworkBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the FruitController")]
        [SerializeField] private FruitController fruitController;
        #endregion
        
        #region Methods
        /// <summary>
        /// Spawns the given <see cref="Fruit"/> at the given position
        /// </summary>
        /// <param name="_NextFruitPosition">The position to spawn the evolved <see cref="Fruit"/> at</param>
        /// <param name="_Fruit">The <see cref="Fruit"/> type as a <see cref="ProtectedInt32"/></param>
        [Client]
        public void Evolve(Vector2 _NextFruitPosition, ProtectedInt32 _Fruit)
        {
            this.CmdEvolve(_NextFruitPosition, _Fruit);
        }
        
        /// <summary>
        /// <see cref="Evolve"/>
        /// </summary>
        /// <param name="_NextFruitPosition">The position to spawn the evolved <see cref="Fruit"/> at</param>
        /// <param name="_Fruit">The <see cref="Fruit"/> type as a <see cref="ProtectedInt32"/></param>
        /// <param name="_Sender"><see cref="NetworkBehaviour.connectionToClient"/></param>
        [Command(requiresAuthority = false)]
        private void CmdEvolve(Vector2 _NextFruitPosition, int _Fruit, NetworkConnectionToClient _Sender = null)
        {
            var _fruitBehaviour = FruitBehaviour.SpawnFruit(null, _NextFruitPosition, Quaternion.identity, _Fruit, true, _Sender);
            
            this.TargetEvolve(_Sender, _fruitBehaviour);
        }

        /// <summary>
        /// <see cref="Evolve"/>
        /// </summary>
        /// <param name="_Target"><see cref="NetworkBehaviour.connectionToClient"/></param>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> component of the evolved <see cref="Fruit"/></param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetEvolve(NetworkConnectionToClient _Target, FruitBehaviour _FruitBehaviour)
        {
            _FruitBehaviour.CmdEvolve();
            this.fruitController.AddFruit(_FruitBehaviour); // TODO: Not added to the dict on client
        }
        #endregion
    }
}