using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Goal : MonoBehaviour
{
    [SerializeField] public MatchManager.Team team;

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();

        if (!ball) return;

        MatchManager.Instance.MakeGoal(team, ball);
    }
}