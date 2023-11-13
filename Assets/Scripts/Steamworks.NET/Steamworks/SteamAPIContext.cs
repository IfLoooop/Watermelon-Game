#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

// ReSharper disable once CheckNamespace
namespace Steamworks {
	internal static class SteamAPIContext {
	    internal static bool Init() {
			return false;
		}
	    
	    internal static IntPtr GetSteamClient() { return IntPtr.Zero; }
		internal static IntPtr GetSteamUserStats() { return IntPtr.Zero; }
    }
}

#endif // !DISABLESTEAMWORKS
