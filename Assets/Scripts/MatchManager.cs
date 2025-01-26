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


    public bool PlayerExists(int playerIndex)
    {
        if (playerIndex < _joinPlayers.Count) return true;

        return false;
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
        SoundManager.Instance.SwitchMusic("event:/main_theme_end");
        SoundManager.Instance.StopAllAmbiences();

        yield return new WaitForSeconds(2.0f);

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

        _joinPlayers.Add(joinPlayer);

        OnPlayerJoinChange();

        Debug.Log("Registered player " + joinPlayer + " for match");
        return true;
    }

    public void UnregisterPlayer(JoinPlayer joinPlayer)
    {
        if (_stage != MatchStage.Join)
        {
            Debug.LogWarning("Failed to unregister player, stage is not \"Join\"");
            return;
        }

        _joinPlayers.Remove(joinPlayer);

        OnPlayerJoinChange?.Invoke();
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
            penguinPlayer.playerId = i;
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