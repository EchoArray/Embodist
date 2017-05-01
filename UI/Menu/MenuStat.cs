using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuStat : MonoBehaviour
{
    public Text Name;
    public Text Score;
    public Text Kills;
    public Text Deaths;
    public Text KillToDeath;

    [HideInInspector]
    public GameManager.GameAspects.Profile Profile;
    [HideInInspector]
    public GameManager.GameAspects.Team Team;

    private RectTransform _rectTransform;

    public void Awake()
    {
        _rectTransform = this.gameObject.GetComponent<RectTransform>();
    }

    public void Populate(GameManager.GameAspects.Profile profile)
    {
        Profile = profile;

        // Set the color of the stat
        MaskableGraphic playerStatImage = this.gameObject.GetComponent<MaskableGraphic>();
        if (GameManager.Instance.Game.GameType.TeamGame)
            playerStatImage.color = GameManager.Instance.Game.Teams[profile.TeamId].Color;
        else
            playerStatImage.color = GameManager.Instance.PlayerColor;

        // Set the details of the stat
        if (Name != null)
            Name.text = profile.Name;
        if (Score != null)
            Score.text = profile.Score.ToString();
        if (Kills != null)
            Kills.text = profile.Kills.ToString();
        if (Deaths != null)
            Deaths.text = profile.Deaths.ToString();
        if (KillToDeath != null)
            KillToDeath.text = profile.KillDeath > 0 ? "+" + profile.KillDeath : profile.KillDeath.ToString();
    }
    public void Populate(GameManager.GameAspects.Team team)
    {
        Team = team;

        MaskableGraphic teamStatImage = this.gameObject.GetComponent<MaskableGraphic>();
        if (GameManager.Instance.Game.GameType.TeamGame)
            teamStatImage.color = team.Color;
        
        if (Name != null)
            Name.text = team.Name;
        if (Score != null)
            Score.text = team.Score.ToString();
        if (Kills != null)
            Kills.text = team.Kills.ToString();
        if (Deaths != null)
            Deaths.text = team.Deaths.ToString();
        if (KillToDeath != null)
            KillToDeath.text = team.KillDeath > 0 ? "+" + team.KillDeath : team.KillDeath.ToString();
    }

    public void SetPosition(Vector2 position)
    {
        _rectTransform.localScale = Vector3.one;
        _rectTransform.anchoredPosition = position;
    }
}
