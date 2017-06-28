using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    #region Values
    /// <summary>
    /// Defines the prefab for the stat object.
    /// </summary>
    public GameObject PlayerStatPrefab;
    public GameObject TeamStatPrefab;
    /// <summary>
    /// Defines the margin between each stat.
    /// </summary>
    public float StatMargin;
    /// <summary>
    /// Defines the offset in-which elements will start at.
    /// </summary>
    public float StartingOffset;

    /// <summary>
    /// Determines if the container will be scaled to the space of its elements.
    /// </summary>
    public bool ScaleContainer = true;
    // A collection of stats
    private List<MenuStat> _stats = new List<MenuStat>();
    // The rect transform of this object
    private RectTransform _rectTransform;
    // The profile count of the previous frame.
    private int _previousFramerofileCount = 0; 
    #endregion

    #region Unity Functions
    public void Awake()
    {
        _previousFramerofileCount = GameManager.Instance.Game.Profiles.Count;
        _rectTransform = this.gameObject.GetComponent<RectTransform>();
        Build();
    }

    public void LateUpdate()
    {
        if (_previousFramerofileCount != GameManager.Instance.Game.Profiles.Count)
            Rebuild();
        _previousFramerofileCount = GameManager.Instance.Game.Profiles.Count;

        Populate();
    }
    #endregion

    #region Functions
    public void Build()
    {
        // Create all stat objects

        int[] teams = GameManager.Instance.GetScoreSortedTeamIndexes(GameManager.Instance.Game.Teams);

        // Create all objects and stats
        for (int i = 0; i < teams.Length; i++)
        {
            // Find the profiles of the team
            List<GameManager.GameAspects.Profile> profiles = GameManager.Instance.GetTeamProfiles(teams[i]);
            // If there are no profiles for the team abort
            if (profiles.Count == 0)
                continue;

            // Create team header
            if (GameManager.Instance.Game.GameType.TeamGame)
            {
                // Create game object
                GameObject teamStatGameObject = Instantiate(TeamStatPrefab, this.transform);
                MenuStat teamStat = teamStatGameObject.GetComponent<MenuStat>();
                // Add stat to local list
                _stats.Add(teamStat);
                // Populate
                teamStat.Populate(GameManager.Instance.Game.Teams[teams[i]]);
            }

            foreach (GameManager.GameAspects.Profile profile in profiles)
            {
                // Create game object
                GameObject playerStatGameObject = Instantiate(PlayerStatPrefab, this.transform);
                MenuStat playerStat = playerStatGameObject.GetComponent<MenuStat>();
                // Add stat to local list
                _stats.Add(playerStat);
                // Populate
                playerStat.Populate(profile);
            }
        }
        Populate();
    }
    public void Rebuild()
    {
        // Clear all existing stat objects and build
        foreach (MenuStat stat in _stats)
            Destroy(stat.gameObject);

        _stats.Clear();

        Build();
    }
    public void Populate()
    {
        // Reposition and repopulate the details of all stats

        float teamStatHeight = TeamStatPrefab.GetComponent<RectTransform>().sizeDelta.y + StatMargin;
        float playerStatHeight = PlayerStatPrefab.GetComponent<RectTransform>().sizeDelta.y + StatMargin;
        float offset = StartingOffset;

        if (GameManager.Instance.Game.GameType.TeamGame)
        {
            int[] teams = GameManager.Instance.GetScoreSortedTeamIndexes(GameManager.Instance.Game.Teams);

            for (int i = 0; i < teams.Length; i++)
            {
                // Find all teams stats
                foreach (MenuStat teamStat in _stats)
                {
                    if (teamStat.Team == GameManager.Instance.Game.Teams[teams[i]])
                    {

                        List<GameManager.GameAspects.Profile> profiles = GameManager.Instance.GetTeamProfiles(teams[i]);
                        // If there are no profiles for the team abort
                        if (profiles.Count == 0)
                            continue;

                        // Sort profiles by score
                        profiles = GameManager.Instance.GetScoreSortedProfiles(profiles);

                        teamStat.SetPosition(new Vector2(0, offset));
                        teamStat.Populate(teamStat.Team);
                        offset -= teamStatHeight;

                        foreach (GameManager.GameAspects.Profile profile in profiles)
                        {
                            foreach (MenuStat playerStat in _stats)
                            {
                                if (playerStat.Profile == profile)
                                {
                                    // Populate profile and reposition
                                    playerStat.SetPosition(new Vector2(0, offset));
                                    playerStat.Populate(profile);
                                    offset -= playerStatHeight;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // Create all profiles
            List<GameManager.GameAspects.Profile> profiles = GameManager.Instance.GetScoreSortedProfiles(GameManager.Instance.Game.Profiles);
            foreach (GameManager.GameAspects.Profile profile in profiles)
            {
                foreach (MenuStat playerStat in _stats)
                {
                    if (playerStat.Profile == profile)
                    {
                        // Populate profile and reposition
                        playerStat.SetPosition(new Vector2(0, offset));
                        playerStat.Populate(profile);
                        offset -= playerStatHeight + StatMargin;
                    }
                }
            }
        }
        if(ScaleContainer)
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, Mathf.Abs(offset));
    }

    public void SetActive(bool state)
    {
        if (this == null)
            return;

        this.gameObject.SetActive(state);
    } 
    #endregion
}
