using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using Mirror.FizzySteam;
using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Menus;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Networking
{
    /// <summary>
    /// <see cref="NetworkManager"/>
    /// </summary>
    [RequireComponent(typeof(FizzySteamworks))]
    internal sealed class CustomNetworkManager : NetworkManager
    {
        #region Constants
        /// <summary>
        /// The default value for <see cref="NetworkManager.networkAddress"/>
        /// </summary>
        private const string DEFAULT_NETWORK_ADDRESS = "localhost";
        #endregion

        #region Fields
        /// <summary>
        /// The connection id of the host
        /// </summary>
        private ProtectedInt32? hostConnectionId;
        /// <summary>
        /// Will be true when connection to another host as a client <br/>
        /// <i>Will be true for the duration of the connection, reset in <see cref="DisconnectFromLobby"/></i>
        /// </summary>
        private static ProtectedBool attemptingToConnectToLobby;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="NetworkManager.maxConnections"/>
        /// </summary>
        public static ProtectedInt32 MaxConnections => singleton.maxConnections;
        /// <summary>
        /// Is true while the client is trying to join a lobby <br/>
        /// <i>Will be set to false after joining or on failure</i>
        /// </summary>
        public static ProtectedBool AttemptingToJoinALobby { get; private set; }
        #endregion
        
        #region Events
        /// <summary>
        /// Is called when a host or client connection has been stopped
        /// </summary>
        public static event Action OnConnectionStopped;
        /// <summary>
        /// Is called after an attempt to join a steam lobby <br/>
        /// <b>Parameter:</b> Indicates if the attempt failed
        /// </summary>
        public static event Action<bool> OnSteamLobbyEnterAttempt;
        #endregion
        
        #region Methods
        public override void Awake()
        {
            if (singleton != null && singleton != this)
            {
                Destroy(base.gameObject);
                return;
            }
            
#if DEBUG
            base.gameObject.AddComponent<NetworkManagerHUD>();
#endif
            base.Awake();
        }
        
        public override void Start()
        {
            base.Start();
            base.StartHost();
        }
        
        public override void OnStartHost()
        {
            base.OnStartHost();
            GameController.Containers.ForEach(_Container => _Container.FreeContainer());
        }
        
        public override void OnStopHost()
        {
            base.OnStopHost();
            OnConnectionStopped?.Invoke();
        }
        
        public override void OnStopClient() // Will only be called when connected AS a client to another host
        {
            base.OnStopClient();
            ConnectionCancelled();
        }
        
        /// <summary>
        /// Determines whether the client was stopped normally or due to an error
        /// </summary>
        private static void ConnectionCancelled()
        {
            if (attemptingToConnectToLobby)
            {
                attemptingToConnectToLobby = false;
                SteamLobby.LeaveLobby();
                MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.ConnectionErrorMenu);
            }
            
            OnConnectionStopped?.Invoke();
        }
        
        public override void OnServerConnect(NetworkConnectionToClient _ConnectionToClient)
        {
            if (!SteamLobby.IsHost.Value.Value)
            {
                _ConnectionToClient.Disconnect();
            }
            
            base.OnServerConnect(_ConnectionToClient);
        }
        
        public override void OnServerDisconnect(NetworkConnectionToClient _ConnectionToClient)
        {
            base.OnServerDisconnect(_ConnectionToClient);
            GameController.Containers.FirstOrDefault(_Container => _Container.ConnectionId == _ConnectionToClient.connectionId)?.FreeContainer();

            if (GameController.ActiveGame)
            {
                
            }
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient _ConnectionToClient)
        {
            // base.OnServerAddPlayer(_ConnectionToClient);
            
            var _container = GameController.Containers.First(_Container => _Container.ConnectionId == null);
            var _fruitSpawner = Instantiate(base.playerPrefab, _container.StartingPosition.Value, base.playerPrefab.transform.rotation).GetComponent<FruitSpawner>();
            
            _fruitSpawner.SetConnectionId(_ConnectionToClient.connectionId);
            _container.AssignToPlayer(_fruitSpawner);
            
            NetworkServer.AddPlayerForConnection(_ConnectionToClient, _fruitSpawner.gameObject);
            _fruitSpawner.SetSteamData(_ConnectionToClient);

            if (this.hostConnectionId == null)
            {
                this.hostConnectionId = _ConnectionToClient.connectionId;
            }
            else
            {
                _fruitSpawner.SendSteamIdToHost(_ConnectionToClient);
            }
        }
        
        /// <summary>
        /// Makes sure only one join request can be sent at a time -> <see cref="AttemptingToJoinALobby"/>
        /// </summary>
        /// <returns>True when the client is currently not joining any lobby and is allowed to join, otherwise false</returns>
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
        public static void SteamLobbyEnterAttempt(bool _Failure, string _HostAddress = DEFAULT_NETWORK_ADDRESS)
        {
            if (!_Failure && AttemptingToJoinALobby)
            {
                singleton.networkAddress = _HostAddress;
                singleton.StopHost();
                //singleton.StartClient(); // TODO: Try starting the client here
            }
            else
            {
                AttemptingToJoinALobby = false;
            }
            
            OnSteamLobbyEnterAttempt?.Invoke(_Failure);
        }
        
        /// <summary>
        /// Starts a client connection to <see cref="NetworkManager.networkAddress"/>
        /// </summary>
        public static void Connect()
        {
            AttemptingToJoinALobby = false;
            attemptingToConnectToLobby = true;
            singleton.StartClient();
        }
        
        /// <summary>
        /// Cancels the current lobby connection process
        /// </summary>
        public static void CancelConnectionAttempt()
        {
            AttemptingToJoinALobby = false;
            singleton.StartHost();
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
            var _map = new Dictionary<int, int>();

            foreach (var _connectionId in NetworkServer.connections.Keys)
            {
                var _containerIndex = GameController.Containers.FindIndex(_Container => _Container.ConnectionId == _connectionId);
                _map.Add(_containerIndex, _connectionId);
            }

            return _map;
        }
        
        /// <summary>
        /// Assigns the the container in <see cref="GameController.containers"/> with the given index to the given connection id
        /// </summary>
        /// <param name="_FruitSpawner">
        /// If the container to assign belongs to the local client, pass a reference of the <see cref="FruitSpawner"/> <br/>
        /// If it belongs to another player, pass null
        /// </param>
        /// <param name="_ContainerIndex">The index in <see cref="GameController.containers"/> to assign the connection to</param>
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

        /// <summary>
        /// Disconnects a client from a lobby <br/>
        /// <b>Won't work for the host of a lobby</b>
        /// </summary>
        /// <param name="_IsHost"><see cref="SteamLobby.IsHost"/></param>
        public static void DisconnectFromLobby(bool _IsHost)
        {
            singleton.networkAddress = DEFAULT_NETWORK_ADDRESS;
            AttemptingToJoinALobby = false;
            attemptingToConnectToLobby = false;
            
            if (!_IsHost)
            {
                singleton.StopClient();
                singleton.StopServer();
                singleton.StartHost();
            }
        }
        #endregion
    }
}