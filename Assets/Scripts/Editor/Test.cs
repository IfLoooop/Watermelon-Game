// using Mirror;
// using Steamworks;
// using UnityEngine;
//
// namespace Watermelon_Game.Editor
// {
//     internal sealed class Test
//     {
//         [Serializable]
//         [InlineProperty(LabelWidth = 50)]
//         // ReSharper disable once InconsistentNaming
//         internal sealed class BackingField<T>
//         {
// #if UNITY_EDITOR
//             #region Inspector Fields
//             [Tooltip("Holds the value of \"Property\"")]
//             [ShowInInspector][ReadOnly] private object value;
//             #endregion  
//
//             #region Fields
//             /// <summary>
//             /// Backing field for <see cref="Property"/>
//             /// </summary>
//             private T backingField;
//             #endregion
// #endif
//         
//             #region Properties
//             /// <summary>
//             /// 
//             /// </summary>
//             public T Property
//             {
// #if UNITY_EDITOR
//                 get => this.backingField;
//                 set
//                 {
//                     this.backingField = value;
//                     this.value = this.backingField;
//                 }
// #else
//             get; set;
// #endif
//             }
//             #endregion
//         }
//
//         internal sealed class SteamLobby
//         {
//             [ShowInInspector][ReadOnly] public static BackingField<ProtectedUInt64?> CurrentLobbyId { get; private set; }
//         }
//     }
// }