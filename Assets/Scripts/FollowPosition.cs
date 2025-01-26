using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    [SerializeField] private Transform _follow;
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_follow.position.x, _follow.position.y, transform.position.z);
    }
}
