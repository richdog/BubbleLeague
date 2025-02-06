using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Rigidbody _rigidbody;
    private readonly float _bubbleChargerForce = 10f;
    private bool _inCharger;
    private bool _isUnderwater;

    public void FixedUpdate()
    {
        transform.localScale = GameVars.Ball.ballSize * Vector3.one;
        _rigidbody.mass = GameVars.Ball.ballMass;

        if (_rigidbody != null)
            if (_isUnderwater)
                _rigidbody.AddForce(Physics.gravity * -GameVars.Ball.buoyancy);

        if (_inCharger) _rigidbody.AddForce(Vector3.up * _bubbleChargerForce);
    }

    private void OnCollisionEnter(Collision other)
    {
        var sfxBallHit = RuntimeManager.CreateInstance("event:/sfx_ball_hit");
        sfxBallHit.setParameterByName("ball_hit_velocity", other.impulse.magnitude);
        sfxBallHit.setParameterByNameWithLabel("water_state", _isUnderwater ? "in_water" : "in_air");
        sfxBallHit.start();
    }

    private void OnTriggerEnter(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            _isUnderwater = true;
            _rigidbody.linearDamping = water.WaterDrag * GameVars.Ball.waterDragModifier;

            var splashSfx = RuntimeManager.CreateInstance("event:/sfx_splash");

            splashSfx.setParameterByName("splash_size", 1);
            splashSfx.setParameterByName("splash_velocity", _rigidbody.linearVelocity.magnitude);
            splashSfx.start();
        }

        if (other.TryGetComponent(out BubbleCharger charger)) _inCharger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            _isUnderwater = false;
            _rigidbody.linearDamping = 0;
        }

        if (other.TryGetComponent(out BubbleCharger charger)) _inCharger = false;
    }

    public void RespawnBall()
    {
        transform.position = respawnPoint.position;
        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.linearVelocity = Vector3.zero;
    }
}