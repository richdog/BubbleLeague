using System;
using UnityEngine;

public class PlayerRigidbody : MonoBehaviour
{
    public Action<Collision> CollisionEnter;
    public Action<Collider> TriggerEnter;
    public Action<Collider> TriggerExit;

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit?.Invoke(other);
    }
}