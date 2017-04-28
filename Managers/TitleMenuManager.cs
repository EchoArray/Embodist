using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using XInputDotNetPure;
using UnityEngine.Networking;

public class TitleMenuManager : MonoBehaviour
{
    #region Values
    public static TitleMenuManager Instance;

    public MenuControlGroup StartScreen;
    public MenuControlGroup CustomGameMenu;

    public MenuControlGroup PostGameMenuGroup;
    public Transform PostGamePlayerStatContainer;
    public GameObject PostGamePlayerStatPrefab;
    public float PostGamePlayerStatVerticalMargin;

    [Space(15)]
    public Transform PlayerUIContainer;

    [Space(5)]
    public GameObject MenuPlayerPrefab;
    public GameObject MenuPlayerJoinPrefab;
    public float MenuPlayerElementMargin;

    private int _registeredPlayerCount;
    private List<GameObject> _menuPlayerJoinObjects = new List<GameObject>();

    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateLocalPlayers();
        RefreshPlayerUI();
    }

    #endregion

    #region Functions
    private void Initialize()
    {
        if (GameManager.Dirty)
        {
            PostGameMenuGroup.Show(null);
            BuildPostGame();
            BuildPlayerUI();
            UpdateLocalPlayers();
        }
        else
        {
            StartScreen.Show(null);            
        }
        Instance = this;

        NetworkManagerHUD networkManagerHUD = FindObjectOfType<NetworkManagerHUD>();
        networkManagerHUD.showGUI = true;
    }

    public void ClientStateChanged()
    {
        CustomGameMenu.LoadAllSettings();
    }

    private void UpdateLocalPlayers()
    {
        // Defines how many players there is room for
        int availableSlots = Mathf.Min(GameManager.Instance.MaxProfileCount - GameManager.Instance.Game.Profiles.Count, 4);
        // 
        int unregisteredPlayerCount = Mathf.Min(GamePadManager.ConnectedControllerCount - _registeredPlayerCount, availableSlots);

        if (_menuPlayerJoinObjects.Count > 0 && unregisteredPlayerCount < _menuPlayerJoinObjects.Count)
        {
            int disconnectedControllerCount = _menuPlayerJoinObjects.Count - unregisteredPlayerCount;
            for (int i = 0; i < disconnectedControllerCount; i++)
            {
                GameObject menuPlayerJoinObject = _menuPlayerJoinObjects[0];
                _menuPlayerJoinObjects.RemoveAt(0);
                Destroy(menuPlayerJoinObject);
            }
        }

        int absentJoinObjectCount = unregisteredPlayerCount - _menuPlayerJoinObjects.Count;
        for (int i = 0; i < absentJoinObjectCount; i++)
            _menuPlayerJoinObjects.Add(Instantiate(MenuPlayerJoinPrefab, PlayerUIContainer));
    }

    public void BuildPlayerUI()
    {
        foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
            AddPlayerUI(profile);
    }
    public void AddPlayerUI(GameManager.GameAspects.Profile profile)
    {
        if(profile.Local)
            _registeredPlayerCount += 1;

        GameObject menuPlayerObject = Instantiate(MenuPlayerPrefab, PlayerUIContainer);
        MenuProfile menuProfile = menuPlayerObject.GetComponent<MenuProfile>();
        menuProfile.SetName(profile.Name);
        menuPlayerObject.name = "ui_player_" + profile.Name;
        profile.UIElement = menuPlayerObject;
    }
    public void RemovePlayerUI(GameManager.GameAspects.Profile profile)
    {
        if(profile.Local)
            _registeredPlayerCount -= 1;
        Destroy(profile.UIElement);
    }
    public void RefreshPlayerUI()
    {
        float horizontalOffset = 0;
        if (GameManager.Instance.Game.GameType.TeamGame)
        {
            for (int teamIndex = 0; teamIndex < GameManager.Instance.Game.Teams.Count; teamIndex++)
            {
                for (int profileIndex = 0; profileIndex < GameManager.Instance.Game.Profiles.Count; profileIndex++)
                {
                    if (GameManager.Instance.Game.Profiles[profileIndex].TeamId == teamIndex)
                    {
                        RectTransform rectTransform = GameManager.Instance.Game.Profiles[profileIndex].UIElement.GetComponent<RectTransform>();

                        float halfWidth = rectTransform.sizeDelta.x / 2;
                        Vector2 position = new Vector2(halfWidth + ((rectTransform.sizeDelta.x + MenuPlayerElementMargin) * profileIndex), 0);
                        rectTransform.anchoredPosition = position;
                        float newHorizontalOffset = position.x + halfWidth;
                        if (newHorizontalOffset > horizontalOffset)
                            horizontalOffset = newHorizontalOffset;

                        rectTransform.localScale = Vector3.one;

                        MaskableGraphic maskableGraphic = GameManager.Instance.Game.Profiles[profileIndex].UIElement.GetComponent<MaskableGraphic>();

                        maskableGraphic.color = GameManager.Instance.Game.Teams[GameManager.Instance.Game.Profiles[profileIndex].TeamId].Color;
                    }
                }
            }
        }
        else
        {
            for (int profileIndex = 0; profileIndex < GameManager.Instance.Game.Profiles.Count; profileIndex++)
            {
                RectTransform rectTransform = GameManager.Instance.Game.Profiles[profileIndex].UIElement.GetComponent<RectTransform>();
                float halfWidth = rectTransform.sizeDelta.x / 2;

                Vector2 position = new Vector2(halfWidth + ((rectTransform.sizeDelta.x + MenuPlayerElementMargin) * profileIndex), 0);
                rectTransform.anchoredPosition = position;
                horizontalOffset = position.x + halfWidth;

                rectTransform.localScale = Vector3.one;

                MaskableGraphic maskableGraphic = GameManager.Instance.Game.Profiles[profileIndex].UIElement.GetComponent<MaskableGraphic>();
                maskableGraphic.color = GameManager.Instance.PlayerColor;
            }
        }

        for (int menuPlayerJoinIndex = 0; menuPlayerJoinIndex < _menuPlayerJoinObjects.Count; menuPlayerJoinIndex++)
        {
            RectTransform rectTransform = _menuPlayerJoinObjects[menuPlayerJoinIndex].GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            if (menuPlayerJoinIndex == 0 && horizontalOffset != 0)
            {
                rectTransform.anchoredPosition = new Vector2(horizontalOffset + (rectTransform.sizeDelta.x / 2) + MenuPlayerElementMargin, 0);
                horizontalOffset += MenuPlayerElementMargin;
            }
            else
                rectTransform.anchoredPosition = new Vector2(horizontalOffset + (rectTransform.sizeDelta.x / 2) + ((rectTransform.sizeDelta.x + MenuPlayerElementMargin) * menuPlayerJoinIndex), 0);
        }

    }

    public void BuildPostGame()
    {
        int postion = 0;
        if (GameManager.Instance.Game.GameType.TeamGame)
        {
            for (int teamIndex = 0; teamIndex < GameManager.Instance.Game.Teams.Count; teamIndex++)
            {
                foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
                    if (profile.TeamId == teamIndex)
                    {
                        GameObject newPlayerStat = Instantiate(PostGamePlayerStatPrefab, PostGamePlayerStatContainer);

                        MenuStat menuPlayerStats = newPlayerStat.GetComponent<MenuStat>();
                        menuPlayerStats.Populate(profile);

                        RectTransform playerStatRectTransform = newPlayerStat.GetComponent<RectTransform>();
                        playerStatRectTransform.localScale = Vector3.one;
                        playerStatRectTransform.sizeDelta = new Vector2(playerStatRectTransform.sizeDelta.x, playerStatRectTransform.sizeDelta.y);
                        playerStatRectTransform.anchoredPosition = new Vector2((playerStatRectTransform.sizeDelta.x / 2), -(playerStatRectTransform.sizeDelta.y / 2) - (playerStatRectTransform.sizeDelta.y * postion) - (PostGamePlayerStatVerticalMargin * postion));

                        postion++;
                    }
            }
        }
        else
        {
            foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
            {
                GameObject newPlayerStat = Instantiate(PostGamePlayerStatPrefab, PostGamePlayerStatContainer);

                MenuStat menuPlayerStats = newPlayerStat.GetComponent<MenuStat>();
                menuPlayerStats.Populate(profile);

                RectTransform playerStatRectTransform = newPlayerStat.GetComponent<RectTransform>();
                playerStatRectTransform.localScale = Vector3.one;
                playerStatRectTransform.sizeDelta = new Vector2(playerStatRectTransform.sizeDelta.x, playerStatRectTransform.sizeDelta.y);
                playerStatRectTransform.anchoredPosition = new Vector2((playerStatRectTransform.sizeDelta.x / 2), -(playerStatRectTransform.sizeDelta.y / 2) - (playerStatRectTransform.sizeDelta.y * postion) - (PostGamePlayerStatVerticalMargin * postion));

                postion++;
            }
        }

    }
    #endregion
}