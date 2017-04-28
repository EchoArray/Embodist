using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Territory : NetworkBehaviour
{
    #region Values
    /// <summary>
    /// Defines the name of the territory.
    /// </summary>
    [HideInInspector]
    [SyncVar]
    private string _name;

    /// <summary>
    /// Determines the type of the territory.
    /// </summary>
    public enum TerritoryType
    {
        Capture,
        Hill,
        Arm
    }
    [SyncVar]
    public TerritoryType Type;

    /// <summary>
    /// Defines the offset from the territories position that the waypoint positioned.
    /// </summary>
    [Space(15)]
    public Vector3 WaypointOffset;

    /// <summary>
    /// Defines the owning team of the territory.
    /// </summary>
    [SyncVar]
    public int TeamId;

    // Defines the owning team of the territory.
    [SyncVar]
    private int _captorTeam = 0;
    // Defines the player that captured the territory.
    [SyncVar]
    private int _captorGamerId;
    // Defines the progress from zero to the capture duration in which the territory has been held for capture.
    [SyncVar]
    private float _captureProgress;
    // Determines if the territory is currently captured.
    private bool _captured;

    // A list of all inanimate objects present in the territory.
    private List<InanimateObject> _occupants = new List<InanimateObject>();

    // Defines the progression to one second, as to when the next addition will be made to the score of the captor.
    private float _scoreAdditionProgress = 0;

    // Defines the material of the territories renderer.
    private Material _material;

    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        SetWaypoint();
    }

    private void Update()
    {
        ResizeOccupants();
        UpdateTerritory();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (NetworkSessionManager.IsClient)
            return;
        InanimateObject inanimateObject = collider.GetComponent<InanimateObject>();
        if (inanimateObject != null && inanimateObject.Controlled && !_occupants.Contains(inanimateObject))
        {
            if (Type == TerritoryType.Arm && (inanimateObject.Class != InanimateObject.Classification.Bomb || inanimateObject.TeamId == TeamId))
                return;

            _occupants.Add(inanimateObject);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        InanimateObject inanimateObject = collider.GetComponent<InanimateObject>();
        if (inanimateObject != null && inanimateObject.Controlled)
            _occupants.Remove(inanimateObject);
    } 
    #endregion

    #region Functions
    private void Initialize()
    {
        // Define material instance, set, define initial values
        Renderer renderer = this.gameObject.GetComponent<Renderer>();
        _material = new Material(renderer.material);
        renderer.material = _material;
    }

    public void SetDefaults(TerritoryType type, int teamId, string name)
    {
        Type = type;
        TeamId = teamId;
        _name = name;
    }

    private void ResizeOccupants()
    {
        if (NetworkSessionManager.IsClient)
            return;

        // Remove all missing occupant references
        if (_occupants.Count > 0)
            _occupants.RemoveAll(occupant => occupant == null);
    }
    private void GetOccupationInfo(out int occupyingTeam, out bool contested)
    {
        occupyingTeam = 0;
        contested = false;
        if (GameManager.Instance.Game.GameType.TeamGame)
        {
            for (int occupant = 0; occupant < _occupants.Count; occupant++)
            {
                // If the occupant index is 0 define initial team
                if (occupant == 0)
                {
                    occupyingTeam = _occupants[occupant].TeamId;
                    continue;
                }

                // If the game is team based and there is more than one team in the territory; it is contested
                if (_occupants[occupant].TeamId != occupyingTeam)
                {
                    contested = true;
                    break;
                }
            }
        }
        else
            // If the game isn't team based and there is more than one occupant; it is contested
            contested = _occupants.Count > 1;
    }

    private void SetMaterialProgress(float progress)
    {
        _material.SetFloat("_Progress", progress);
    }
    private void SetMaterialFlashing(bool state)
    {
        _material.SetFloat("_Flashing", state ? 1 : 0);
    }
    private void SetMaterialProgressColor()
    {
        if (Type == TerritoryType.Arm)
            return;

        Color color = GameManager.Instance.Game.GameType.TeamGame ? GameManager.Instance.Game.Teams[_captorTeam].Color
            : GameManager.Instance.PlayerColor;
        _material.SetColor("_ColorB", color);
    }


    public void SetWaypoint()
    {
        // Add this code to hud manager (also see player to add manager support there as well)
        foreach (HeadsUpDisplay headsUpDisplay in HUDManager.Instance.HeadsUpDisplays)
        {
            if (Type == TerritoryType.Arm && headsUpDisplay.LocalPlayer.Profile.TeamId == TeamId)
                return;

            HeadsUpDisplay.Waypoint.WaypointType type = HeadsUpDisplay.Waypoint.WaypointType.ObjectiveCapture;
            switch (Type)
            {
                case TerritoryType.Capture:
                    break;
                case TerritoryType.Hill:
                    type = HeadsUpDisplay.Waypoint.WaypointType.ObjectiveKOTH;
                    break;
                case TerritoryType.Arm:
                    type = HeadsUpDisplay.Waypoint.WaypointType.ObjectiveArm;
                    break;
            }
            headsUpDisplay.AddWaypoint(type, this.transform, WaypointOffset, _name.ToUpper(), HUDManager.Instance.Colors.DefaultColor);
        }
    }
    private void RefreshWaypoint(float progress)
    {
        if (Type == TerritoryType.Arm)
            return;

        foreach (HeadsUpDisplay headsUpDisplay in HUDManager.Instance.HeadsUpDisplays)
        {
            // Define waypoint color
            Color waypointColor = GameManager.Instance.Game.GameType.TeamGame ? GameManager.Instance.Game.Teams[_captorTeam].Color : GameManager.Instance.PlayerColor;
            if (progress == 0)
                waypointColor = HUDManager.Instance.Colors.DefaultColor;

            // Refresh waypoint
            headsUpDisplay.RefreshWaypoint(this.transform, _name.ToUpper(), waypointColor, progress);
        }
    }


    private void UpdateTerritory()
    {
        // If there are no occupants and the territory is a hill, clear the progress of the material
        bool pushScore = true;
        if (!NetworkSessionManager.IsClient)
        {
            if (_occupants.Count == 0)
            {
                if (Type == TerritoryType.Hill && _captured || Type == TerritoryType.Arm)
                    Nuetralize();
            }
            else
            {
                // Determine if territory is contested
                int occupyingTeam = 0;
                bool contested = false;
                GetOccupationInfo(out occupyingTeam, out contested);
                pushScore = !contested;

                if (contested)
                    return;

                if (Type == TerritoryType.Hill)
                {
                    // If the territory is a hill capture instantly
                    _captureProgress = GameManager.Instance.Game.GameType.TerritoryCaptureDuration;
                }
                else if (occupyingTeam != _captorTeam && _captureProgress > 0)
                {
                    // If the territory has capture progress toward the previous occupying team, and is currently occupied by the competing team, take away the previous teams progress
                    _captureProgress = Mathf.Max(_captureProgress -= Time.deltaTime, 0);
                }
                else if (_captureProgress == 0)
                {
                    // If the territory is occupied by the competing team and the progress is zero, capture the territory and advance progress
                    Nuetralize();
                    _captorTeam = occupyingTeam;
                    _captureProgress += Time.deltaTime;
                }
                else if (occupyingTeam == _captorTeam && _captureProgress != GameManager.Instance.Game.GameType.TerritoryCaptureDuration)
                {
                    // If the territory is below captured progress and the occupying team is the owning team, advance progress
                    _captureProgress = Mathf.Min(_captureProgress += Time.deltaTime, GameManager.Instance.Game.GameType.TerritoryCaptureDuration);
                    _captorGamerId = _occupants[0].GamerId;
                }

                if (_captureProgress == GameManager.Instance.Game.GameType.TerritoryCaptureDuration && !_captured)
                {
                    _captorGamerId = _occupants[0].GamerId;

                    if (Type == TerritoryType.Arm)
                        DetonateBomb();
                    else
                        Capture(occupyingTeam);
                }
            }
        }
        
        // Update territory material
        float progress = _captureProgress / GameManager.Instance.Game.GameType.TerritoryCaptureDuration;

        SetMaterialProgress(progress);
        SetMaterialFlashing(progress != 1);
        SetMaterialProgressColor();

        RefreshWaypoint(progress);

        if (pushScore && progress == 1 && Type != TerritoryType.Arm)
            TickScore();
    }

    private void Capture(int team)
    {
        _captured = true;
        _captorTeam = team;
        _captorGamerId = _occupants[0].GamerId;
       // if (!Hill)
          //  _captorGamerId.AttachedCapturedTerritory(Name);
    }

    private void DetonateBomb()
    {
        if (!NetworkSessionManager.IsClient)
        {

            bool local = GameManager.Instance.GetProfileByGamerId(_captorGamerId).Local;
            if (local)
                _occupants[0].Die(false);
            else
                RpcDetonateBomb();

            Nuetralize();
            GameManager.Instance.AddProfileScore(_captorGamerId);
        }
    }
    [ClientRpc]
    private void RpcDetonateBomb()
    {
        bool local = GameManager.Instance.GetProfileByGamerId(_captorGamerId).Local;
        if (local)
        {
            InanimateObject inanimateObject = GameManager.Instance.GetInanimateByGamerId(_captorGamerId);
            inanimateObject.Die(false);
        }
    }

    private void Nuetralize()
    {
        _captureProgress = 0;
        _scoreAdditionProgress = 0;
        _captured = false;
    }

    private void TickScore()
    {
        // If the territory isn't captured or the hill isn't occupied; reset values and abort
        if (!_captured || Type == TerritoryType.Hill && _occupants.Count == 0)
        {
            _scoreAdditionProgress = 0;
            return;
        }

        // Add to score time
        _scoreAdditionProgress += Time.deltaTime;

        if (_scoreAdditionProgress >= 1)
        {
            // If score time is complete; clear score time, add to score
            _scoreAdditionProgress = 0;
            GameManager.Instance.AddProfileScore(_captorGamerId);
        }
    }
    #endregion
}
