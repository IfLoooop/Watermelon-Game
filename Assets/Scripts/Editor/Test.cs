// using Mirror;
// using UnityEngine;
//
// namespace Watermelon_Game.Editor
// {
//     internal sealed class Test
//     {
//         internal sealed class FruitSpawner
//         {
//             [Client]
//             private void ReleaseFruit()
//             {
//                 this.CmdReleaseFruit(this.fruitBehaviour, -this.fruitSpawnerAim.transform.up);
//             }
//             [Command(requiresAuthority = false)]
//             private void CmdReleaseFruit(FruitBehaviour _FruitBehaviour, Vector2 _AimRotation, NetworkConnectionToClient _Sender = null)
//             {
//                 if (_Sender == base.netIdentity.connectionToClient)
//                 {
//                     this.TargetRelease(_Sender, _FruitBehaviour, _AimRotation);
//                 }
//             }
//             [TargetRpc]
//             private void TargetRelease(NetworkConnectionToClient _Target, FruitBehaviour _FruitBehaviour, Vector2 _AimRotation)
//             {
//                 _FruitBehaviour.CmdRelease(_AimRotation);
//             }
//         }
//         internal sealed class FruitBehaviour
//         {
//             [Command(requiresAuthority = false)]
//             public void CmdRelease(Vector2 _AimRotation, NetworkConnectionToClient _Sender = null)
//             {
//                 this.TargetRelease(_Sender, _AimRotation);
//             }
//             [TargetRpc]
//             private void TargetRelease(NetworkConnectionToClient _Target, Vector2 _AimRotation)
//             { 
//                 base.transform.SetParent(FruitController.FruitContainerTransform, true);
//                 this.HasBeenReleased = true;
//                 this.DecreaseSortingOrder();
//                 this.InitializeRigidBody();
//                 this.fruitsFirstCollision.SetActive();
//                 this.UseSkill(_AimRotation);
//                 OnFruitRelease?.Invoke(this);
//             }
//         }
//     }
// }