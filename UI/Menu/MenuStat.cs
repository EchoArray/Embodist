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

    public GameManager.GameAspects.Profile Profile;
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
        Image playerStatImage = this.gameObject.GetComponent<Image>();
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

        Image playerStatImage = this.gameObject.GetComponent<Image>();
        if (GameManager.Instance.Game.GameType.TeamGame)
            playerStatImage.color = team.Color;

        Name.text = team.Name;
        Score.text = team.Score.ToString();
    }

    public void SetPosition(Vector2 position)
    {
        _rectTransform.localScale = Vector3.one;
        _rectTransform.anchoredPosition = position;
    }
}
