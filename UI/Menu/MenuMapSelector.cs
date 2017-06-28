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
        public bool Disabled;
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
        if (forward)
            ActiveOption = ActiveOption == Options.Length - 1 ? 0 : ActiveOption + 1;
        else
            ActiveOption = ActiveOption == 0 ? Options.Length - 1 : ActiveOption - 1;
        
        if (forward)
        {
            for (int i = ActiveOption; i < Options.Length; i++)
            {
                if (!Options[i].Disabled)
                {
                    ActiveOption = i;
                    break;
                }

                if (i == Options.Length - 1)
                    i = 0;
            }
        }
        else
        {
            for (int i = ActiveOption; i >= 0; i--)
            {
                if (!Options[i].Disabled)
                {
                    ActiveOption = i;
                    break;
                }

                if (i == 0)
                    i = Options.Length;
            }
        }

        SelectMap(ActiveOption);
    }

    public void SelectMap(int index)
    {
        if (Options[index].Disabled)
        {
            Change(true);
            return;
        }

        MapNameLabel.text = Options[index].Name.ToUpper();

        GameManager.Instance.Game.SelectedMapName = Options[index].SceneName;

        SetTexture(Options[index].PreviewImage);
        if (NetworkSessionManager.IsHost)
            NetworkSessionNode.Instance.RpcChangeMapForClients(Options[index].SceneName);
    }
    #endregion
}