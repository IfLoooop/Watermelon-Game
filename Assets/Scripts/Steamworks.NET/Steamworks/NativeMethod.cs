#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
	#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

#if UNITY_EDITOR_64 || (UNITY_STANDALONE && !UNITY_EDITOR && UNITY_64)
#define STEAMWORKS_X64
#elif UNITY_EDITOR_32 || (UNITY_STANDALONE && !UNITY_EDITOR && !UNITY_64)
	#define STEAMWORKS_X86
#endif

#if UNITY_EDITOR_WIN
#define STEAMWORKS_WIN
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
	#define STEAMWORKS_LIN_OSX
#elif UNITY_STANDALONE_WIN
	#define STEAMWORKS_WIN
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
	#define STEAMWORKS_LIN_OSX
#endif

#if !STEAMWORKS_WIN && !STEAMWORKS_LIN_OSX
	#error You must define STEAMWORKS_WIN or STEAMWORKS_LIN_OSX if you're not using Unity.
#endif

#define STEAMNETWORKINGSOCKETS_ENABLE_SDR

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

// ReSharper disable once CheckNamespace
namespace Steamworks {
    [System.Security.SuppressUnmanagedCodeSecurity()]
    internal static class NativeMethod {
        #if STEAMWORKS_WIN && STEAMWORKS_X64
		internal const string NativeLibraryName = "steam_api64";
		internal const string NativeLibrary_SDKEncryptedAppTicket = "sdkencryptedappticket64";
#else
		internal const string NativeLibraryName = "steam_api";
		internal const string NativeLibrary_SDKEncryptedAppTicket = "sdkencryptedappticket";
#endif

#region SteamUserStats
		//[DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamUserStats_UploadLeaderboardScore", CallingConvention = CallingConvention.Cdecl)]
		public static ulong ISteamUserStats_UploadLeaderboardScore(IntPtr instancePtr, SteamLeaderboard_t hSteamLeaderboard, ELeaderboardUploadScoreMethod eLeaderboardUploadScoreMethod, int nScore, [In, Out] int[] pScoreDetails, int cScoreDetailsCount) { return 0; }
#endregion
    }
}

#endif // !DISABLESTEAMWORKS
