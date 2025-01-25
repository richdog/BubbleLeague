using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StageOneGoal : MonoBehaviour
{
    [SerializeField] private MatchManager.Team goalTeam = MatchManager.Team.Team1;

    [SerializeField] private Transform pipeExit;

    #region Parameters

    [SerializeField] private float exitSpeed;

    #endregion

    private BoxCollider _boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();
        var ballRigidBody = collision.rigidbody;

        if (!ball) return;

        if (!MatchManager.Instance.GiveTeamAdvantage(goalTeam)) return;

        var newVelocity = pipeExit.right * exitSpeed;

        ballRigidBody.linearVelocity = newVelocity;
        ballRigidBody.transform.position = pipeExit.transform.position;
    }
}