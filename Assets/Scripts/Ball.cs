using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private MatchManager.Team? _owningTeam;

    public void RespawnBall()
    {
        transform.position = respawnPoint.position;

        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.linearVelocity = Vector3.zero;
    }

    public MatchManager.Team? GetOwningTeam()
    {
        return _owningTeam;
    }

    public void UnclaimBall()
    {
        _owningTeam = null;
    }

    public void ClaimBall(MatchManager.Team team)
    {
        Debug.Log("Team " + team + " has claimed the ball");
        _owningTeam = team;
    }
}