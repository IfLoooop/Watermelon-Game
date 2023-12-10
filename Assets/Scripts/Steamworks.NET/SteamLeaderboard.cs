using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OPS.AntiCheat.Field;
using Steamworks;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Menus.Leaderboards;
using Watermelon_Game.Points;
using Watermelon_Game.Utility;
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
        /// <summary>
        /// Maximum number of leaderboard entries to download 
        /// </summary>
        private const int MAX_LEADERBOARD_ENTRIES = 999;
        /// <summary>
        /// Minimum amount of entries for the leaderboard
        /// </summary>
        private const int MIN_LEADERBOARD_ENTRIES = 100;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="SteamLeaderboard"/>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static SteamLeaderboard instance;
        /// <summary>
        /// Used to cancel <see cref="GetDownloadedLeaderboardScoresAsync"/> <see cref="OnApplicationQuit"/>
        /// </summary>
        private static readonly CancellationTokenSource cancellationTokenSource = new();
        /// <summary>
        /// The leaderboard that was found during <see cref="Init"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#SteamLeaderboard_t</i>
        /// </summary>
        private static SteamLeaderboard_t? steamLeaderboard;
        /// <summary>
        /// Is true when global rank 1 - <see cref="MIN_LEADERBOARD_ENTRIES"/> haven't been downloaded on the initial download
        /// </summary>
        private static bool downloadAdditionalEntries;
        /// <summary>
        /// Indicates how many user information request have been made, e.g. how often the <see cref="GetUserName"/>-Method should be allowed to run. <br/>
        /// <i>
        /// Sometimes the <see cref="onPersonaStateChange"/>-Callback is received without manually requesting it. <br/>
        /// A value less than 1 prevents the <see cref="GetUserName"/>-Method to run
        /// </i>
        /// </summary>
        private static int userInformationRequested;
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
        /// <see cref="SteamUsers"/>
        /// </summary>
        public static List<LeaderboardUserData> SteamUsers { get; private set; } = new();
        /// <summary>
        /// Contains all friends of the local user, including the local user 
        /// </summary>
        public static ConcurrentBag<LeaderboardUserData> Friends { get; private set; } = new();
        /// <summary>
        /// Indicates whether <see cref="GetDownloadedLeaderboardScoresAsync"/> is currently running or not
        /// </summary>
        public static bool ProcessingLeaderboardEntries { get; private set; }
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
        private static Callback<PersonaStateChange_t> onPersonaStateChange;
        #endregion

        #region Events
        /// <summary>
        /// Is called when all scores of the <see cref="steamLeaderboard"/> have been downloaded -> <see cref="GetDownloadedLeaderboardScoresAsync"/>
        /// </summary>
        public static event Action OnLeaderboardScoresDownloaded;
        /// <summary>
        /// Is called everytime <see cref="GetUserName"/> finds a user <br/>
        /// <b>Parameter:</b> The index in <see cref="SteamUsers"/>, for whom the username was found
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
        
        private void OnApplicationQuit()
        {
            cancellationTokenSource.Cancel();
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
            onPersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
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
        /// <param name="_LeaderboardDataRequest">The type of data to download</param>
        /// <param name="_RangeStart">At what position to start to download (dependant on <see cref="_LeaderboardDataRequest"/>)</param>
        /// <param name="_RangeEnd">Until what position to download (dependant on <see cref="_LeaderboardDataRequest"/>)</param>
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public static void DownloadLeaderboardScores(ELeaderboardDataRequest _LeaderboardDataRequest = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, int _RangeStart = -(MAX_LEADERBOARD_ENTRIES / 2), int _RangeEnd = MAX_LEADERBOARD_ENTRIES / 2)
        {
#if UNITY_EDITOR
            DownloadLeaderboardScores_DEVELOPMENT(LEADERBOARD_ENTRY_COUNT, false);
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
                // TODO: If the user has not entry in the leaderboard, "ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser" will have 0 entries.
                // TODO: In that case, check if the user has any entries, if not, download the first "MAX_LEADERBOARD_ENTRIES"
                // TODO: Also wait for "OnLeaderboardFound()" (success or failure) before going any further
                
                var _steamAPICall = SteamUserStats.DownloadLeaderboardEntries(steamLeaderboard.Value, _LeaderboardDataRequest, _RangeStart, _RangeEnd);
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

            if (_Callback.m_cEntryCount <= 0)
            {
                Debug.LogError($"[{nameof(SteamLeaderboard)}].{nameof(GetDownloadedLeaderboardScoresAsync)} {nameof(_Callback.m_cEntryCount)}:{_Callback.m_cEntryCount}");
                return;
            }
            
            ProcessingLeaderboardEntries = true;
            await Task.Run(() =>
            {
                ConcurrentBag<ulong> _friendIds;
                if (!downloadAdditionalEntries)
                {
                    Friends.Clear();
                    _friendIds = new ConcurrentBag<ulong> { SteamManager.SteamID.m_SteamID };
                    // ReSharper disable once InconsistentNaming
                    for (var i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                    
                        _friendIds.Add(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate).m_SteamID);   
                    }
                }
                else
                {
                    _friendIds = new ConcurrentBag<ulong>(Friends.Select(_Fiend => _Fiend.SteamId));
                }
                
                var _concurrentBag = new ConcurrentBag<LeaderboardUserData>();
                // ReSharper disable once InconsistentNaming
                Parallel.For(0, _Callback.m_cEntryCount, i =>
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

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
                            
                            Interlocked.Increment(ref userInformationRequested);

                            var _username = string.Empty;
                            // This will only get the username for friends
                            // To get the username for unknown players, the "onPersonaStateChange"-Callback needs to be awaited after calling "SteamFriends.RequestUserInformation()"
                            var _needsToRetrieveInformationFromInternet = SteamFriends.RequestUserInformation(_steamUser, true);
                            if (!_needsToRetrieveInformationFromInternet)
                            {
                                GetUserName(_steamID, false, out _username);
                            }

                            var _leaderboardUserData = new LeaderboardUserData
                            {
                                SteamId = _leaderboardEntry.m_steamIDUser.m_SteamID,
                                GlobalRank = _leaderboardEntry.m_nGlobalRank,
                                Score = _leaderboardEntry.m_nScore,
                                Username = _username
                            };
                            
                            _concurrentBag.Add(_leaderboardUserData);

                            if (_friendIds.Contains(_leaderboardUserData.SteamId))
                            {
                                Friends.Add(_leaderboardUserData);
                            }
                        }
                    }
                });

                Friends = new ConcurrentBag<LeaderboardUserData>(Friends.OrderBy(_SteamUser => _SteamUser.GlobalRank));
                
                if (!downloadAdditionalEntries)
                {
                    SteamUsers = _concurrentBag.OrderBy(_SteamUser => _SteamUser.GlobalRank).ToList();
                    
                    if (SteamUsers[0].GlobalRank > 1)
                    {
                        downloadAdditionalEntries = true;
                        var _rangeEnd = SteamUsers[0].GlobalRank <= MIN_LEADERBOARD_ENTRIES ? SteamUsers[0].GlobalRank - 1 : MIN_LEADERBOARD_ENTRIES;
                        DownloadLeaderboardScores(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, _rangeEnd);
                    }
                }
                else
                {
                    SteamUsers.AddRange(_concurrentBag);
                    SteamUsers = SteamUsers.OrderBy(_SteamUser => _SteamUser.GlobalRank).ToList();
                    downloadAdditionalEntries = false;
                }
            });
            if (!downloadAdditionalEntries)
            {
                ProcessingLeaderboardEntries = false;
            
                OnLeaderboardScoresDownloaded?.Invoke();
                instance.StartCoroutine(nameof(GetUserNames));   
            }
            
#if DEBUG || DEVELOPMENT_BUILD
            //CheckForMissingCharactersAsync_DEVELOPMENT();
#endif
        }

        /// <summary>
        /// Gets the usernames for all steam ids in <see cref="steamUserNameRequests"/>
        /// </summary>
        private IEnumerator GetUserNames()
        {
            var _waitTime = new WaitForEndOfFrame();
            foreach (var _steamId in steamUserNameRequests)
            {
                GetUserName(_steamId, true, out _);
                yield return _waitTime;
            }
        }
        
        /// <summary>
        /// <see cref="onPersonaStateChange"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnPersonaStateChange(PersonaStateChange_t _Callback)
        {
            GetUserName(_Callback.m_ulSteamID, true, out _);
        }

        /// <summary>
        /// Gets the username for the given steam id <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#GetFriendPersonaName</i>
        /// </summary>
        /// <param name="_SteamId">The id of the user to get the username for</param>
        /// <param name="_RetrievedFromInternet">Indicates whether the data for the given steam id was already available, or had to be retrieved from the internet</param>
        /// <param name="_Username">Will contain the username, if <see cref="_RetrievedFromInternet"/> was false</param>
        private static void GetUserName(ulong _SteamId, bool _RetrievedFromInternet, out string _Username)
        {
            _Username = string.Empty;
            if (userInformationRequested <= 0)
            {
                return;
            }
            if (ProcessingLeaderboardEntries && _RetrievedFromInternet)
            {
                steamUserNameRequests.Add(_SteamId);
                return;
            }
            
            userInformationRequested = Mathf.Clamp(--userInformationRequested, 0, int.MaxValue);

            var _steamId = new CSteamID(_SteamId);
            if (_steamId.IsValid())
            {
                _Username = SteamFriends.GetFriendPersonaName(_steamId);
                if (_RetrievedFromInternet)
                {
                    var _index = SteamUsers.FindIndexParallel(_SteamUser => _SteamUser.SteamId == _SteamId);
                    if (_index != -1)
                    {
                        SteamUsers[_index] = new LeaderboardUserData(SteamUsers[_index], _Username);
                        OnUsernameFound?.Invoke(_index);
                    }
                    else
                    {
                        Debug.LogError($"Could not find an entry for the given steam id, in {nameof(SteamUsers)} | {_steamId} | {_Username}");
                    }       
                }
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
            if (GameController.ActivGame)
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

#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(SteamLeaderboard)}.{nameof(UploadScore)} should not be called in editor");
                return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
                // ReSharper disable once HeuristicUnreachableCode
                var _steamAPICall = SteamUserStat.UploadLeaderboardScore(steamLeaderboard.Value, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (int)_Score, null, 0);
                onLeaderboardScoreUploaded.Set(_steamAPICall, OnScoreUploaded);
#pragma warning restore CS0162 // Unreachable code detected
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
        /// <summary>
        /// Amount of entries to download with <see cref="DownloadLeaderboardScores_DEVELOPMENT"/>
        /// </summary>
        private const uint LEADERBOARD_ENTRY_COUNT = 1100;
        
        /// <summary>
        /// Fills <see cref="SteamUsers"/> with random values <br/>
        /// <b>Only for testing</b>
        /// </summary>
        /// <param name="_Amount">Number of entries to add to <see cref="SteamUsers"/></param>
        /// <param name="_AddNew">True = randomizes the steamID for each user, False = always adds the same steamID with every method call</param>
        private static void DownloadLeaderboardScores_DEVELOPMENT(ulong _Amount, bool _AddNew)
        {
            ulong _id = 1000000000000000;
            
            SteamUsers.Clear();
            
            // ReSharper disable once InconsistentNaming
            for (ulong i = 0; i < _Amount; i++)
            {
                var _steamID = new CSteamID(_AddNew ? (ulong)Random.Range(1000000000000000, 9999999999999999) : _id++);
                var _username = _id.ToString();
                
                AddUser_DEVELOPMENT(_steamID, _username, true);
            }

            var _friends = new List<ulong> { SteamManager.SteamID.m_SteamID };
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++)
            {
                var _friend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                var _friendUserName = SteamFriends.GetFriendPersonaName(_friend);
                
                _friends.Add(_friend.m_SteamID);
                
                AddUser_DEVELOPMENT(_friend, _friendUserName, false);
            }
            
            AddUser_DEVELOPMENT(SteamManager.SteamID, SteamFriends.GetPersonaName(), false);
            
            SteamUsers = SteamUsers.OrderByDescending(_SteamUser => _SteamUser.Score).ToList();
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < SteamUsers.Count; i++)
            {
                SteamUsers[i] = new LeaderboardUserData(SteamUsers[i], i + 1);

                if (_friends.Contains(SteamUsers[i].SteamId))
                {
                    Friends.Add(SteamUsers[i]);
                }
            }
            Friends = new ConcurrentBag<LeaderboardUserData>(Friends.OrderByDescending(_Friend => _Friend.GlobalRank));
            
            OnLeaderboardScoresDownloaded?.Invoke();
        }

        /// <summary>
        /// Adds the given user to <see cref="SteamUsers"/> <br/>
        /// <b>Only for testing!</b>
        /// </summary>
        /// <param name="_SteamId">The id of the user to add</param>
        /// <param name="_Username">The name of the user to add</param>
        /// <param name="_NeedsToRetrieveInformationFromInternet">
        /// True means the needs to be requested and the <see cref="onPersonaStateChange"/> callback has to be awaited. <br/>
        /// False means all details about the user are already there.
        /// </param>
        private static void AddUser_DEVELOPMENT(CSteamID _SteamId, string _Username, bool _NeedsToRetrieveInformationFromInternet)
        {
            var _leaderboardEntry = new LeaderboardEntry_t
            {
                m_steamIDUser = _SteamId,
                m_nScore = Random.Range(0, 10000)
            };

            SteamUsers.Add(new LeaderboardUserData
            {
                SteamId = _leaderboardEntry.m_steamIDUser.m_SteamID,
                Username = _Username,
                GlobalRank = _leaderboardEntry.m_nGlobalRank,
                Score = _leaderboardEntry.m_nScore
            }); 

            instance.StartCoroutine(GetUserName_DEVELOPMENT(_SteamId, _Username, _NeedsToRetrieveInformationFromInternet));
        }

        /// <summary>
        /// Sets the given username for the given id in <see cref="SteamUsers"/>
        /// </summary>
        /// <param name="_SteamId">The id of the user to set the username of</param>
        /// <param name="_Username">The username to set</param>
        /// <param name="_NeedsToRetrieveInformationFromInternet">
        /// True means the needs to be requested and the <see cref="onPersonaStateChange"/> callback has to be awaited. <br/>
        /// False means all details about the user are already there.
        /// </param>
        /// <returns></returns>
        private static IEnumerator GetUserName_DEVELOPMENT(CSteamID _SteamId, string _Username, bool _NeedsToRetrieveInformationFromInternet)
        {
            if (_NeedsToRetrieveInformationFromInternet)
            {
                yield return new WaitForSeconds(Random.Range(2, 5));
            }
            
            var _index = SteamUsers.FindIndex(_SteamUser => _SteamUser.SteamId == _SteamId.m_SteamID);
            if (_index != -1)
            {
                SteamUsers[_index] = new LeaderboardUserData(SteamUsers[_index], _Username);
                OnUsernameFound?.Invoke(_index);
            }
        }
        
        /// <summary>
        /// Checks if any username in <see cref="SteamLeaderboard"/>.<see cref="SteamLeaderboard.SteamUsers"/> contains a character that is not supported <br/>
        /// <b>Development only!</b>
        /// </summary>
        private static async void CheckForMissingCharactersAsync_DEVELOPMENT() // TODO: Maybe not needed
        {
            var _usernames = new ConcurrentBag<string>();

            await Task.Run(() =>
            {
                Parallel.ForEach(SteamUsers, _SteamUser =>
                {
                    _usernames.Add(_SteamUser.Username);
                });
            });
            
            await FontManager.AddCharactersToFontAssetAsync_DEVELOPMENT(_usernames);
            
            var _missingCharactersFound = false;
            
            await Task.Run(() =>
            {
                Parallel.ForEach(SteamUsers, _SteamUser =>
                {
                    var _info = $"[{_SteamUser.GlobalRank}] {_SteamUser.Username} [{_SteamUser.Score}]";
                    
                    if (FontManager.CheckForMissingCharacters_DEVELOPMENT(_SteamUser.Username, _info))
                    {
                        _missingCharactersFound = true;
                    }
                });
            });
            
            if (_missingCharactersFound)
            {
                FontManager.WriteToFile_DEVELOPMENT();
            }
        }
#endif
        #endregion
    }
}