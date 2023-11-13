#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

using System.Text;
using UnityEngine.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Steamworks {
    public class InteroHelp {
        public static void TestIfPlatformSupported() {
#if !UNITY_EDITOR && !UNITY_STANDALONE && !STEAMWORKS_WIN && !STEAMWORKS_LIN_OSX
			throw new System.InvalidOperationException("Steamworks functions can only be called on platforms that Steam is available on.");
#endif
        }
        
        public static void TestIfAvailableClient() {
            TestIfPlatformSupported();
            if (SteamAPIContext.GetSteamClient() == System.IntPtr.Zero) {
                if (!SteamAPIContext.Init()) {
                    Debug.LogException("Steamworks is not initialized.");
                }
            }
        }
    }
}

#endif // !DISABLESTEAMWORKS
