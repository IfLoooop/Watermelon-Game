using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Menus.Leaderboards;
using Watermelon_Game.Points;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Steamworks.NET
{
    /// <summary>
    /// Contains logic for down/uploading to the Steam Leaderboard <br/>
    /// https://partner.steamgames.com/doc/features/leaderboards/guide
    /// </summary>
    internal sealed class SteamLeaderboard : MonoBehaviour
    {
        #region Constants
        /// <summary>
        /// The name of the leaderboard at: <br/>
        /// https://partner.steamgames.com/apps/leaderboards/2658820
        /// </summary>
        private const string LEADERBOARD_NAME = "Highscores";
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="SteamLeaderboard"/>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static SteamLeaderboard instance;
        /// <summary>
        /// The leaderboard that was found during <see cref="Init"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#SteamLeaderboard_t</i>
        /// </summary>
        private static SteamLeaderboard_t? steamLeaderboard;
        /// <summary>
        /// <b>Key:</b> The steam id <br/>
        /// <b>Value:</b> <see cref="LeaderboardUserData"/>
        /// </summary>
        private static List<LeaderboardUserData> steamUsers = new();
        /// <summary>
        /// Indicates whether <see cref="GetDownloadedLeaderboardScoresAsync"/> is currently running or not
        /// </summary>
        private static bool processingLeaderboardEntries;
        /// <summary>
        /// Indicates how many user information request have been made, e.g. how often the <see cref="GetUserName"/>-Method should be allowed to run. <br/>
        /// <i>
        /// Sometimes the <see cref="onPersonaStateChanged"/>-Callback is received without manually requesting it. <br/>
        /// A value less than 1 prevents the <see cref="GetUserName"/>-Method to run
        /// </i>
        /// </summary>
        private static uint userInformationRequested;
        /// <summary>
        /// Contains steam ids for users whose usernames still have to be requested
        /// </summary>
        private static readonly List<ulong> steamUserNameRequests = new();
        /// <summary>
        /// The current score in <see cref="steamLeaderboard"/> of <see cref="SteamManager.SteamID"/>
        /// </summary>
        private static ProtectedInt32 currentLeaderboardScore;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="steamUsers"/>
        /// </summary>
        public static List<LeaderboardUserData> SteamUsers => steamUsers;
        #endregion
        
        #region Callbacks
        /// <summary>
        /// Callback for when a leaderboard is found with <see cref="SteamUserStats.FindLeaderboard"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#LeaderboardFindResult_t</i>
        /// </summary>
        private static readonly CallResult<LeaderboardFindResult_t> onLeaderboardFound = new();
        /// <summary>
        /// Callback for when the scores have been downloaded with <see cref="SteamUserStats.DownloadLeaderboardEntries"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#LeaderboardScoresDownloaded_t</i>
        /// </summary>
        private static readonly CallResult<LeaderboardScoresDownloaded_t> onLeaderboardScoresDownloaded = new();
        /// <summary>
        /// Callback for when a score is uploaded to the leaderboard with <see cref="SteamUserStats.UploadLeaderboardScore"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#LeaderboardScoreUploaded_t</i>
        /// </summary>
        private static readonly CallResult<LeaderboardScoreUploaded_t> onLeaderboardScoreUploaded = new();
        /// <summary>
        /// Callback for when the user information of a specific steam id is requested with <see cref="SteamFriends.RequestUserInformation"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#PersonaStateChange_t</i>
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private static Callback<PersonaStateChange_t> onPersonaStateChanged;
        #endregion

        #region Events
        /// <summary>
        /// Is called when all scores of the <see cref="steamLeaderboard"/> have been downloaded -> <see cref="GetDownloadedLeaderboardScoresAsync"/>
        /// </summary>
        public static event Action OnLeaderboardScoresDownloaded;
        /// <summary>
        /// Is called everytime <see cref="GetUserName"/> finds a user <br/>
        /// <b>Parameter:</b> The index in <see cref="steamUsers"/>, for whom the username was found
        /// </summary>
        public static event Action<int> OnUsernameFound; 
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.Init();
        }

        private void OnEnable()
        {
            PointsController.OnPointsChanged += UploadScore;
        }

        private void OnDisable()
        {
            PointsController.OnPointsChanged -= UploadScore;
        }
        
        /// <summary>
        /// Initializes the steam leaderboard <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#FindLeaderboard</i>
        /// </summary>
        private void Init()
        {
            instance = this;
            
            if (!SteamManager.Initialized)
            {
                return;
            }
            
            var _steamAPICall = SteamUserStats.FindLeaderboard(LEADERBOARD_NAME);
            onLeaderboardFound.Set(_steamAPICall, OnLeaderboardFound);
            onPersonaStateChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
        }

        /// <summary>
        /// <see cref="onLeaderboardFound"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether a leaderboard with the given name has been found</param>
        private static void OnLeaderboardFound(LeaderboardFindResult_t _Callback, bool _Failure)
        {
            if (_Failure)
            {
                Debug.LogError($"Couldn't find the leaderboard with name: {LEADERBOARD_NAME}");
            }
            else
            {
                steamLeaderboard = _Callback.m_hSteamLeaderboard;
                
                DownloadLeaderboardScores();
            }
        }

#pragma warning disable CS0162 // Unreachable code detected
        /// <summary>
        /// Downloads all scores from <see cref="steamLeaderboard"/> <br/>
        /// <i>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#DownloadLeaderboardEntries <br/>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#ELeaderboardDataRequest
        /// </i>
        /// </summary>
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public static void DownloadLeaderboardScores()
        {
#if UNITY_EDITOR
            DownloadLeaderboardScores_DEVELOPMENT(SCORE_AMOUNT, false);
            return;
#endif
            if (!SteamManager.Initialized)
            {
                return;
            }
            
            if (steamLeaderboard is null)
            {
                Debug.LogError($"The leaderboard is not initialized [{nameof(DownloadLeaderboardScores)}]");
            }
            else
            {
                var _steamAPICall = SteamUserStats.DownloadLeaderboardEntries(steamLeaderboard.Value, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, int.MaxValue);
                onLeaderboardScoresDownloaded.Set(_steamAPICall, GetDownloadedLeaderboardScoresAsync);
            }
        }
#pragma warning restore CS0162 // Unreachable code detected

        /// <summary>
        /// <see cref="onLeaderboardScoresDownloaded"/> <br/>
        /// <i>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#GetDownloadedLeaderboardEntry <br/>
        /// https://partner.steamgames.com/doc/api/ISteamFriends#RequestUserInformation
        /// </i>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the download was successful</param>
        private static async void GetDownloadedLeaderboardScoresAsync(LeaderboardScoresDownloaded_t _Callback, bool _Failure)
        {
            if (!SteamManager.Initialized)
            {
                return;
            }

            processingLeaderboardEntries = true;
            await Task.Run(() =>
            {
                steamUsers.Clear();
            
                // ReSharper disable once InconsistentNaming
                for (var i = 0; i < _Callback.m_cEntryCount; i++)
                {
                    var _successfullyDownloadedLeaderboardEntries = SteamUserStats.GetDownloadedLeaderboardEntry(_Callback.m_hSteamLeaderboardEntries, i, out var _leaderboardEntry, null, 0);
                    if (_successfullyDownloadedLeaderboardEntries)
                    {
                        var _steamUser = _leaderboardEntry.m_steamIDUser;
                        var _steamID = _steamUser.m_SteamID;
                    
                        if (_steamUser.IsValid())
                        {
                            if (_steamID == SteamManager.SteamID.m_SteamID)
                            {
                                currentLeaderboardScore = _leaderboardEntry.m_nScore;
                            }
                        
                            steamUsers.Add(new LeaderboardUserData
                            {
                                SteamId = _leaderboardEntry.m_steamIDUser.m_SteamID,
                                GlobalRank = _leaderboardEntry.m_nGlobalRank,
                                Score = _leaderboardEntry.m_nScore
                            });

                            userInformationRequested++;
                        
                            // This will only get the username for friends
                            // To get the username for unknown players, the "onPersonaStateChanged"-Callback needs to be awaited after calling "SteamFriends.RequestUserInformation()"
                            var _needsToRetrieveInformationFromInternet = SteamFriends.RequestUserInformation(_steamUser, true);
                            if (!_needsToRetrieveInformationFromInternet)
                            {
                                GetUserName(_steamID, false);
                            }
                        }
                    }
                }
            
                steamUsers = steamUsers.OrderBy(_SteamUser => _SteamUser.GlobalRank).ToList();
            });
            processingLeaderboardEntries = false;
            
            OnLeaderboardScoresDownloaded?.Invoke();
            instance.StartCoroutine(nameof(GetUserNames));
        }

        /// <summary>
        /// Gets the usernames for all steam ids in <see cref="steamUserNameRequests"/>
        /// </summary>
        private IEnumerator GetUserNames()
        {
            foreach (var _steamId in steamUserNameRequests)
            {
                GetUserName(_steamId, true);
            }

            yield return null;
        }
        
        /// <summary>
        /// <see cref="onPersonaStateChanged"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnPersonaStateChanged(PersonaStateChange_t _Callback)
        {
            GetUserName(_Callback.m_ulSteamID, true);
        }

        /// <summary>
        /// Gets the username for the given steam id <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#GetFriendPersonaName</i>
        /// </summary>
        /// <param name="_SteamId">The id of the user to get the username for</param>
        /// <param name="_RetrievedFromInternet">Indicates whether the data for the given steam id was already available, or had to be retrieved from the internet</param>
        private static void GetUserName(ulong _SteamId, bool _RetrievedFromInternet)
        {
            if (userInformationRequested <= 0)
            {
                return;
            }
            if (processingLeaderboardEntries && _RetrievedFromInternet)
            {
                steamUserNameRequests.Add(_SteamId);
                return;
            }
            
            userInformationRequested = (uint)Mathf.Clamp(--userInformationRequested, 0, uint.MaxValue);
            
            var _username = SteamFriends.GetFriendPersonaName(new CSteamID(_SteamId));
            
            var _index = steamUsers.FindIndex(_SteamUser => _SteamUser.SteamId == _SteamId);
            if (_index != -1)
            {
                steamUsers[_index] = new LeaderboardUserData(steamUsers[_index], _username);
                OnUsernameFound?.Invoke(_index);
            }
            else
            {
                Debug.LogError($"Could not find an entry for the given steam id, in {nameof(steamUsers)}");
            }
        }
        
        /// <summary>
        /// Uploads the given score to the <see cref="steamLeaderboard"/> <br/>
        /// <b>Max 10 request per 10 minutes</b> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#UploadLeaderboardScore</i>
        /// </summary>
        private static void UploadScore(uint _Score) // TODO: Make sure to not exceed the rate limit
        {
            if (!SteamManager.Initialized)
            {
                return;
            }
            if (GameController.IsGameRunning)
            {
                return;
            }
            
            if (steamLeaderboard is null)
            {
                Debug.LogError($"The leaderboard is not initialized [{nameof(UploadScore)}]");
            }
            else
            {
                if (_Score <= currentLeaderboardScore)
                {
                    return;
                }
                
                var _steamAPICall = SteamUserStat.UploadLeaderboardScore(steamLeaderboard.Value, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (int)_Score, null, 0);
                onLeaderboardScoreUploaded.Set(_steamAPICall, OnScoreUploaded);
            }
        }
        
        /// <summary>
        /// <see cref="onLeaderboardScoreUploaded"/> <br/>
        /// <i>
        /// Calls <see cref="DownloadLeaderboardScores"/> after a successful upload <br/>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#LeaderboardScoreUploaded_t
        /// </i>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the upload was successful</param>
        private static void OnScoreUploaded(LeaderboardScoreUploaded_t _Callback, bool _Failure)
        {
            if (!_Failure)
            {
                DownloadLeaderboardScores();
            }
            else
            {
                Debug.LogError("Error while uploading the score");
            }
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        [FilePath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Test", Extensions = ".txt")]
        [Tooltip("Filepath to the file that holds all names")]
        [LabelWidth(75)]
        [SerializeField]private string filepath;
        
        /// <summary>
        /// Amount of scores to download with <see cref="DownloadLeaderboardScores_DEVELOPMENT"/>
        /// </summary>
        private const uint SCORE_AMOUNT = 950;

        /// <summary>
        /// Returns a random entry from the given list and removes it from the list
        /// </summary>
        /// <param name="_Names">The list to get the name from</param>
        /// <returns>A random entry from the given list</returns>
        private static string GetRandomName(IList<string> _Names)
        {
            var _index = Random.Range(0, _Names.Count - 1);
            var _name = _Names[_index];
            
            _Names.RemoveAt(_index);

            return _name;
        }
        
        /// <summary>
        /// Fills <see cref="steamUsers"/> with random values <br/>
        /// <b>Only for testing</b>
        /// </summary>
        /// <param name="_Amount">Number of entries to add to <see cref="steamUsers"/></param>
        /// <param name="_AddNew">True = randomizes the steamID for each user, False = always adds the same steamID with every method call</param>
        private static void DownloadLeaderboardScores_DEVELOPMENT(ulong _Amount, bool _AddNew)
        {
            ulong _id = 1000000000000000;
            var _names = File.ReadAllLines(instance.filepath).ToList();

            steamUsers.Clear();
            
            // ReSharper disable once InconsistentNaming
            for (ulong i = 0; i < _Amount; i++)
            {
                var _steamID = new CSteamID(_AddNew ? (ulong)Random.Range(1000000000000000, 9999999999999999) : _id++);
                var _username = GetRandomName(_names);
                
                AddUser_DEVELOPMENT(_steamID, _username, true);
            }
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll); i++)
            {
                var _friend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll);
                var _friendUserName = SteamFriends.GetFriendPersonaName(_friend);
                
                AddUser_DEVELOPMENT(_friend, _friendUserName, false);
            }
            
            AddUser_DEVELOPMENT(SteamManager.SteamID, SteamFriends.GetPersonaName(), false);
            
            steamUsers = steamUsers.OrderByDescending(_SteamUser => _SteamUser.Score).ToList();
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < steamUsers.Count; i++)
            {
                steamUsers[i] = new LeaderboardUserData(steamUsers[i], i + 1);
            }
            
            OnLeaderboardScoresDownloaded?.Invoke();
        }

        /// <summary>
        /// Adds the given user to <see cref="steamUsers"/> <br/>
        /// <b>Only for testing!</b>
        /// </summary>
        /// <param name="_SteamId">The id of the user to add</param>
        /// <param name="_Username">The name of the user to add</param>
        /// <param name="_NeedsToRetrieveInformationFromInternet">
        /// True means the needs to be requested and the <see cref="onPersonaStateChanged"/> callback has to be awaited. <br/>
        /// False means all details about the user are already there.
        /// </param>
        private static void AddUser_DEVELOPMENT(CSteamID _SteamId, string _Username, bool _NeedsToRetrieveInformationFromInternet)
        {
            var _leaderboardEntry = new LeaderboardEntry_t
            {
                m_steamIDUser = _SteamId,
                m_nScore = Random.Range(0, 10000)
            };
                
            steamUsers.Add(new LeaderboardUserData
            {
                SteamId = _leaderboardEntry.m_steamIDUser.m_SteamID,
                GlobalRank = _leaderboardEntry.m_nGlobalRank,
                Score = _leaderboardEntry.m_nScore
            });

            instance.StartCoroutine(GetUserName_DEVELOPMENT(_SteamId, _Username, _NeedsToRetrieveInformationFromInternet));
        }

        /// <summary>
        /// Sets the given username for the given id in <see cref="steamUsers"/>
        /// </summary>
        /// <param name="_SteamId">The id of the user to set the username of</param>
        /// <param name="_Username">The username to set</param>
        /// <param name="_NeedsToRetrieveInformationFromInternet">
        /// True means the needs to be requested and the <see cref="onPersonaStateChanged"/> callback has to be awaited. <br/>
        /// False means all details about the user are already there.
        /// </param>
        /// <returns></returns>
        private static IEnumerator GetUserName_DEVELOPMENT(CSteamID _SteamId, string _Username, bool _NeedsToRetrieveInformationFromInternet)
        {
            if (_NeedsToRetrieveInformationFromInternet)
            {
                yield return new WaitForSeconds(Random.Range(2, 5));
            }
            
            var _index = steamUsers.FindIndex(_SteamUser => _SteamUser.SteamId == _SteamId.m_SteamID);
            if (_index != -1)
            {
                steamUsers[_index] = new LeaderboardUserData(steamUsers[_index], _Username);
                OnUsernameFound?.Invoke(_index);
            }
        }
#endif
        #endregion
    }
}