using UnityEngine;
using static Player;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Rigidbody _rigidbody;
    private bool _isUnderwater = false;

    public void RespawnBall()
    {
        transform.position = respawnPoint.position;
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.linearVelocity = Vector3.zero;
    }

    public void FixedUpdate()
    {
        transform.localScale = GameVars.Ball.ballSize * Vector3.one;
        _rigidbody.mass = GameVars.Ball.ballMass;
        
        if (_rigidbody != null)
        {
            if (_isUnderwater)
                _rigidbody.AddForce(Physics.gravity * -GameVars.Ball.buoyancy);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            _isUnderwater = true;
            _rigidbody.linearDamping = water.WaterDrag * GameVars.Ball.waterDragModifier;
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