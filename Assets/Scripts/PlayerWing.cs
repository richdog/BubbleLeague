using System;
using UnityEngine;

public class PlayerWing : MonoBehaviour
{
    public Action<Collision> CollisionEnter;

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision);
    }
}
