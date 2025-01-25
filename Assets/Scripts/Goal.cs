using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Goal : MonoBehaviour
{
    [SerializeField] public MatchManager.Team team;

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();

        if (!ball) return;

        if (!MatchManager.Instance.MakeGoal(team)) return;

        ball.RespawnBall();
    }
}