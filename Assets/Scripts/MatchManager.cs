using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Sound;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public enum MatchStage
    {
        Join,
        Play,
        Victory
    }

    public enum Team
    {
        Team1,
        Team2
    }

    [SerializeField] private bool onlyFirstPlayerCanStart;

    public GameObject penguinPrefab;
    public GameObject ballPrefab;

    private readonly List<JoinPlayer> _joinPlayers = new();


    private readonly List<GameObject> _players = new();

    private readonly Dictionary<Team, uint> _teamPoints = new()
    {
        { Team.Team1, 0 },
        { Team.Team2, 0 }
    };

    private GameObject _ball;

    private bool _inOvertime;

    private MatchStage _stage = MatchStage.Join;

    private JoinPlayer joinSpot1Team1;
    private JoinPlayer joinSpot1Team2;
    private JoinPlayer joinSpot2Team1;
    private JoinPlayer joinSpot2Team2;
    public Action OnGoalScored;

    public Action OnPlayerJoinChange;

    public static MatchManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }


    public string GetDescriptionForPlayer(int playerMenuIndex)
    {
        switch (playerMenuIndex)
        {
            case 0:
                if (joinSpot1Team1) return "Player " + (joinSpot1Team1.joinOrder + 1);

                return "Empty";

                break;

            case 1:
                if (joinSpot1Team2) return "Player " + (joinSpot1Team2.joinOrder + 1);

                return "Empty";

                break;

            case 2:
                if (joinSpot2Team1) return "Player " + (joinSpot2Team1.joinOrder + 1);

                return "Empty";

                break;

            case 3:
                if (joinSpot2Team2) return "Player " + (joinSpot2Team2.joinOrder + 1);

                return "Empty";

                break;
        }

        return "ERROR";
    }

    public int GetPlayerInputJoinIndex(PlayerInput playerInput)
    {
        for (var i = 0; i < _joinPlayers.Count; i++)
            if (_joinPlayers[i].GetComponent<PlayerInput>() == playerInput)
                return i;

        return -1;
    }

    private void AddPointsForTeam(Team team, uint numPoints)
    {
        Instance._teamPoints[team] += numPoints;
    }

    public uint GetPointsForTeam(Team team)
    {
        return Instance._teamPoints[team];
    }

    /// <summary>
    ///     Makes a goal for a specific team from a specific ball, respawning players
    /// </summary>
    /// <param name="team"></param>
    /// <param name="ball"></param>
    /// <returns></returns>
    public void MakeGoal(Team team, Ball ball)
    {
        if (_inOvertime)
        {
            TeamWin(team);
            StartCoroutine(GoalFX());
            Destroy(ball.gameObject);
            return;
        }

        Debug.Log("Team " + team + " scored 1 point");

        AddPointsForTeam(team, 1);
        ScoreboardManager.Instance.SetScore(_teamPoints[Team.Team1], _teamPoints[Team.Team2]);

        if (_teamPoints[Team.Team1] >= GameVars.General.pointsNeededToWin)
        {
            TeamWin(Team.Team1);
            StartCoroutine(GoalFX());
            Destroy(ball.gameObject);
            return;
        }

        if (_teamPoints[Team.Team2] >= GameVars.General.pointsNeededToWin)
        {
            TeamWin(Team.Team2);
            StartCoroutine(GoalFX());
            Destroy(ball.gameObject);
            return;
        }

        StartCoroutine(GoalFX());
        Destroy(ball.gameObject);


        StartCoroutine(SetNewPoint());
    }

    private IEnumerator GoalFX()
    {
        RuntimeManager.PlayOneShot("event:/make_goal");

        OnGoalScored?.Invoke();

        yield return null;
    }

    private IEnumerator OnTeamWin()
    {
        SoundManager.Instance.StopAllAmbiences();

        yield return new WaitForSeconds(0.5f);

        SoundManager.Instance.SwitchMusic("event:/main_theme_end");

        yield return new WaitForSeconds(11.0f);

        //SceneManager.LoadScene("Scenes/Game/VictoryMenu");

        SceneManager.LoadScene("Scenes/Game/MainMenu");
        DestroyOnRestart.DestroyAll();

        yield return null;
    }

    private void TeamWin(Team team)
    {
        ScoreboardManager.Instance.StopTimer();

        StartCoroutine(OnTeamWin());
    }

    private IEnumerator SetNewPoint()
    {
        yield return new WaitForSeconds(2.0f);

        RespawnPlayers();
        SpawnNewBall();
    }

    private void RespawnPlayers()
    {
        // Respawn all players
        uint playerNum = 0;
        foreach (var player in _players)
        {
            Debug.Log("Respawning " + player);

            // Find spawn point
            var spawnPoint = SpawnPoint.GetSpawnPointTransformForPlayer(playerNum);

            if (spawnPoint)
            {
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
            }

            playerNum++;
        }
    }

    private void SpawnNewBall()
    {
        var ballSpawnPointTransform = FindFirstObjectByType<BallSpawnPoint>().transform;

        var newBall = Instantiate(ballPrefab);
        newBall.transform.position = ballSpawnPointTransform.position;
        newBall.transform.rotation = ballSpawnPointTransform.rotation;
    }

    /// <summary>
    ///     Attempts to register a menu player into the match
    /// </summary>
    /// <param name="joinPlayer"></param>
    /// <returns>False if the match is full, True if the player is registered</returns>
    public bool RegisterPlayer(JoinPlayer joinPlayer)
    {
        if (_stage != MatchStage.Join)
        {
            Debug.LogWarning("Failed to register player, stage is not \"Join\"");
            return false;
        }

        if (_joinPlayers.Count >= 4) return false;

        if (_joinPlayers.Contains(joinPlayer))
        {
            Debug.LogWarning("Player " + joinPlayer + " already registered!");
            return true;
        }

        assignJoinOrder(joinPlayer);

        assignTeamPlace(joinPlayer);

        _joinPlayers.Add(joinPlayer);

        OnPlayerJoinChange?.Invoke();

        Debug.Log("Registered player " + joinPlayer + " for match");
        return true;
    }

    private void assignTeamPlace(JoinPlayer joinPlayer)
    {
        if (!joinSpot1Team1)
        {
            joinSpot1Team1 = joinPlayer;
            Debug.Log("joinSpot1Team1 went to " + joinPlayer + "with order " + joinPlayer.joinOrder);
            return;
        }

        if (!joinSpot1Team2)
        {
            joinSpot1Team2 = joinPlayer;
            Debug.Log("joinSpot1Team2 went to " + joinPlayer + "with order " + joinPlayer.joinOrder);
            return;
        }

        if (!joinSpot2Team1)
        {
            joinSpot2Team1 = joinPlayer;
            Debug.Log("joinSpot2Team1 went to " + joinPlayer + "with order " + joinPlayer.joinOrder);
            return;
        }

        if (!joinSpot2Team2)
        {
            joinSpot2Team2 = joinPlayer;
            Debug.Log("joinSpot2Team2 went to " + joinPlayer + "with order " + joinPlayer.joinOrder);
            return;
        }

        throw new Exception("All join spots taken");
    }

    private int getPlayerTeamPlaceID(JoinPlayer joinPlayer)
    {
        if (joinPlayer == joinSpot1Team1) return 0;

        if (joinPlayer == joinSpot1Team2) return 2;

        if (joinPlayer == joinSpot2Team1) return 1;

        if (joinPlayer == joinSpot2Team2) return 3;

        throw new Exception("JoinPlayer not apart of a team");
    }

    private void assignJoinOrder(JoinPlayer joinPlayer)
    {
        for (uint i = 0; i < 4; i++)
            if (!joinOrderTaken(i))
            {
                joinPlayer.joinOrder = i;
                return;
            }


        throw new Exception("Failed to assign join order");
    }

    private bool joinOrderTaken(uint order)
    {
        foreach (var player in _joinPlayers)
            if (player.joinOrder == order)
                return true;

        return false;
    }

    public void UnregisterPlayer(JoinPlayer joinPlayer)
    {
        if (_stage != MatchStage.Join)
        {
            Debug.LogWarning("Failed to unregister player, stage is not \"Join\"");
            return;
        }

        unassignTeamPlace(joinPlayer);

        _joinPlayers.Remove(joinPlayer);

        OnPlayerJoinChange?.Invoke();
    }

    private void unassignTeamPlace(JoinPlayer joinPlayer)
    {
        if (joinSpot1Team1 == joinPlayer)
        {
            joinSpot1Team1 = null;
            Debug.Log("joinSpot1Team1 was freed");
            return;
        }

        if (joinSpot1Team2 == joinPlayer)
        {
            joinSpot1Team2 = null;
            Debug.Log("joinSpot1Team2 was freed");
            return;
        }

        if (joinSpot2Team1 == joinPlayer)
        {
            joinSpot2Team1 = null;
            Debug.Log("joinSpot2Team1 was freed");
            return;
        }

        if (joinSpot2Team2 == joinPlayer)
        {
            joinSpot2Team2 = null;
            Debug.Log("joinSpot2Team2 was freed");
        }
    }

    private IEnumerator StartGameCoroutine(JoinPlayer joinPlayer)
    {
        if (_stage != MatchStage.Join)
        {
            Debug.Log("Failed to start game, MatchStage != Join");
            yield break;
        }

        if (!_joinPlayers.Contains(joinPlayer))
        {
            Debug.Log("Failed to start game, requested from player not joined");
            yield break;
        }

        if (onlyFirstPlayerCanStart && _joinPlayers[0] != joinPlayer)
        {
            Debug.Log("Failed to start game, requester not player one");
            yield break;
        }

        if (_joinPlayers.Count < 2)
        {
            Debug.Log("Failed to start game, too few players");
            yield break;
        }

        // Make sure one per team
        uint team1NumPlayers = 0;
        uint team2NumPlayers = 0;
        if (joinSpot1Team1) team1NumPlayers++;
        if (joinSpot2Team1) team1NumPlayers++;
        if (joinSpot1Team2) team2NumPlayers++;
        if (joinSpot2Team2) team2NumPlayers++;

        if (team1NumPlayers < 1 || team2NumPlayers < 1) yield break;

        Debug.Log("Starting game...");
        _stage = MatchStage.Play;
        SceneManager.LoadScene("Scenes/Game/Game");

        yield return null;

        // Spawn Penguins
        for (var i = 0; i < _joinPlayers.Count; i++)
        {
            Debug.Log("Spawning penguin for player " + _joinPlayers[i]);

            var playerInput = _joinPlayers[i].GetComponent<PlayerInput>();

            var penguinGameObject =
                Instantiate(penguinPrefab);

            _players.Add(penguinGameObject);

            var penguinPlayer = penguinGameObject.GetComponent<Player>();
            penguinPlayer.playerId = getPlayerTeamPlaceID(_joinPlayers[i]);
            penguinPlayer.ConnectPlayerInput(playerInput);

            playerInput.SwitchCurrentActionMap("Player");
        }

        RespawnPlayers();
        SpawnNewBall();

        SoundManager.Instance.SwitchMusic("event:/main_theme_start");
        SoundManager.Instance.AddAmbience("event:/wah_ambience");

        ScoreboardManager.Instance.StartTimer();
    }

    /// <summary>
    ///     Tries to end the match by time expiring, returns false if going into overtime
    /// </summary>
    /// <returns></returns>
    public bool TryWinByTime()
    {
        var team1Points = _teamPoints[Team.Team1];
        var team2Points = _teamPoints[Team.Team2];

        if (team1Points > team2Points)
        {
            TeamWin(Team.Team1);
            return true;
        }

        if (team2Points > team1Points)
        {
            TeamWin(Team.Team2);
            return true;
        }

        _inOvertime = true;
        return false;
    }

    /// <summary>
    ///     Attempts to start the game with the given player.
    ///     Fails if:
    ///     - That player isn't in the game
    ///     - onlyFirstPlayerCanStart is true and the player requesting isn't player one
    ///     - The stage isn't "Join"
    /// </summary>
    /// <param name="joinPlayer"></param>
    public void TryStartGame(JoinPlayer joinPlayer)
    {
        StartCoroutine(StartGameCoroutine(joinPlayer));
    }
}