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

        /// <summary>
        /// Gets the container from <see cref="containers"/> for the given client
        /// </summary>
        /// <param name="_Sender">The client who requested the container</param>
        /// <returns>The index of the container in <see cref="containers"/></returns>
        [Server]
        public static int GetContainerIndex(NetworkConnectionToClient _Sender)
        {
            return instance.containers.FindIndex(_Container => _Container.ConnectionId == _Sender.connectionId);
        }

        /// <summary>
        /// Assigns the container in <see cref="containers"/> with the given index to the given <see cref="_FruitSpawner"/>
        /// </summary>
        /// <param name="_FruitSpawner">The player object to assign to the container</param>
        /// <param name="_ContainerIndex">The index of the container in <see cref="containers"/></param>
        [Client]
        public static void AssignPlayerContainer(FruitSpawner _FruitSpawner, int _ContainerIndex)
        {
            foreach (var _kvp in NetworkServer.connections)
            {
                Debug.Log(_kvp);
            }
            
            instance.containers[_ContainerIndex].AssignToPlayer(_FruitSpawner);
        }
        #endregion
    }
}