using System.Collections;
using System.Collections.Generic;
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

    private readonly List<JoinPlayer> _joinPlayers = new();

    private readonly Dictionary<Team, int> _teamPoints = new()
    {
        { Team.Team1, 0 },
        { Team.Team2, 0 }
    };

    private Team? _advantageTeam;

    private MatchStage _stage = MatchStage.Join;

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

    // Update is called once per frame
    private void Update()
    {
    }

    public void IntializePlayer(PlayerInput plyaerInput )
    {

    }
    private void AddPointsForTeam(Team team, int numPoints)
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

    /// <summary>
    ///     Attempts to score a point for a specific team. Returns false if the team doesn't have advantage.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public bool MakeFinalGoal(Team team)
    {
        if (_advantageTeam != team) return false;

        Debug.Log("Team " + team + " scored 1 point");

        AddPointsForTeam(team, 1);
        return true;
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

        // TODO: Start game
        Debug.Log("Starting game...");
        _stage = MatchStage.Play;
        SceneManager.LoadScene("Scenes/Game/Game");

        yield return null;

        // Spawn Penguins
        uint playerNum = 0;
        foreach (var player in _joinPlayers)
        {
            Debug.Log("Spawning penguin for player " + player);

            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            
            // Find spawn point
            var spawnPoint = SpawnPoint.GetSpawnPointTransformForPlayer(playerNum);

            var penguin =
                Instantiate(penguinPrefab);

            if (spawnPoint)
            {
                penguin.transform.position = spawnPoint.position;
                penguin.transform.rotation = spawnPoint.rotation;
            }

            penguin.GetComponent<Player>().ConnectPlayerInput(playerInput);
            
            playerInput.SwitchCurrentActionMap("Player");

            playerNum++;
        }
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