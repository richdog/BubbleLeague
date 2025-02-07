using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    private static string[] ballSounds = { "A", "B", "C" };

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Rigidbody _rigidbody;
    private readonly float _bubbleChargerForce = 10f;
    private bool _inCharger;
    private bool _isUnderwater;
    private float _lastHitTime;
    private float _accumulatedImpulse;

    public static Ball Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void FixedUpdate()
    {
        transform.localScale = GameVars.Ball.ballSize * Vector3.one;
        _rigidbody.mass = GameVars.Ball.ballMass;

        if (_rigidbody != null)
            if (_isUnderwater)
                _rigidbody.AddForce(Physics.gravity * -GameVars.Ball.buoyancy);

        if (_inCharger) _rigidbody.AddForce(Vector3.up * _bubbleChargerForce);

        // handle tons of hits at the same time making lots of sound effects instead of a big one
        if (_accumulatedImpulse > 0 && Time.time - _lastHitTime > 0.05f)
        {
            CameraController.Instance.ShakeCamera(_accumulatedImpulse / 50, 0.2f);

            FMOD.Studio.EventInstance sfxBallHit = RuntimeManager.CreateInstance("event:/sfx_ball_hit");
            sfxBallHit.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            sfxBallHit.setParameterByName("ball_hit_velocity", _accumulatedImpulse);
            sfxBallHit.setParameterByNameWithLabel("water_state", _isUnderwater ? "in_water" : "in_air");
            string randomSound = ballSounds[Random.Range(0, ballSounds.Length)];
            sfxBallHit.setParameterByNameWithLabel("ball_sound", randomSound);
            sfxBallHit.start();
            _accumulatedImpulse = 0;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        _lastHitTime = Time.time;
        _accumulatedImpulse += other.impulse.magnitude;
    }

    private void OnTriggerEnter(Collider other)
    {
        Water water = other.GetComponent<Water>();
        if (water != null)
        {
            _isUnderwater = true;
            _rigidbody.linearDamping = water.WaterDrag * GameVars.Ball.waterDragModifier;

            FMOD.Studio.EventInstance splashSfx = RuntimeManager.CreateInstance("event:/sfx_splash");
            splashSfx.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

            splashSfx.setParameterByName("splash_size", 1);
            splashSfx.setParameterByName("splash_velocity", _rigidbody.linearVelocity.magnitude);
            splashSfx.start();
        }

        if (other.TryGetComponent(out BubbleCharger charger)) _inCharger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Water water = other.GetComponent<Water>();
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
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.linearVelocity = Vector3.zero;
    }
}