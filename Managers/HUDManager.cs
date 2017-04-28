using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HUDManager : MonoBehaviour
{
    #region Values
    public static HUDManager Instance;

    public List<HeadsUpDisplay> HeadsUpDisplays = new List<HeadsUpDisplay>();

    public GameObject GameOverComponent;
    public Text GameOverWinText;
    public MaskableGraphic GameOverWinBand;


    public float AlertAttachedRange = 32f;
    public GameObject BeaconPrefab;
    public float BeaconCooldownDuration;
    public LayerMask BeaconSetIgnoredLayers;

    [Serializable]
    public class GenericColors
    {
        public Color DefaultColor;
        public Color FriendlyWaypointColor;
    }
    public GenericColors Colors;

    [Serializable]
    public class SplitScreenSettings
    {
        public GameObject HorizontalSplitter;
        public GameObject VerticalSplitter;
        public GameObject VerticalSplitterHalf;
        public GameObject SplitEdgeCoverA;
        public GameObject SplitEdgeCoverB;
    }
    [Space(10)]
    public SplitScreenSettings SplitScreen;

    [Serializable]
    public class WaypointSettings
    {
        public GameObject TeamMatePrefab;
        public GameObject CapturePrefab;
        public GameObject KOTHPrefab;
        public GameObject ArmPrefab;
        public GameObject BombPrefab;
        public GameObject ControlAlertPrefab;
        public GameObject EnemyPrefab;
        public GameObject BeaconPrefab;
    }
    [Space(10)]
    public WaypointSettings Waypoints;

    [Serializable]
    public class FeedSettings
    {
        /// <summary>
        /// Defines the duration in-which a feed item will display.
        /// </summary>
        public float FeedItemDuration;
        /// <summary>
        /// Defines the alpha of the feed item over its duration.
        /// </summary>
        public AnimationCurve FeedItemAlphaOverLifetime;
        /// <summary>
        /// Defines the collective string of all feed items.
        /// </summary>
        [HideInInspector]
        public string String;
    }
    [Space(10)]
    public FeedSettings Feed;
    public class FeedItem
    {
        /// <summary>
        /// Defines the text for the feed item.
        /// </summary>
        public string Text;
        /// <summary>
        /// Defines the duration in-which the feed item displays.
        /// </summary>
        public float Duration;
        /// <summary>
        /// Defines the duration of which the feed item has displayed.
        /// </summary>
        public float DurationRemaining;
        public FeedItem(string text, float duration)
        {
            Text = text;
            Duration = duration;
            DurationRemaining = duration;
        }
    }
    private List<FeedItem> _feed = new List<FeedItem>();
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateFeed();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        Instance = this;
    }

    private void UpdateFeed()
    {
        // Loop through each feed item, and create the text for it
        string feedString = string.Empty;
        for (int i = 0; i < _feed.Count; i++)
        {
            FeedItem feedItem = _feed[i];
            // Defines the alpha of the strings color
            float alpha = Feed.FeedItemAlphaOverLifetime.Evaluate(1 - (feedItem.DurationRemaining / feedItem.Duration));
            // Define the strings color and its hex string
            Color color = new Color(Colors.DefaultColor.r, Colors.DefaultColor.g, Colors.DefaultColor.b, alpha);
            string hexColor = ((byte)(color.r * 255)).ToString("X2") + ((byte)(color.g * 255)).ToString("X2") + ((byte)(color.b * 255)).ToString("X2") + ((byte)(color.a * 255)).ToString("X2");
            // Add constructed string to buffer
            feedString += "<color=#" + hexColor + ">" + feedItem.Text + "</color> \n";

            // Tick down the item
            feedItem.DurationRemaining = Mathf.Max(feedItem.DurationRemaining - Time.deltaTime, 0);
            if (feedItem.DurationRemaining == 0)
                _feed.Remove(feedItem);

        }
        // Set feed string
        Feed.String = feedString.ToUpper();
    }
    public void AddFeedItem(string text)
    {
        _feed.Add(new FeedItem(text, Feed.FeedItemDuration));
    }

    public void AddWaypointToTeamsHud(int teamId, HeadsUpDisplay.Waypoint.WaypointType type, Transform transform, Vector3 worldOffset, string text = "", Color color = new Color())
    {
        foreach (HeadsUpDisplay headsUpDisplay in HeadsUpDisplays)
        {
            if (headsUpDisplay.LocalPlayer.Profile.TeamId == teamId)
                headsUpDisplay.AddWaypoint(type, transform, worldOffset, text, color);
        }
    }

    public void AddWaypointToAllHuds( HeadsUpDisplay.Waypoint.WaypointType type, Transform transform, Vector3 worldOffset, string text = "", Color color = new Color())
    {
        foreach (HeadsUpDisplay headsUpDisplay in HeadsUpDisplays)
                headsUpDisplay.AddWaypoint(type, transform, worldOffset, text, color);
    }

    public void PopulateWaypointsOnControl(int gamerId)
    {
        GameManager.GameAspects.Profile profile = GameManager.Instance.GetProfileByGamerId(gamerId);
        InanimateObject inanimateObject = GameManager.Instance.GetInanimateByGamerId(gamerId);
        if (inanimateObject == null)
            return;

        foreach (HeadsUpDisplay headsUpDisplay in HeadsUpDisplays)
        {
            if (gamerId == headsUpDisplay.LocalPlayer.Profile.GamerId)
                continue;


            bool enemies = GameManager.Instance.CheckIfProfilesEnemies(headsUpDisplay.LocalPlayer.Profile, profile);
            bool attached = headsUpDisplay.LocalPlayer.InanimateObject != null;


            // Add team waypoints
            if (!enemies)
            {
                if (inanimateObject.Class == InanimateObject.Classification.Bomb)
                    headsUpDisplay.RefreshWaypoint(inanimateObject.transform, "", Colors.FriendlyWaypointColor);

                headsUpDisplay.AddWaypoint(HeadsUpDisplay.Waypoint.WaypointType.TeamMate, inanimateObject.transform, inanimateObject.WaypointOffset, profile.Name, HUDManager.Instance.Colors.FriendlyWaypointColor);
            }
            else if (attached)
            {
                // Show enemies for smashy
                if (headsUpDisplay.LocalPlayer.InanimateObject.Class == InanimateObject.Classification.Smashy)
                    headsUpDisplay.AddWaypoint(HeadsUpDisplay.Waypoint.WaypointType.Enemy, inanimateObject.transform, Vector3.zero);

                // Show nearby enemy attach alert
                float distance = Vector3.Distance(headsUpDisplay.LocalPlayer.CameraController.transform.position, inanimateObject.transform.position);
                if (distance <= AlertAttachedRange)
                {
                    headsUpDisplay.AddWaypoint(HeadsUpDisplay.Waypoint.WaypointType.AlertAttached, inanimateObject.transform, Vector3.zero);
                    headsUpDisplay.AddPointer(HeadsUpDisplay.Pointer.Type.Alert, inanimateObject.transform.position);
                }
            }
        }
    }

    public void PopulateEnemyWaypoints(HeadsUpDisplay headsUpDisplay)
    {
        foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
        {
            InanimateObject inanimateObject = GameManager.Instance.GetInanimateByGamerId(profile.GamerId);
            if (inanimateObject == null)
                continue;

            bool enemies = GameManager.Instance.CheckIfProfilesEnemies(headsUpDisplay.LocalPlayer.Profile, profile);
            if (!enemies)
                continue;

            headsUpDisplay.AddWaypoint(HeadsUpDisplay.Waypoint.WaypointType.Enemy, inanimateObject.transform, Vector3.zero);
        }
    }


    public void ShowScreenSplitters(int screenCount)
    {
        if (screenCount == 1)
            return;
        // Determines which screen splitters are active
        if (screenCount == 2)
        {
            // If there are two players, show only the horizontal splitter
            SplitScreen.HorizontalSplitter.SetActive(true);
        }
        else if (screenCount == 3)
        {
            // If there are three players, show the half vertical splitter, the horizontal splitter, and the edge covers
            SplitScreen.SplitEdgeCoverA.SetActive(true);
            SplitScreen.SplitEdgeCoverB.SetActive(true);
            SplitScreen.HorizontalSplitter.SetActive(true);
            SplitScreen.VerticalSplitterHalf.SetActive(true);
        }
        else if (screenCount == 4)
        {
            // If there are four players, show both horizontal and vertical splitters
            SplitScreen.HorizontalSplitter.SetActive(true);
            SplitScreen.VerticalSplitter.SetActive(true);
        }
    }
    public void ShowGameOver()
    {
        GameOverComponent.SetActive(true);

        if (GameManager.Instance.CheckTieGame())
        {
            GameOverWinText.text = "TIE GAME!";
        }
        else if(GameManager.Instance.Game.GameType.TeamGame)
        {
            GameManager.GameAspects.Team winningTeam = GameManager.Instance.GetWinningTeam();
            GameOverWinText.text = winningTeam.Name.ToUpper() + " WINS!";
            GameOverWinText.color = winningTeam.Color;
            GameOverWinBand.color = winningTeam.Color;
        }
        else
        {
            GameManager.GameAspects.Profile winningProfile = GameManager.Instance.GetWinningProfile();
            GameOverWinText.text = winningProfile.Name.ToUpper() + " WINS!";
            GameOverWinText.color = GameManager.Instance.PlayerColor;
            GameOverWinBand.color = GameManager.Instance.PlayerColor;
        }


    }
    #endregion
}
