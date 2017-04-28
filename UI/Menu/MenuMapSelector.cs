using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MenuMapSelector : MenuControl
{
    #region Values
    /// <summary>
    /// Defines the menu group this option is part of
    /// </summary>
    public Text MapNameLabel;

    [Space(15)]
    public bool AutoSelectLastLoaded;
    public int ActiveOption;
    [Serializable]
    public class Option
    {
        public string Name;
        public string SceneName;
        public Texture2D PreviewImage;
    }
    public Option[] Options;

    #endregion
    private void Awake()
    {
        base.Awake();
        if (GameManager.Dirty)
        {
            if (AutoSelectLastLoaded)
                ChangeToMap(GameManager.Instance.Game.SelectedMapName);
        }
        else
            SelectMap(ActiveOption);
    }

    #region Functions

    public void ChangeToMap(string sceneName)
    {
        for (int i = 0; i < Options.Length; i++)
        {
            if (Options[i].SceneName == sceneName)
            {
                ActiveOption = i;

                SelectMap(ActiveOption);
                GameManager.Instance.Game.SelectedMapName = sceneName;
                return;
            }
        }
    }
    public void Change(bool forward)
    {
        // Increase active index positon
        ActiveOption += forward ? 1 : -1;

        if (ActiveOption > Options.Length - 1)
            ActiveOption = 0;
        else if (ActiveOption < 0)
            ActiveOption = Options.Length - 1;

        SelectMap(ActiveOption);
    }

    public void SelectMap(int index)
    {
        MapNameLabel.text = Options[index].Name.ToUpper();

        GameManager.Instance.Game.SelectedMapName = Options[index].SceneName;

        SetTexture(Options[index].PreviewImage);
        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcChangeMapForClients(Options[index].SceneName);
    }
    #endregion
}