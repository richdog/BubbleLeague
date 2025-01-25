using UnityEngine;
using static Player;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField]
    private Rigidbody _rigidbody;

    private MatchManager.Team? _owningTeam;

    [SerializeField, Range(0, 2)]
    private float waterDragModifier = 0.5f;

    private bool _isUnderwater = false;

    [SerializeField, Range(0, 2)] private float _buoyancy = 1;

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

    public void FixedUpdate()
    {
        if (_rigidbody != null)
        {
            if(_isUnderwater)
                _rigidbody.AddForce(Physics.gravity * -_buoyancy);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            _isUnderwater = true;
            _rigidbody.linearDamping = water.WaterDrag * waterDragModifier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            _isUnderwater = false;
            _rigidbody.linearDamping = 0;
        }
    }
}