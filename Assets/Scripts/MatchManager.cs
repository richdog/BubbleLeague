using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public enum Team
    {
        Team1,
        Team2
    }

    public static MatchManager Instance;

    private readonly Dictionary<Team, int> _teamPoints = new()
    {
        { Team.Team1, 0 },
        { Team.Team2, 0 }
    };

    private Team? _advantageTeam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void AddPointsForTeam(Team team, int numPoints)
    {
        Instance._teamPoints[team] += numPoints;
    }

    public int GetPointsForTeam(Team team)
    {
        return Instance._teamPoints[team];
    }

    public bool AnyTeamHasAdvantage()
    {
        return _advantageTeam.HasValue;
    }

    public bool TeamHasAdvantage(Team team)
    {
        return _advantageTeam == team;
    }


    /// <summary>
    ///     Attempts to give a specific team advantage (IE: Move the point from Stage 1 -> Stage 2). Returns false if the game
    ///     is already in stage 2
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public bool GiveTeamAdvantage(Team team)
    {
        if (_advantageTeam.HasValue) return false;

        _advantageTeam = team;

        Debug.Log("Gave advantage to " + _advantageTeam);

        return true;
    }
}