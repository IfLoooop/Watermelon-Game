using System.Linq;
using Mirror;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Fruits;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Networking
{
    internal sealed class NetworkStoneFruitCharge : NetworkBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the StoneFruitCharge")]
        [SerializeField] private StoneFruitCharge stoneFruitCharge;
        [Tooltip("Prefab for the StoneFruit")]
        [SerializeField] private GameObject stoneFruitPrefab;
        #endregion

        #region Fields
        /// <summary>
        /// Reference to the <see cref="FruitSpawner"/> of the local player
        /// </summary>
        private static FruitSpawner fruitSpawner;
        #endregion
        
        #region Methods
        /// <summary>
        /// Spawns the given <see cref="Fruit"/> as a stone fruit
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to spawn as a stone fruit</param>
        [Client]
        public void SpawnStoneFruit(Fruit _Fruit)
        {
            this.CmdSpawnStoneFruit(_Fruit, fruitSpawner.transform.position);
        }

        /// <summary>
        /// <see cref="SpawnStoneFruit"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to spawn as a stone fruit</param>
        /// <param name="_Position">The position to spawn the <see cref="Fruit"/> at</param>
        /// <param name="_Sender">The client who requested the spawn</param>
        [Command(requiresAuthority = false)]
        private void CmdSpawnStoneFruit(Fruit _Fruit, Vector3 _Position, NetworkConnectionToClient _Sender = null)
        {
            if ((int)_Fruit > this.stoneFruitCharge.MaxFruitValue)
            {
                return;
            }

            var _fruitPrefab = FruitPrefabSettings.FruitPrefabs.First(_FruitPrefab => _FruitPrefab.Fruit == (int)_Fruit);
            var _stoneFruit = Instantiate(this.stoneFruitPrefab, _Position, Quaternion.identity).GetComponent<StoneFruitBehaviour>();
            
            _stoneFruit.Init(_fruitPrefab);
            
            NetworkServer.Spawn(_stoneFruit.gameObject);
            _stoneFruit.netIdentity.AssignClientAuthority(_Sender);
            
            this.RpcSpawnStoneFruit(_stoneFruit);
            this.TargetSpawnStoneFruit(_Sender, _stoneFruit);
        }
        
        /// <summary>
        /// <see cref="SpawnStoneFruit"/>
        /// </summary>
        /// <param name="_StoneFruit"></param>
        [ClientRpc]
        private void RpcSpawnStoneFruit(StoneFruitBehaviour _StoneFruit)
        {
            _StoneFruit.transform.SetParent(FruitContainer.Transform, true);
            FruitController.AddStoneFruit(_StoneFruit);
            AudioPool.PlayClip(AudioClipName.Shoot);
        }

        /// <summary>
        /// Shoots the stone fruit
        /// </summary>
        /// <param name="_Target">The client who requested the stone fruit</param>
        /// <param name="_StoneFruit">The spawned <see cref="StoneFruitBehaviour"/></param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetSpawnStoneFruit(NetworkConnectionToClient _Target, StoneFruitBehaviour _StoneFruit)
        {
            // TODO: Set achievements
            fruitSpawner.ShootStoneFruit(_StoneFruit, this.stoneFruitCharge.ShootForceMultiplier);
        }
        
        /// <summary>
        /// Sets the reference to the <see cref="FruitSpawner"/> of the local player
        /// </summary>
        /// <param name="_FruitSpawner">The <see cref="FruitSpawner"/> of the local player</param>
        public static void SetFruitSpawner(FruitSpawner _FruitSpawner)
        {
            fruitSpawner = _FruitSpawner;
        }
        #endregion
    }
}