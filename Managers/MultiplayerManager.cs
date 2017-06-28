using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerManager : MonoBehaviour
{
    #region Values
    public static MultiplayerManager Instance;
    /// <summary>
    /// Defines the prefab for local player
    /// </summary>
    public GameObject PlayerPrefab;
    /// <summary>
    /// Defines the prefab for the camera controller
    /// </summary>
    public GameObject CameraControllerPrefab;
    /// <summary>
    /// Defines the prefab for the heads up display
    /// </summary>
    public GameObject HeadsUpDisplayPrefab;

    /// <summary>
    /// Defines the prefab for the territory objective
    /// </summary>
    [Space(15)]
    public GameObject TerritoryPrefab;

    public GameObject HotLava;

    public MenuControlGroup EmojiMenuControlGroup;
    public Scoreboard Scoreboard;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }
    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        UpdateTimeLimit();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("Title");
            return;
        }
        Instance = this;
    }

    private void InitializeGame()
    {
        int[] localProfileIndexs = GameManager.Instance.GetLocalProfileIndexes();
        HUDManager.Instance.ShowScreenSplitters(localProfileIndexs.Length);
        InstantiateLocalPlayers(localProfileIndexs);

        Decal.AllowDecals = localProfileIndexs.Length == 1;


        GameManager.Instance.Game.Playing = true;
        GameManager.Instance.Game.GameType.TimeLimitRemaining = GameManager.Instance.GetTimeLimit();

        Globals.Instance.Containers.AreaIsolators.gameObject.SetActive(GameManager.Instance.Game.GameType.IsolateAreas);
        HotLava.SetActive(GameManager.Instance.Game.GameType.HotLava);


    }

    private void InstantiateLocalPlayers(int[] localProfilesIndexs)
    {
        // Instantiate player, camera and hud
        for (int i = 0; i < localProfilesIndexs.Length; i++)
        {
            // Create local player and set defaults
            GameObject newLocalPlayer = Instantiate(PlayerPrefab, Globals.Instance.Containers.Players);
            newLocalPlayer.name = PlayerPrefab.name + "_" + GameManager.Instance.Game.Profiles[localProfilesIndexs[i]].ControllerId;
            LocalPlayer localPlayer = newLocalPlayer.GetComponent<LocalPlayer>();
            GameManager.Instance.Game.Profiles[localProfilesIndexs[i]].LocalPlayer = localPlayer;
            localPlayer.Profile = GameManager.Instance.Game.Profiles[localProfilesIndexs[i]];

            // Create camera and set defaults
            GameObject newCameraController = Instantiate(CameraControllerPrefab, Vector3.zero, Quaternion.identity, Globals.Instance.Containers.Cameras);
            newCameraController.name = CameraControllerPrefab.name + "_" + localProfilesIndexs[i];
            localPlayer.CameraController = newCameraController.GetComponent<CameraController>();
            localPlayer.CameraController.LocalPlayer = localPlayer;
            localPlayer.CameraController.AppropriateRect(localProfilesIndexs.Length, i);
            SpawnManager.Instance.RespawnCamera(localPlayer);

            // Create heads up display and set defaults
            GameObject newHeadsUpDisplay = Instantiate(HeadsUpDisplayPrefab);
            newHeadsUpDisplay.name = HeadsUpDisplayPrefab.name + "_" + localProfilesIndexs[i];
            newHeadsUpDisplay.transform.SetAsFirstSibling();
            localPlayer.HeadsUpDisplay = newHeadsUpDisplay.GetComponent<HeadsUpDisplay>();
            localPlayer.HeadsUpDisplay.LocalPlayer = localPlayer;
            HUDManager.Instance.HeadsUpDisplays.Add(localPlayer.HeadsUpDisplay);
        }
    }

    private void UpdateTimeLimit()
    {
        if (!GameManager.Instance.Game.Playing || GameManager.Instance.GetTimeLimit() == 0)
            return;

        GameManager.Instance.Game.GameType.TimeLimitRemaining = Mathf.Max(GameManager.Instance.Game.GameType.TimeLimitRemaining - Time.deltaTime, 0);
        if (GameManager.Instance.Game.GameType.TimeLimitRemaining == 0)
            GameManager.Instance.EndGame();
    }
    #endregion
}
