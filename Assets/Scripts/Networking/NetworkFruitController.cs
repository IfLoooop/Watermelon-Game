using Mirror;
using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Fruits;

namespace Watermelon_Game.Networking
{
    internal sealed class NetworkFruitController : NetworkBehaviour
    {
        #region Methods
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!base.isServer)
            {
                Debug.Log(base.netIdentity.netId);
                this.CmdAssignAuthority(this);
            }
        }
        
        [Command(requiresAuthority = false)]
        private void CmdAssignAuthority(NetworkFruitController _NetworkFruitController, NetworkConnectionToClient _Sender = null) // TODO: Probably not needed
        {
            _NetworkFruitController.netIdentity.AssignClientAuthority(_Sender);
        }
        
        [Client]
        public void Evolve(Vector2 _NextFruitPosition, ProtectedInt32 _Fruit)
        {
            Debug.Log($"[NetworkFruitController] Evolve | base: {base.netId}");
            this.CmdEvolve(_NextFruitPosition, _Fruit);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdEvolve(Vector2 _NextFruitPosition, int _Fruit, NetworkConnectionToClient _Sender = null)
        {
            Debug.Log($"[NetworkFruitController] CmdEvolve | netId: {_Sender?.identity.netId} | _Sender: {_Sender} | connectionId: {_Sender?.connectionId} | address: {_Sender?.address}");
            var _fruitBehaviour = FruitBehaviour.SpawnFruit(FruitController.FruitContainerTransform, _NextFruitPosition, Quaternion.identity, _Fruit, true);
            NetworkServer.Spawn(_fruitBehaviour.gameObject);
            _fruitBehaviour.netIdentity.AssignClientAuthority(_Sender);
            this.TargetEvolve(_Sender, _fruitBehaviour);
        }

        [TargetRpc]
        private void TargetEvolve(NetworkConnectionToClient _Target, FruitBehaviour _FruitBehaviour)
        {
            Debug.Log($"[NetworkFruitController] TargetEvolve | base: {base.netId}");
            _FruitBehaviour.CmdEvolve();
            FruitController.AddFruit(_FruitBehaviour);
        }
        #endregion
    }
}