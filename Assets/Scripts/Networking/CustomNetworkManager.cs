using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine;
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
        /// <summary>
        /// Singleton of <see cref="CustomNetworkManager"/>
        /// </summary>
        private static CustomNetworkManager instance;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="NetworkManager.maxConnections"/>
        /// </summary>
        public static int MaxConnection => instance.maxConnections;
        /// <summary>
        /// Is true while the client is trying to join a lobby <br/>
        /// <i>Will be set to false after joining or on failure</i>
        /// </summary>
        public static bool AttemptingToJoinALobby { get; private set; }
        #endregion
        
        #region Events
        /// <summary>
        /// Is called after an attempt to join a steam lobby <br/>
        /// <b>Parameter:</b> Indicates if the attempt failed
        /// </summary>
        public static event Action<bool> OnSteamLobbyEnterAttempt;
        #endregion
        
        #region Methods
        public override void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(base.gameObject);
                return;
            }
            
            base.Awake();
            instance = this;
            
#if DEBUG || DEVELOPMENT_BUILD
            if (instance.gameObject.GetComponent<NetworkManagerHUD>() == null)
            {
                instance.gameObject.AddComponent<NetworkManagerHUD>();   
            }
#endif
        }  

        public override void Start()
        {
            base.Start();
            base.StartHost();
        }

        public override void OnStopHost()
        {
            base.OnStopHost();
            
            GameController.Containers.Clear();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient _ConnectionToClient)
        {
            base.OnServerDisconnect(_ConnectionToClient);
            
            GameController.Containers.FirstOrDefault(_Container => _Container.ConnectionId == _ConnectionToClient.connectionId)?.FreeContainer();
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient _ConnectionToClient)
        {
            // base.OnServerAddPlayer(_ConnectionToClient);
            
            var _container = GameController.Containers.First(_Container => _Container.ConnectionId == null);
            var _fruitSpawner = Instantiate(base.playerPrefab, _container.StartingPosition.Value, base.playerPrefab.transform.rotation).GetComponent<FruitSpawner>();
            
            _fruitSpawner.SetConnectionId(_ConnectionToClient.connectionId);
            _container.AssignToPlayer(_fruitSpawner);
            
            NetworkServer.AddPlayerForConnection(_ConnectionToClient, _fruitSpawner.gameObject);
        }
        
        /// <summary>
        /// Makes sure only one join request can be sent at a time -> <see cref="AttemptingToJoinALobby"/>
        /// </summary>
        /// <returns>True when the client is currently not joining any lobby and is allowed to join, otherwise false</returns>
        [Client]
        public static bool AllowSteamLobbyJoinRequest()
        {
            if (AttemptingToJoinALobby)
            {
                return false;
            }

            return AttemptingToJoinALobby = true;
        }

        /// <summary>
        /// Attempts to join the lobby with the given host address
        /// </summary>
        /// <param name="_Failure">Indicates whether the lobby can be joined or not</param>
        /// <param name="_HostAddress">The address of the lobby to join</param>
        [Client]
        public static void SteamLobbyEnterAttempt(bool _Failure, string _HostAddress)
        {
            if (!_Failure && AttemptingToJoinALobby)
            {
                // TODO: Enable
                instance.networkAddress = _HostAddress;
                instance.StopHost();
                instance.StartClient();
            }
            
            OnSteamLobbyEnterAttempt?.Invoke(_Failure);
            AttemptingToJoinALobby = false;
        }
        
        /// <summary>
        /// Returns the connection id for every connected client and the index of the corresponding container in <see cref="GameController.containers"/> <br/>
        /// <b>Key:</b> Index of the container in <see cref="GameController.containers"/> <br/>
        /// <b>Value:</b> Connection od of the client that is assigned to the container
        /// </summary>
        /// <returns>The connection id for every connected client and the index of the corresponding container in <see cref="GameController.containers"/></returns>
        [Server]
        public static Dictionary<int, int> GetContainerConnectionMap()
        {
            var _dict = new Dictionary<int, int>();

            foreach (var _connectionId in NetworkServer.connections.Keys)
            {
                var _containerIndex = GameController.Containers.FindIndex(_Container => _Container.ConnectionId == _connectionId);
                _dict.Add(_containerIndex, _connectionId);
            }

            return _dict;
        }
        
        /// <summary>
        /// Assigns the the container in <see cref="containers"/> with the given index to the given connection id
        /// </summary>
        /// <param name="_FruitSpawner">
        /// If the container to assign belongs to the client, pass a reference of the <see cref="FruitSpawner"/> <br/>
        /// If it belongs to another player, pass null
        /// </param>
        /// <param name="_ContainerIndex">The index in <see cref="containers"/> to assign the connection to</param>
        /// <param name="_ConnectionId">The connection to assign</param>
        [Client]
        public static void AssignContainer([CanBeNull] FruitSpawner _FruitSpawner, int _ContainerIndex, int _ConnectionId)
        {
            if (_FruitSpawner != null)
            {
                GameController.Containers[_ContainerIndex].AssignToPlayer(_FruitSpawner);
            }
            else
            {
                GameController.Containers[_ContainerIndex].AssignConnectionId(_ConnectionId);
            }
        }
        #endregion
    }
}