using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Menus.Leaderboards;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Steamworks.NET
{
    /// <summary>
    /// Steam leaderboards <br/>
    /// https://partner.steamgames.com/doc/features/leaderboards/guide
    /// </summary>
    internal sealed class SteamLeaderboard : MonoBehaviour
    {
        #region Constants
        /// <summary>
        /// The name of the leaderboard at: <br/>
        /// https://partner.steamgames.com/apps/leaderboards/2658820
        /// </summary>
        private const string LEADERBOARD_NAME = "Test"; // TODO: Change to correct name
        #endregion

        #region Fields
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
        /// Indicates how many user information request have been made, e.g. how often the <see cref="GetUserName"/>-Method should be allowed to run. <br/>
        /// <i>
        /// Sometimes the <see cref="onPersonaStateChanged"/>-Callback is received without manually requesting it. <br/>
        /// A value less than 1 prevents the <see cref="GetUserName"/>-Method to run
        /// </i>
        /// </summary>
        private static uint userInformationRequested;
        /// <summary>
        /// The current score in <see cref="steamLeaderboard"/> of <see cref="SteamManager.SteamID"/>
        /// </summary>
        private static int currentLeaderboardScore;
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
        /// Is called when all scores of the <see cref="steamLeaderboard"/> have been downloaded -> <see cref="GetDownloadedLeaderboardScores"/>
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
#if UNITY_EDITOR
            instance_DEVELOPMENT = this;
#endif
            Init();
        }
        
#pragma warning disable CS0162 // Unreachable code detected
        /// <summary>
        /// Initializes the steam leaderboard <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#FindLeaderboard</i>
        /// </summary>
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        private static void Init()
        {
#if UNITY_EDITOR
            DownloadLeaderboardScores_DEVELOPMENT(1000, false);
            return;
#endif
            
            if (!SteamManager.Initialized)
            {
                return;
            }
            
            var _steamAPICall = SteamUserStats.FindLeaderboard(LEADERBOARD_NAME);
            onLeaderboardFound.Set(_steamAPICall, OnLeaderboardFound);
            onPersonaStateChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
        }
#pragma warning restore CS0162 // Unreachable code detected

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

        /// <summary>
        /// Downloads all scores from <see cref="steamLeaderboard"/> <br/>
        /// <i>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#DownloadLeaderboardEntries <br/>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#ELeaderboardDataRequest
        /// </i>
        /// </summary>
        private static void DownloadLeaderboardScores()
        {
            if (steamLeaderboard is null)
            {
                Debug.LogError($"The leaderboard is not initialized [{nameof(DownloadLeaderboardScores)}]");
            }
            else
            {
                var _steamAPICall = SteamUserStats.DownloadLeaderboardEntries(steamLeaderboard.Value, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, int.MaxValue);
                onLeaderboardScoresDownloaded.Set(_steamAPICall, GetDownloadedLeaderboardScores);
            }
        }

        /// <summary>
        /// <see cref="onLeaderboardScoresDownloaded"/> <br/>
        /// <i>
        /// https://partner.steamgames.com/doc/api/ISteamUserStats#GetDownloadedLeaderboardEntry <br/>
        /// https://partner.steamgames.com/doc/api/ISteamFriends#RequestUserInformation
        /// </i>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the download was successful</param>
        private static void GetDownloadedLeaderboardScores(LeaderboardScoresDownloaded_t _Callback, bool _Failure)
        {
            if (_Failure)
            {
                // TODO: 
            }
            
            steamUsers.Clear();
            var _steamLeaderboardEntries = _Callback.m_hSteamLeaderboardEntries;
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < (int)_steamLeaderboardEntries.m_SteamLeaderboardEntries; i++)
            {
                var _successfullyDownloadedLeaderboardEntries = SteamUserStats.GetDownloadedLeaderboardEntry(_steamLeaderboardEntries, i, out var _leaderboardEntry, null, 0);
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
                        
                        AddUser(_leaderboardEntry);

                        userInformationRequested++;
                        
                        // This will only get the username for friends
                        // To get the username for unknown players, the "onPersonaStateChanged"-Callback needs to be awaited after calling "SteamFriends.RequestUserInformation()"
                        var _needsToRetrieveInformationFromInternet = SteamFriends.RequestUserInformation(_steamUser, true);
                        if (!_needsToRetrieveInformationFromInternet)
                        {
                            GetUserName(_steamID);
                        }
                    }
                }
            }

            steamUsers = steamUsers.OrderBy(_SteamUser => _SteamUser.GlobalRank).ToList();
            OnLeaderboardScoresDownloaded?.Invoke();
        }
        
        /// <summary>
        /// Adds the user of the given <see cref="LeaderboardEntry_t"/> to <see cref="steamUsers"/> or overwrites the data, if the user's already added 
        /// </summary>
        /// <param name="_LeaderboardEntry">The <see cref="LeaderboardEntry_t"/> to get the data from</param>
        private static void AddUser(LeaderboardEntry_t _LeaderboardEntry)
        {
            var _index = steamUsers.FindIndex(_SteamUser => _SteamUser.SteamId == _LeaderboardEntry.m_steamIDUser.m_SteamID);
            if (_index != -1)
            {
                steamUsers[_index] = new LeaderboardUserData
                {
                    GlobalRank = _LeaderboardEntry.m_nGlobalRank,
                    Username = steamUsers[_index].Username,
                    Score = _LeaderboardEntry.m_nScore
                };
            }
            else
            {
                steamUsers.Add(new LeaderboardUserData
                {
                    SteamId = _LeaderboardEntry.m_steamIDUser.m_SteamID,
                    GlobalRank = _LeaderboardEntry.m_nGlobalRank,
                    Score = _LeaderboardEntry.m_nScore
                });
            }
        }
        
        /// <summary>
        /// <see cref="onPersonaStateChanged"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnPersonaStateChanged(PersonaStateChange_t _Callback)
        {
            GetUserName(_Callback.m_ulSteamID);
        }

        /// <summary>
        /// Gets the username for the given steam id <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#GetFriendPersonaName</i>
        /// </summary>
        /// <param name="_SteamId">The id of the user to get the username for</param>
        private static void GetUserName(ulong _SteamId)
        {
            if (userInformationRequested <= 0)
            {
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
                Debug.LogError($"Could not find an entry for ID: {_SteamId}, User: {_username} in {nameof(steamUsers)}");
            }
        }
        
        /// <summary>
        /// Uploads the given score to the <see cref="steamLeaderboard"/> <br/>
        /// <b>Max 10 request per 10 minutes</b> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#UploadLeaderboardScore</i>
        /// </summary>
        public static void UploadScore(int _Score) // TODO: Make sure to not exceed the rate limit
        {
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
                
                // TODO:
                // Keep track on how often this was called (for rate limit)
                // If the rate limit is reached, save the score to a .txt for (in case of game close) and try again after some time (also at game start, if the .txt file has an entry)
                var _steamAPICall = SteamUserStats.UploadLeaderboardScore(steamLeaderboard.Value, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, _Score, null, 0);
                onLeaderboardScoreUploaded.Set(_steamAPICall, OnScoreUploaded);
            }
        }
        
        /// <summary>
        /// <see cref="onLeaderboardScoreUploaded"/> <br/>
        /// <i>Calls <see cref="DownloadLeaderboardScores"/> after a successful upload</i>
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
        
        private static int score = 75;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                //DownloadLeaderboardScores_DEVELOPMENT(1200, false);
                UploadScore(score);

                score += 5;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Singleton of <see cref="SteamLeaderboard"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static SteamLeaderboard instance_DEVELOPMENT;



        /// <summary>
        /// Fills <see cref="steamUsers"/> with random values <br/>
        /// <b>Only for testing</b>
        /// </summary>
        /// <param name="_Amount">Number of entries to add to <see cref="steamUsers"/></param>
        /// <param name="_AddNew">True = randomizes the steamID for each user, False = always adds the same steamID with every method call</param>
        private static void DownloadLeaderboardScores_DEVELOPMENT(ulong _Amount, bool _AddNew)
        {
            ulong _id = 1000000000000000;
            
            var _names = new[]
            {
                "Hans", "Detlef", "Peter", "Jan", "Utz", "Adolf", "Klaus", "Günter"
            };

            // ReSharper disable once InconsistentNaming
            for (ulong i = 0; i < _Amount; i++)
            {
                var _steamID = new CSteamID(_AddNew ? (ulong)Random.Range(1000000000000000, 9999999999999999) : _id++);
                var _username = $"{_names[Random.Range(0, _names.Length - 1)]}{i + 1}";
                
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
                
            AddUser(_leaderboardEntry);

            instance_DEVELOPMENT.StartCoroutine(GetUserName_DEVELOPMENT(_SteamId, _Username, _NeedsToRetrieveInformationFromInternet));
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