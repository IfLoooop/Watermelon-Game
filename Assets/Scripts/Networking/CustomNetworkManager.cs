using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruit_Spawn;

namespace Watermelon_Game.Networking
{
    /// <summary>
    /// <see cref="NetworkManager"/>
    /// </summary>
    [RequireComponent(typeof(FizzySteamworks))]
    internal sealed class CustomNetworkManager : NetworkManager
    {
        #region Inspectoir Fields
        [Header("References")]
        [Tooltip("All available container")]
        [SerializeField] private List<ContainerBounds> containers;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="CustomNetworkManager"/>
        /// </summary>
        private static CustomNetworkManager instance;
        #endregion
        
        #region Methods
        public override void Awake()
        {
#if DEBUG || DEVELOPMENT_BUILD
            base.gameObject.AddComponent<NetworkManagerHUD>();
#endif
            base.Awake();
            instance = this;
        }  

        public override void Start()
        {
            base.Start();
            //base.StartHost(); // TODO: Enable
        }
        
        public override void OnServerDisconnect(NetworkConnectionToClient _ConnectionToClient)
        {
            base.OnServerDisconnect(_ConnectionToClient);
            
            this.containers.First(_Container => _Container.ConnectionId == _ConnectionToClient.connectionId).FreeContainer();
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient _ConnectionToClient)
        {
            // base.OnServerAddPlayer(_ConnectionToClient);
            
            var _container = this.containers.First(_Container => _Container.ConnectionId == null);
            var _fruitSpawner = Instantiate(base.playerPrefab, _container.StartingPosition.Value, base.playerPrefab.transform.rotation).GetComponent<FruitSpawner>();
            
            _fruitSpawner.SetConnectionId(_ConnectionToClient.connectionId);
            _container.AssignToPlayer(_fruitSpawner);
            
            NetworkServer.AddPlayerForConnection(_ConnectionToClient, _fruitSpawner.gameObject);
        }
        
        
        [Server]
        public static Dictionary<int, int> GetContainerIndex(NetworkConnectionToClient _Sender)
        {
            var _dict = new Dictionary<int, int>();

            foreach (var _connectionId in NetworkServer.connections.Keys)
            {
                var _containerIndex = instance.containers.FindIndex(_Container => _Container.ConnectionId == _connectionId);
                _dict.Add(_containerIndex, _connectionId);
            }

            return _dict;
        }
        
        
        [Client]
        public static void AssignContainer([CanBeNull] FruitSpawner _FruitSpawner, int _ContainerIndex, int _ConnectionId)
        {
            if (_FruitSpawner != null)
            {
                instance.containers[_ContainerIndex].AssignToPlayer(_FruitSpawner);
            }
            else
            {
                instance.containers[_ContainerIndex].AssignConnectionId(_ConnectionId);
            }
        }
        #endregion
    }
}