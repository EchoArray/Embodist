using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using UnityEngine.Networking;

[ValueReference("game_manager")]
public class GameManager : MonoBehaviour
{
    #region Values
    public static GameManager Instance;
    /// <summary>
    /// Determines if a game has been played since game load
    /// </summary>
    public static bool Dirty;
    /// <summary>
    /// Determines if local players are allowed to join the game.
    /// </summary>
    public bool AllowLocalPlayerJoining = true;
    /// <summary>
    /// Defines the maximum quantity of players in a session.
    /// </summary>
    public int MaxProfileCount = 8;
    [Serializable]
    public class GameAspects
    {
        /// <summary>
        /// Defines the selected map of the game.
        /// </summary>
        [ValueReference("selected_map_name")]
        public string SelectedMapName = "house";
        /// <summary>
        /// Determines if the game is in play
        /// </summary>
        public bool Playing;
        /// <summary>
        /// Determies if local players are allowed to input.
        /// </summary>
        public bool AllowInput;
        [Serializable]
        public class GameSettings
        {
            public enum GameMode
            {
                Goop,
                Capture,
                KingOfTheHill,
                Bomb
            }

            /// <summary>
            /// Determines which game mode is active.
            /// </summary>
            [ValueReference("mode")]
            public GameMode Mode;

            public enum Score
            {
                Unlimited,
                One,
                Two,
                Three,
                Five,
                Ten,
                Fifteen,
                Twenty,
                TwentyFive,
                Fifty,
                OneHundred,
                TwoHundred,
                ThreeHundred,
                FiveHundred,
                OneThousand,
                TwoThousand,
                ThreeThousand,
                FiveThousand,

            }
            /// <summary>
            /// Defines the score in-which it takes to win the game.
            /// </summary>
            [ValueReference("score_to_win")]
            public Score ScoreToWin;

            /// <summary>
            /// Determines if the game is team based or not
            /// </summary>
            [ValueReference("team_game")]
            public bool TeamGame;

            /// <summary>
            /// Determines if rooms will be sealed off when selecting.
            /// </summary>
            [ValueReference("isolate_areas")]
            public bool IsolateAreas;
            /// <summary>
            /// Determines if inanaimate objects are allowed to jump off of walls.
            /// </summary>
            [ValueReference("climb_walls")]
            public bool ClimbWalls = false;
            /// <summary>
            /// Determines the active state of the hot lava objects container.
            /// </summary>
            [ValueReference("hot_lava")]
            public bool HotLava = false;
            /// <summary>
            /// Determines if all inanaimate objects in the game are to be spawned as bottles of ketchup.
            /// </summary>
            [ValueReference("ketchup_only")]
            public bool KetchupOnly = false;

            /// <summary>
            /// Determines if the throwy class is allowed to be selected.
            /// </summary>
            [ValueReference("throwy_allowed")]
            public bool ThrowyAllowed = true;
            /// <summary>
            /// Determines if the squirty class is allowed to be selected.
            /// </summary>
            [ValueReference("squirty_allowed")]
            public bool SquirtyAllowed = true;
            /// <summary>
            /// Determines if the floppy class is allowed to be selected.
            /// </summary>
            [ValueReference("floppy_allowed")]
            public bool FloppyAllowed = true;

            public float LastDamageCasterDuration = 10f;
            /// <summary>
            /// Defines the time span of the game.
            /// </summary>
            public enum Duration
            {
                Unlimited,
                OneMinute,
                TwoMinutes,
                ThreeMinutes,
                FiveMinutes,
                TenMinutes,
                FifteenMinutes,
                ThirtyMinutes,
                OneHour,
            }
            [ValueReference("time_limit")]
            public Duration TimeLimit;

            [HideInInspector]
            [ValueReference("time_limit_remaining")]
            public float TimeLimitRemaining;

            /// <summary>
            /// Determines if the game is kill based.
            /// </summary>
            public bool KillBased
            {
                get
                {
                    return Mode == GameMode.Goop;
                }
            }

            /// <summary>
            /// Defines the duration in-which when ended a player will be forced to attach to an inaninamte object.
            /// </summary>
            public float AutoAttachDuration = 15f;

            /// <summary>
            /// Defines the time in which it takes to re-spawn.
            /// </summary>
            public float RespawnDuration;
            /// <summary>
            /// Defines the duration in which it takes to capture a territory.
            /// </summary>
            public float TerritoryCaptureDuration;
        }
        [ValueReference("game_type")]
        public GameSettings GameType;

        [Serializable]
        public class Profile
        {
            /// <summary>
            /// Defines the local player for this profile.
            /// </summary>
            [ValueReference("player")]
            public LocalPlayer LocalPlayer;
            /// <summary>
            /// Determines if the profile is local.
            /// </summary>
            public bool Local;

            /// <summary>
            /// Defines the profiles name.
            /// </summary>
            [Space(15)]
            public string Name;
            /// <summary>
            /// Defines the unique id associated with the profiles gamertag.
            /// </summary>
            public int GamerId;
            /// <summary>
            /// Defines the team that the profile is part of.
            /// </summary>
            public int TeamId;
            /// <summary>
            /// Determines the local controller id of the profile.
            /// </summary>
            public int ControllerId;
            /// <summary>
            /// Defines the connection id associated with the connection.
            /// </summary>
            public int ConnectionId;

            public enum Sensitivity
            {
                One,
                Two,
                Three,
                Four,
                Five,
                Six,
                Seven,
                Eight,
                Nine,
                Ten
            }
            /// <summary>
            /// Defines the profiles look sensitivity.
            /// </summary>
            [Space(15)]
            [ValueReference("look_sensitivity")]
            public Sensitivity LookSensitivity;

            /// <summary>
            /// Determines if the profiles look is inverted.
            /// </summary>
            [ValueReference("look_inverted")]
            public bool LookInverted;
            /// <summary>
            /// Determines if the profiles controller is allowed to vibrate.
            /// </summary>
            [ValueReference("controller_vibration")]
            public bool ControllerVibration;
            /// <summary>
            /// Defines the player color index used for coloration of the profile
            /// </summary>
            public int PlayerColorIndex;

            /// <summary>
            /// Defines the active kill count for the profile
            /// </summary>
            [Space(15)]
            public int Kills;
            /// <summary>
            /// Defines the active death count for the profile
            /// </summary>
            public int Deaths;
            /// <summary>
            /// Defines the active score for the profile
            /// </summary>
            public int Score;

            /// <summary>
            /// Defines the kill death amount of the profiles kills.
            /// </summary>
            [ValueReference("kill_death")]
            public int KillDeath
            {
                get { return Kills - Deaths; }
            }

            [ValueReference("active_score")]
            public int ActiveScore
            {
                get
                {

                    if (Instance.Game.GameType.TeamGame)
                        return Instance.Game.Teams[TeamId].Score;
                    else
                        return Score;
                }
            }

            [ValueReference("opponent_score")]
            public int OpponentScore
            {
                get
                {
                    int highestPlayerScore = 0;

                    if (Instance.Game.GameType.TeamGame)
                    {
                        foreach (GameAspects.Team team in GameManager.Instance.Game.Teams)
                            if (team.Score > highestPlayerScore && team != GameManager.Instance.Game.Teams[TeamId])
                                highestPlayerScore = team.Score;
                    }
                    else
                    {
                        foreach (GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
                            if (profile.Score > highestPlayerScore && profile != this)
                                highestPlayerScore = profile.Score;
                    }


                    return highestPlayerScore;
                }

            }

            /// <summary>
            /// Defines the menu UI element associated with the profile
            /// </summary>
            [Space(15)]
            public GameObject MenuUIElement;

            public Profile(string name, bool local, int gamerID, int controllerID, int teamID, int connectionId, int lookSensitivity, bool lookInverted, bool controllerVibration)
            {
                Name = name;
                Local = local;
                GamerId = gamerID;
                ControllerId = controllerID;
                TeamId = teamID;
                ConnectionId = connectionId;

                LookSensitivity = Sensitivity.Three;
                LookInverted = lookInverted;
                ControllerVibration = controllerVibration;
            }
        }
        [Space(15)]
        public List<Profile> Profiles;

        [Serializable]
        public class Team
        {
            /// <summary>
            /// Defines the color of the team.
            /// </summary>
            public Color Color;
            /// <summary>
            /// Defines the name of the team
            /// </summary>
            public string Name;
            /// <summary>
            /// Defines the active score for the team
            /// </summary>
            public int Score;
            /// <summary>
            /// Defines the active kills for the team
            /// </summary>
            public int Kills;
            /// <summary>
            /// Defines the active kills for the team
            /// </summary>
            public int Deaths;
            /// <summary>
            /// Defines the kill to death amout of the team.
            /// </summary>
            public int KillDeath
            {
                get { return Kills - Deaths; }
            }

        }
        public List<Team> Teams;
    }
    [ValueReference("game")]
    public GameAspects Game;
    /// <summary>
    /// Defines the color of players if they are not on a team.
    /// </summary>
    public Color PlayerColor;

    // A collection of various names used for new profiles
    public static string[] _genericPlayerNames = { "Donut", "Penguin", "Stumpy", "Whicker", "Shadow", "Howard", "Wilshire", "Darling", "Disco", "Jack", "The Bear", "Sneak", "The Big L", "Whisp", "Wheezy", "Crazy", "Goat", "Pirate", "Saucy", "Hambone", "Butcher", "Walla Walla", "Snake", "Caboose", "Sleepy", "Killer", "Stompy", "Mopey", "Dopey", "Weasel", "Ghost", "Dasher", "Grumpy", "Hollywood", "Tooth", "Noodle", "King", "Cupid", "Prancer" };

    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "title")
            return;

        Dirty = true;
        ResetStatistics();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Application.runInBackground = true;
        SceneManager.sceneLoaded += OnSceneLoaded;


        // Define instance
        Instance = this;

        // Prevent this from being destroyed upon level load
        DontDestroyOnLoad(transform.gameObject);
    }

    [MethodReference("start_game")]
    public void StartGame()
    {
        SceneManager.LoadScene(Game.SelectedMapName);
        if (NetworkSessionManager.IsHost)
            NetworkSessionManager.singleton.ServerChangeScene(Game.SelectedMapName);
    }
    [MethodReference("end_game")]
    public void EndGame()
    {
        if (!GameManager.Instance.Game.Playing)
            return;

        Game.Playing = false;
        HUDManager.Instance.ShowGameOver();

        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcEndGameForClients();

        StartCoroutine(WaitToReturnToMenu());
    }
    [MethodReference("leave_game")]
    public void LeaveGame()
    {
        if (!GameManager.Instance.Game.Playing)
            return;

        Game.Playing = false;

        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcEndGameForClients();

        StartCoroutine(WaitToReturnToMenu());
    }

    private IEnumerator WaitToReturnToMenu()
    {
        yield return new WaitForSeconds(3);
        NetworkSessionManager.networkSceneName = string.Empty;
        SceneManager.LoadScene("title");
        XboxInputManager.ClearAllVibrations();
    }


    private void ResetStatistics()
    {
        // Reset the values for all teams and players
        foreach (GameAspects.Team team in Game.Teams)
        {
            team.Score = 0;
            team.Kills = 0;
        }
        foreach (GameAspects.Profile profile in Game.Profiles)
        {
            profile.Score = 0;
            profile.Kills = 0;
            profile.Deaths = 0;
        }
    }

    public int GetScoreToWin()
    {
        switch (Game.GameType.ScoreToWin)
        {
            case GameAspects.GameSettings.Score.Unlimited:
                return 0;
            case GameAspects.GameSettings.Score.One:
                return 1;
            case GameAspects.GameSettings.Score.Two:
                return 2;
            case GameAspects.GameSettings.Score.Three:
                return 3;
            case GameAspects.GameSettings.Score.Five:
                return 5;
            case GameAspects.GameSettings.Score.Ten:
                return 10;
            case GameAspects.GameSettings.Score.Fifteen:
                return 15;
            case GameAspects.GameSettings.Score.Twenty:
                return 20;
            case GameAspects.GameSettings.Score.TwentyFive:
                return 25;
            case GameAspects.GameSettings.Score.Fifty:
                return 50;
            case GameAspects.GameSettings.Score.OneHundred:
                return 100;
            case GameAspects.GameSettings.Score.TwoHundred:
                return 200;
            case GameAspects.GameSettings.Score.ThreeHundred:
                return 300;
            case GameAspects.GameSettings.Score.FiveHundred:
                return 500;
            case GameAspects.GameSettings.Score.OneThousand:
                return 1000;
            case GameAspects.GameSettings.Score.TwoThousand:
                return 2000;
            case GameAspects.GameSettings.Score.ThreeThousand:
                return 3000;
            case GameAspects.GameSettings.Score.FiveThousand:
                return 5000;
        }
        return 0;
    }
    public int GetTimeLimit()
    {
        switch (Game.GameType.TimeLimit)
        {
            case GameAspects.GameSettings.Duration.Unlimited:
                return 0;
            case GameAspects.GameSettings.Duration.OneMinute:
                return 60;
            case GameAspects.GameSettings.Duration.TwoMinutes:
                return 120;
            case GameAspects.GameSettings.Duration.ThreeMinutes:
                return 180;
            case GameAspects.GameSettings.Duration.FiveMinutes:
                return 300;
            case GameAspects.GameSettings.Duration.TenMinutes:
                return 600;
            case GameAspects.GameSettings.Duration.FifteenMinutes:
                return 900;
            case GameAspects.GameSettings.Duration.ThirtyMinutes:
                return 1800;
            case GameAspects.GameSettings.Duration.OneHour:
                return 3600;
        }
        return 0;
    }

    public bool CheckTieGame()
    {
        if (!IsVersus())
        {
            return false;
        }
        else if (Game.GameType.TeamGame)
        {
            GameAspects.Team winningTeam = GetWinningTeam();
            foreach (GameAspects.Team team in Game.Teams)
            {
                if (team == winningTeam)
                    continue;
                if (team.Score == winningTeam.Score)
                    return true;
            }
        }
        else
        {
            GameAspects.Profile winnningProfile = GetWinningProfile();
            foreach (GameAspects.Profile profile in Game.Profiles)
            {
                if (profile == winnningProfile)
                    continue;
                if (profile.Score == winnningProfile.Score)
                    return true;
            }
        }
        return false;
    }
    public GameAspects.Team GetWinningTeam()
    {
        GameAspects.Team winningTeam = Game.Teams[0];
        if (!IsVersus())
            return Game.Teams[Game.Profiles[0].TeamId];

        foreach (GameAspects.Team team in Game.Teams)
            if (team.Score > winningTeam.Score)
                winningTeam = team;

        return winningTeam;
    }
    public GameAspects.Profile GetWinningProfile()
    {
        GameAspects.Profile winningProfile = Game.Profiles[0];

        foreach (GameAspects.Profile profile in Game.Profiles)
            if (profile.Score > winningProfile.Score)
                winningProfile = profile;

        return winningProfile;
    }

    public static float GetLookSensitivity(GameAspects.Profile.Sensitivity sensitivity)
    {
        return (float)sensitivity + 1;
    }

    public int LocalProfileCount()
    {
        int count = 0;
        foreach (GameAspects.Profile profile in Game.Profiles)
            if (profile.Local)
                count += 1;
        return count;
    }
    public int[] GetLocalProfileIndexes()
    {
        // Find each profiles 
        List<int> localPlayerIndexs = new List<int>();
        for (int i = 0; i < Game.Profiles.Count; i++)
            if (Game.Profiles[i].Local)
                localPlayerIndexs.Add(i);

        return localPlayerIndexs.ToArray();
    }


    public InanimateObject GetInanimateByGamerId(int gamerId)
    {
        // Primarily used in networking to find the inanaimate object associated with a gamer id
        if (gamerId == 0)
            return null;

        InanimateObject[] inanimateObjects = Globals.Instance.Containers.InanimateObjects.GetComponentsInChildren<InanimateObject>();

        return Array.Find(inanimateObjects, p => p.GamerId == gamerId);
    }
    public GameAspects.Profile GetProfileByGamerId(int gamerId)
    {
        if (gamerId == 0)
            return null;
        GameAspects.Profile profile = Instance.Game.Profiles.Find(p => p.GamerId == gamerId);
        return profile;
    }
    public List<GameAspects.Profile> GetTeamProfiles(int teamId)
    {
        List<GameAspects.Profile> profiles = new List<GameAspects.Profile>();
        foreach (GameAspects.Profile profile in Game.Profiles)
            if (profile.TeamId == teamId)
                profiles.Add(profile);

        return profiles;
    }

    public int[] GetScoreSortedTeamIndexes(List<GameAspects.Team> list)
    {
        List<GameAspects.Team> sortedList = new List<GameAspects.Team>();
        sortedList.AddRange(list);
        sortedList.Sort(delegate(GameAspects.Team x, GameAspects.Team y)
        {
            return y.Score.CompareTo(x.Score);
        });

        List<int> indexes = new List<int>();

        for (int i = 0; i < sortedList.Count; i++)
            for (int x = 0; x < list.Count; x++)
                if (sortedList[i].Name == list[x].Name)
                    indexes.Add(x);

        return indexes.ToArray();
    }
    public List<GameAspects.Profile> GetScoreSortedProfiles(List<GameAspects.Profile> list)
    {
        list.Sort(delegate(GameAspects.Profile x, GameAspects.Profile y)
        {
            return y.Score.CompareTo(x.Score);
        });

        return list;
    }


    public bool IsVersus()
    {
        if (!Game.GameType.TeamGame || Game.Profiles.Count <= 1)
            return true;
        else
        {
            int teamComparison = Game.Profiles[0].TeamId;
            for (int i = 1; i < Game.Profiles.Count; i++)
            {
                GameAspects.Profile profile = Game.Profiles[i];
                if (profile.TeamId != teamComparison)
                    return true;
            }
        }
        return false;
    }
    public bool IsOnlyLocalProfiles()
    {
        foreach (GameAspects.Profile profile in Game.Profiles)
            if (!profile.Local)
                return false;
        return true;
    }
    public bool CheckEnemies(GameAspects.Profile profileA, GameAspects.Profile profileB)
    {
        return Game.GameType.TeamGame && profileA.TeamId != profileB.TeamId || !Game.GameType.TeamGame && profileA != profileB;
    }


    public void AddProfileKill(int gamerId, bool fromNetwork = false)
    {
        if (gamerId == 0)
            return;

        // If the game has ended, abort
        if (!Game.Playing && !fromNetwork)
            return;


        if (!fromNetwork && NetworkSessionManager.IsClient)
        {
            NetworkSessionNode.Instance.CmdAddProfileKill(gamerId);
            return;
        }

        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcAddProfileKill(gamerId);

        // Add score to player, and if the game is team based add it to the players team as-well
        GameAspects.Profile profile = GetProfileByGamerId(gamerId);
        if (profile == null)
            return;

        profile.Kills += 1;

        if (Game.GameType.TeamGame)
            Game.Teams[profile.TeamId].Kills += 1;

        if ((!fromNetwork || NetworkSessionManager.IsHost) && GameManager.Instance.Game.GameType.KillBased)
            AddProfileScore(gamerId, fromNetwork);
    }
    public void AddProfileDeath(int gamerId, bool fromNetwork = false)
    {
        if (gamerId == 0)
            return;
        // If the game has ended, abort
        if (!Game.Playing && !fromNetwork)
            return;

            if (!fromNetwork && NetworkSessionManager.IsClient)
            {
                NetworkSessionNode.Instance.CmdAddProfileDeath(gamerId);
                return;
            }
            if (NetworkSessionManager.IsHost)
                NetworkSessionNode.Instance.RpcAddProfileDeath(gamerId);


        // Add score to player, and if the game is team based add it to the players team as-well
        GameAspects.Profile profile = GetProfileByGamerId(gamerId);
        if (profile == null)
            return;

        profile.Deaths += 1;

        if (Game.GameType.TeamGame)
            Game.Teams[profile.TeamId].Deaths += 1;
    }
    public void AddProfileScore(int gamerId, bool fromNetwork = false)
    {
        if (gamerId == 0)
            return;

        // If the game has ended, abort
        if (!Game.Playing && !fromNetwork)
            return;

        if (!fromNetwork && NetworkSessionManager.IsClient)
        {
            NetworkSessionNode.Instance.CmdAddProfileScore(gamerId);
            return;
        }
        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcAddProfileScore(gamerId);
        
        int scoreToWin = GetScoreToWin();

        // Add score to player, and if the game is team based add it to the players team as-well
        GameAspects.Profile profile = GetProfileByGamerId(gamerId);
        if (profile == null)
            return;

        profile.Score = scoreToWin == 0 ? profile.Score + 1 : Mathf.Min(profile.Score + 1, scoreToWin);

        if (Game.GameType.TeamGame)
            Game.Teams[profile.TeamId].Score = scoreToWin == 0 ? Game.Teams[profile.TeamId].Score + 1 : Mathf.Min(Game.Teams[profile.TeamId].Score + 1, scoreToWin);

        // If the score to win is unlimited, abort
        if (Game.GameType.ScoreToWin == 0)
            return;

        // If the score has reached its limit end the game
        if (profile.Score >= scoreToWin || Game.Teams[profile.TeamId].Score >= scoreToWin)
            EndGame();

    }

    [MethodReference("register_local_player")]
    public void RegisterLocalProfile(int controllerID)
    {
        if (!Instance.AllowLocalPlayerJoining)
            return;

        // Determine if the inputting player is already signed in, if so abort

        GameAspects.Profile profile = Instance.Game.Profiles.Find(p => p.ControllerId == controllerID && p.Local);
        if (profile != null)
            return;

        // Otherwise add profile, build and refresh ui
        int gamerId = UnityEngine.Random.Range(11111, 99999);
        string name = _genericPlayerNames[UnityEngine.Random.Range(0, _genericPlayerNames.Length - 1)]; ;
        bool invert = false;
#if UNITY_EDITOR
        if (System.Environment.UserName == "Adam")
            invert = true;
#endif
        GameAspects.Profile newProfile = new GameAspects.Profile(name, true, gamerId, controllerID, 0, 0, 3, invert, true);
        GameManager.Instance.Game.Profiles.Add(newProfile);
        TitleMenuManager.Instance.AddPlayerUI(newProfile);

        if (NetworkSessionManager.IsClient)
            NetworkSessionNode.Instance.CmdRegisterProfileOnHost(newProfile.Name, newProfile.GamerId, 0);

        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcRegisterProfileForClients(newProfile.Name, newProfile.GamerId, 0, NetworkSessionNode.Instance.connectionToServer.connectionId);
    }
    public bool RegisterNetworkedProfile(string name, int gamerId, int teamId, int connectionId)
    {
        GameAspects.Profile profile = Instance.Game.Profiles.Find(p => p.GamerId == gamerId);
        if (profile != null)
            return false;

        GameAspects.Profile newProfile = new GameAspects.Profile(name, false, gamerId, -1, teamId, connectionId, -1, false, false);
        Instance.Game.Profiles.Add(newProfile);
        TitleMenuManager.Instance.AddPlayerUI(newProfile);
        return true;
    }

    [MethodReference("dismiss_local_player")]
    public void DismissLocalProfile(int controllerID)
    {
        // Find the inputting player, remove ui component, remove profile, refresh ui
        GameAspects.Profile profile = Game.Profiles.Find(p => p.ControllerId == controllerID && p.Local);
        if (profile == null)
            return;

        TitleMenuManager.Instance.RemovePlayerUI(profile);
        Game.Profiles.Remove(profile);

        if (NetworkSessionManager.IsClient)
            NetworkSessionNode.Instance.CmdDismissProfileOnHost(profile.GamerId);
        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcDismissProfileForClients(profile.GamerId);

    }
    public void DismissNetworkedProfile(int gamerId)
    {
        for (int i = 0; i < Game.Profiles.Count; i++)
            if (Game.Profiles[i].GamerId == gamerId)
            {
                TitleMenuManager.Instance.RemovePlayerUI(Game.Profiles[i]);
                Game.Profiles.RemoveAt(i);
                return;
            }
    }

    [MethodReference("switch_local_player_team")]
    public void SwitchLocalProfileTeam(int controllerID)
    {
        if (!Game.GameType.TeamGame)
            return;

        GameAspects.Profile profile = Game.Profiles.Find(p => p.ControllerId == controllerID && p.Local);
        if (profile == null)
            return;
        // Find the profile of the inputting player and switch their team
        profile.TeamId++;
        if (profile.TeamId >= Game.Teams.Count)
            profile.TeamId = 0;

        if (NetworkSessionManager.IsClient)
            NetworkSessionNode.Instance.CmdSwitchProfileTeamOnHost(profile.GamerId, profile.TeamId);

        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcSwitchProfileTeamForClients(profile.GamerId, profile.TeamId);
    }
    public void SwitchNetworkedProfileTeam(int gamerId, int teamId)
    {
        for (int i = 0; i < Game.Profiles.Count; i++)
            if (Game.Profiles[i].GamerId == gamerId)
            {
                Game.Profiles[i].TeamId = teamId;
                return;
            }

    }

    public void RemoveAllNonLocalProfiles()
    {
        for (int i = 0; i < Game.Profiles.Count; i++)
        {
            GameAspects.Profile profile = Game.Profiles[i];
            if (profile.Local)
                continue;
            Game.Profiles.Remove(profile);
            TitleMenuManager.Instance.RemovePlayerUI(profile);
            i--;
        }
    }
    public void RemoveAllProfilesForConnection(int connectionId)
    {
        for (int i = 0; i < Game.Profiles.Count; i++)
        {
            GameAspects.Profile profile = Game.Profiles[i];
            if (profile.ConnectionId != connectionId)
                continue;
            Game.Profiles.Remove(profile);
            TitleMenuManager.Instance.RemovePlayerUI(profile);
            i--;
        }
    }
    #endregion
}