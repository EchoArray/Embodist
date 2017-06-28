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
    /// <summary>
    /// Defines the menu control group for the start screen.
    /// </summary>
    public MenuControlGroup StartScreen;
    /// <summary>
    /// Defines the menu control group for the custom game menu.
    /// </summary>
    public MenuControlGroup CustomGameMenu;
    /// <summary>
    /// Defines the menu control group for the post game menu.
    /// </summary>
    public MenuControlGroup PostGameMenu;
    /// <summary>
    /// Defines the scorboard for the post game menu.
    /// </summary>
    public Scoreboard PostGameScoreBoard;

    /// <summary>
    /// Defines the container of instantiated player ui elements.
    /// </summary>
    [Space(15)]
    public Transform PlayerUIContainer;

    /// <summary>
    /// Defines the menu player element instantiated to represent a registed player.
    /// </summary>
    [Space(5)]
    public GameObject MenuPlayerPrefab;
    /// <summary>
    /// Defines the menu player join element instantiated to represent an absent registration.
    /// </summary>
    public GameObject MenuPlayerJoinPrefab;
    /// <summary>
    /// Defines the margin between menu player ui elements.
    /// </summary>
    public float MenuPlayerElementMargin;

    // Defines the ammount of registed players.
    private int _registeredPlayerCount;
    // A collection of menu player join elements.
    private List<GameObject> _menuPlayerJoinObjects = new List<GameObject>();

    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateJoinUI();
        UpdateUIPositions();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (GameManager.Dirty)
        {
            PostGameMenu.Show(null);
            AddAllPlayersUI();
            UpdateJoinUI();
        }
        else
            StartScreen.Show(null);

        Instance = this;
    }

    public void ClientStateChanged()
    {
        CustomGameMenu.LoadAllSettings();
    }

    private void UpdateJoinUI()
    {
        // Defines how many players there is room for in the game - eg if the max is 8 and there are 7 players only show 1 slot
        int availableSlots = Mathf.Min(GameManager.Instance.MaxProfileCount - GameManager.Instance.Game.Profiles.Count, 4);
        // Defines how many unregisted players there are
        int unregisteredPlayerCount = Mathf.Min(XboxInputManager.ConnectedControllerCount - _registeredPlayerCount, availableSlots);

        // Remove existing menu objects if there are less unregistered players than menu objects
        if (_menuPlayerJoinObjects.Count > 0 && unregisteredPlayerCount < _menuPlayerJoinObjects.Count)
        {
            int removalCount = _menuPlayerJoinObjects.Count - unregisteredPlayerCount;
            for (int i = 0; i < removalCount; i++)
            {
                RemovePlayerJoinUI(0);

                if (i < 0)
                    i--;
            }
        }
        else if (unregisteredPlayerCount > _menuPlayerJoinObjects.Count)
        {
            int absentCount = unregisteredPlayerCount - _menuPlayerJoinObjects.Count;
            for (int i = 0; i < absentCount; i++)
                _menuPlayerJoinObjects.Add(Instantiate(MenuPlayerJoinPrefab, PlayerUIContainer));
        }

    }
    private void UpdateUIPositions()
    {
        float offset = 0;

        for (int i = 0; i < GameManager.Instance.Game.Teams.Count; i++)
            for (int x = 0; x < GameManager.Instance.Game.Profiles.Count; x++)
                offset = RefreshPlayerUI(i, x, offset);

        for (int i = 0; i < _menuPlayerJoinObjects.Count; i++)
            offset = RefreshPlayerJoinUI(i, offset);
    }

    public void AddAllPlayersUI()
    {
        foreach (GameManager.GameAspects.Profile profile in GameManager.Instance.Game.Profiles)
            AddPlayerUI(profile);
    }

    public void RemovePlayerJoinUI(int index)
    {
        Destroy(_menuPlayerJoinObjects[index]);
        _menuPlayerJoinObjects.RemoveAt(index);
    }
    public void RemovePlayerUI(GameManager.GameAspects.Profile profile)
    {
        if(profile.Local)
            _registeredPlayerCount -= 1;
        Destroy(profile.MenuUIElement);
    }

    public void AddPlayerJoinUI()
    {
        _menuPlayerJoinObjects.Add(Instantiate(MenuPlayerJoinPrefab, PlayerUIContainer));
    }
    public void AddPlayerUI(GameManager.GameAspects.Profile profile)
    {
        if (profile.Local)
            _registeredPlayerCount += 1;

        GameObject menuPlayerObject = Instantiate(MenuPlayerPrefab, PlayerUIContainer);
        MenuProfile menuProfile = menuPlayerObject.GetComponent<MenuProfile>();
        menuProfile.SetName(profile.Name);
        menuPlayerObject.name = "ui_player_" + profile.Name;
        profile.MenuUIElement = menuPlayerObject;
    }

    public float RefreshPlayerUI(int teamIndex, int profileIndex, float offset)
    {
        if (GameManager.Instance.Game.Profiles[profileIndex].TeamId == teamIndex)
        {
            RectTransform rectTransform = GameManager.Instance.Game.Profiles[profileIndex].MenuUIElement.GetComponent<RectTransform>();

            float halfWidth = rectTransform.sizeDelta.x / 2;
            Vector2 position = new Vector2(halfWidth + ((rectTransform.sizeDelta.x + MenuPlayerElementMargin) * profileIndex), 0);

            rectTransform.anchoredPosition = position;
            float newOffset = position.x + halfWidth;
            if (newOffset > offset)
                offset = newOffset;

            rectTransform.localScale = Vector3.one;

            MaskableGraphic maskableGraphic = GameManager.Instance.Game.Profiles[profileIndex].MenuUIElement.GetComponent<MaskableGraphic>();
            maskableGraphic.color = GameManager.Instance.Game.GameType.TeamGame ? GameManager.Instance.Game.Teams[GameManager.Instance.Game.Profiles[profileIndex].TeamId].Color :
                 GameManager.Instance.PlayerColor;
        }
        return offset;
    }
    public float RefreshPlayerJoinUI(int index, float offset)
    {
        RectTransform rectTransform = _menuPlayerJoinObjects[index].GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        if (index == 0 && offset != 0)
        {
            rectTransform.anchoredPosition = new Vector2(offset + (rectTransform.sizeDelta.x / 2) + MenuPlayerElementMargin, 0);
            return offset += MenuPlayerElementMargin;
        }
        else
            rectTransform.anchoredPosition = new Vector2(offset + (rectTransform.sizeDelta.x / 2) + ((rectTransform.sizeDelta.x + MenuPlayerElementMargin) * index), 0);
        return offset;
    }
    #endregion
}