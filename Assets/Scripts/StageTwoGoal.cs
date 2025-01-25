using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StageTwoGoal : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();

        if (!ball) return;

        var ballOwningTeam = ball.GetOwningTeam();

        if (!ballOwningTeam.HasValue) return;

        if (!MatchManager.Instance.MakeFinalGoal(ballOwningTeam.Value)) return;

        ball.UnclaimBall();
        ball.RespawnBall();
    }
}