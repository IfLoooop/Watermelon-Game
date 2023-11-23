using System.Collections.Generic;
using System.Linq;
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
        #region Fields
        [Header("References")]
        [Tooltip("All available container")]
        [SerializeField] private List<ContainerBounds> containers;
        #endregion
        
        #region Methods
#if DEBUG || DEVELOPMENT_BUILD
        public override void Awake()
        {
            base.Awake();

            base.gameObject.AddComponent<NetworkManagerHUD>();
        }  
#endif

        public override void Start()
        {
            base.Start();
            base.StartHost();
        }
        
        public override void OnServerConnect(NetworkConnectionToClient _ConnectionToClient)
        {
            base.OnServerConnect(_ConnectionToClient);
            
            // TODO: Handle connect
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
            
            _fruitSpawner.SetConnectionId(_ConnectionToClient);
            _container.AssignToPlayer(_fruitSpawner);
            NetworkServer.AddPlayerForConnection(_ConnectionToClient, _fruitSpawner.gameObject);
        }
        #endregion
    }
}