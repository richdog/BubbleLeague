using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.Windows;
using System.Collections;
using UnityEngine.VFX;
using static SkinInit;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public enum PenguinState
    {
        WATER,
        AIR
    }

    public PenguinState State { get; set; } = PenguinState.WATER;

    [SerializeField]
    private ForceMode _forceMode = ForceMode.Impulse;

    [SerializeField]
    private Rigidbody _rigidbody;

    public PlayerInput playerInput;

    [SerializeField]
    public SkinInit skin;

    [SerializeField]
    private VisualEffect boostParticles;

    private Vector2 _prevMovementInput;
    private Vector2 _movementInput;
    private bool _isBraking;
    private PlayerInput _playerInput;
    private bool _isBoosting;
    private bool _isCharging;
    private float _currBoostBubble = 1f;
    private float _boostBubbleCharge = 0.1f;
    private float _bubbleChargerForce = 5f;

    private float _currentBrakeDrag = 0f;

    [SerializeField] private Rigidbody wingLRB;
    [SerializeField] private PlayerWing wingL;
    [SerializeField] private Rigidbody wingRRB;
    [SerializeField] private PlayerWing wingR;

    [SerializeField] private GameObject _bubbleBar;

    private bool isSpinning;
    private float spinStartTime;

    private float stunDuration = 0;
    private bool isStunned => stunDuration > 0;

    public bool isDebug;
    public int playerId;

    void Start()
    {
        wingL.CollisionEnter += OnCollisionEnter;
        wingR.CollisionEnter += OnCollisionEnter;

        boostParticles.Stop();

        if (isDebug)
        {
            ConnectPlayerInput(playerInput);
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            _playerInput.actions["Move"].performed -= Move;
            _playerInput.actions["Move"].canceled -= CancelMove;

            _playerInput.actions["Brake"].performed -= Brake;
            _playerInput.actions["Brake"].canceled -= CancelBrake;

            _playerInput.actions["Boost"].performed -= Boost;
            _playerInput.actions["Boost"].canceled -= CancelBoost;

            _playerInput.actions["RotateL"].performed -= SpinLeft;
            _playerInput.actions["RotateR"].performed -= SpinRight;
        }
    }

    void Update()
    {

    }

    float accumulatedAngle = 0;
    float maxAccumulatedAngle = 45;

    void FixedUpdate()
    {
        float massMultiplier = isSpinning ? GameVars.Player.spinMassMultiplier : _isBraking ? GameVars.Player.brakeMassMultiplier : 1;
        _rigidbody.mass = GameVars.Player.playerBodyMass * massMultiplier;
        wingLRB.mass = GameVars.Player.playerWingMass * massMultiplier;
        wingRRB.mass = GameVars.Player.playerWingMass * massMultiplier;

        var totalMass = _rigidbody.mass + wingLRB.mass + wingRRB.mass;

        if (spinDir != 0)
        {
            if(!isSpinning)
            {
                if (_currBoostBubble > GameVars.Player.spinBubbleBurn)
                {
                    _currBoostBubble -= GameVars.Player.spinBubbleBurn;

                    isSpinning = true;
                    spinStartTime = Time.time;
                    Debug.Log("Spin!");
                    _rigidbody.AddTorque(Vector3.forward * spinDir * GameVars.Player.spinInitialForce * totalMass, ForceMode.Force);
                }
                else
                {
                    spinDir = 0;
                }
            }

            if (isSpinning)
            {
                _rigidbody.AddTorque(Vector3.forward * spinDir * GameVars.Player.spinContinuedForce * totalMass, ForceMode.Force);

                if (spinStartTime + GameVars.Player.spinDuration < Time.time)
                {
                    spinDir = 0;
                    isSpinning = false;
                }
            }
        }

        if(isStunned)
        {
            stunDuration -= Time.fixedDeltaTime;
            if(stunDuration < 0)
                stunDuration = 0;
        }

        if (!isSpinning && stunDuration <= 0)
        {
            float signedAngle = Vector3.SignedAngle(_rigidbody.transform.up, _movementInput, Vector3.forward);
            float sign = Mathf.Sign(signedAngle);
            //accumulatedAngle += signedAngle;

            //accumulatedAngle *= _movementInput.sqrMagnitude;

            //accumulatedAngle = Mathf.Clamp(accumulatedAngle, -maxAccumulatedAngle, maxAccumulatedAngle);

            float dampenFactor = GameVars.Player.rotDampening * totalMass;
            float adjustFactor = GameVars.Player.rotAcceleration;

            _rigidbody.AddTorque(-_rigidbody.angularVelocity * dampenFactor, ForceMode.Force);
            _rigidbody.AddTorque(Vector3.forward * signedAngle * adjustFactor * totalMass, ForceMode.Force);

            skin.SetBodySprite(signedAngle / maxAccumulatedAngle);
        }
        else
        {
            accumulatedAngle = 0;
            skin.SetBodySprite(_rigidbody.angularVelocity.z / 180f);
        }

        if (State == PenguinState.WATER)
        {
            var force =  _movementInput * GameVars.Player.acceleration * totalMass;
            _rigidbody.AddForce(force, ForceMode.Force);
            _rigidbody.AddForce(Physics.gravity * -GameVars.Player.penguinBuoyancy * totalMass, ForceMode.Force);

            if (_isBoosting && _currBoostBubble > 0)
            {
                boostParticles.SetFloat("BubbleAmount", 64);

                _rigidbody.AddForce(GameVars.Player.boostForce * _rigidbody.transform.up * totalMass, ForceMode.Force);
                _currBoostBubble -= GameVars.Player.boostBubbleBurn;
            }
            else if (_boostBubbleCharge <= 0)
            {
                boostParticles.SetFloat("BubbleAmount", 0);
                _isBoosting = false;
            }

            if (_isCharging)
            {
                _currBoostBubble += _boostBubbleCharge;
                _rigidbody.AddForce(Vector3.up * _bubbleChargerForce);
            }
        }

        _currBoostBubble += Time.fixedDeltaTime * (State == PenguinState.WATER ? GameVars.Player.waterBubbleGainSpeed : State == PenguinState.AIR ? GameVars.Player.airBubbleGainSpeed : 0);

        _currBoostBubble = math.clamp(_currBoostBubble, 0, 1);
        var drag = CalcDrag();
        _rigidbody.linearDamping = drag;
        wingLRB.linearDamping = drag;
        wingRRB.linearDamping = drag;

        _bubbleBar.transform.localScale = new Vector3(math.lerp(_bubbleBar.transform.localScale.x, _currBoostBubble, 0.5f), _bubbleBar.transform.localScale.y, _bubbleBar.transform.localScale.z);
        HandleWings();

        _prevMovementInput = _movementInput;
    }

    public void ConnectPlayerInput(PlayerInput input)
    {
        playerId = input.playerIndex +1;
        _playerInput = input;
        _playerInput.actions["Move"].performed += Move;
        _playerInput.actions["Move"].canceled += CancelMove;

        _playerInput.actions["Brake"].performed += Brake;
        _playerInput.actions["Brake"].canceled += CancelBrake;

        _playerInput.actions["Boost"].performed += Boost;
        _playerInput.actions["Boost"].canceled += CancelBoost;

        _playerInput.actions["RotateL"].performed += SpinLeft;
        _playerInput.actions["RotateR"].performed += SpinRight;
    }

    private void Move(InputAction.CallbackContext ctx)
    {
        if (isStunned)
        {
            _prevMovementInput = Vector3.zero;
            _movementInput = Vector3.zero;
            return;
        }

        if(!isSpinning)
            _movementInput = ctx.ReadValue<Vector2>();
    }

    private void CancelMove(InputAction.CallbackContext ctx)
    {
        _prevMovementInput = Vector3.zero;
        _movementInput = Vector2.zero;
    }

    private void Brake(InputAction.CallbackContext ctx)
    {
        if(!isStunned)
            _isBraking = true;
    }

    private void CancelBrake(InputAction.CallbackContext ctx)
    {
        _isBraking = false;
    }

    private void Boost(InputAction.CallbackContext ctx)
    {
        if(!isStunned)
        {
            _isBoosting = true;

            boostParticles.Play();
        }
    }
    

    private void CancelBoost(InputAction.CallbackContext ctx)
    {
        _isBoosting = false;
        boostParticles.Stop();
    }

    private float spinDir;
    private void SpinLeft(InputAction.CallbackContext ctx)
    {
        if(!isStunned)
            spinDir = 1;

    }

    private void SpinRight(InputAction.CallbackContext ctx)
    {
        if(!isStunned)
            spinDir = -1;
    }


    void HandleWings()
    {
        var torque = _isBraking || isSpinning ? -GameVars.Player.wingOpenTorqueAmt : GameVars.Player.wingCloseTorqueAmt;

        wingLRB.AddRelativeTorque(0, 0, torque, ForceMode.Acceleration);
        wingRRB.AddRelativeTorque(0, 0, -torque, ForceMode.Acceleration);
    }

    float CalcDrag()
    {
        float drag = 0;

        if (State == PenguinState.WATER)
            drag += GameVars.Player.waterDrag;
        else if (State == PenguinState.AIR)
            drag += GameVars.Player.airDrag;

        var targetBrakeDrag = _isBraking ? GameVars.Player.brakeDrag : 0;
        _currentBrakeDrag = Mathf.Lerp(_currentBrakeDrag, targetBrakeDrag, GameVars.Player.brakeDragLerpSpeed);

        drag *= 1 + _currentBrakeDrag;
        return drag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Water water))
        {
            State = PenguinState.WATER;
        }

        if (other.TryGetComponent(out BubbleCharger charger))
        {
            _isCharging = true;
            _boostBubbleCharge = charger.ChargeRate;
            
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            State = PenguinState.AIR;
        }

        var charger = other.GetComponent<BubbleCharger>();
        if (charger != null)
        {
            _isCharging = false;
            _boostBubbleCharge = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isSpinning)
        {
            var hitForce = collision.impulse.normalized * _rigidbody.angularVelocity.magnitude * GameVars.Player.playerHitForce;

            //apply some backwards force to self
            //if (!_isBraking)
            //    _rigidbody.AddForceAtPosition(hitForce / 2, collision.GetContact(0).point, ForceMode.Impulse);

            var otherRigidbody = collision.gameObject.GetComponentInParent<Rigidbody>();
            if (otherRigidbody != null)
            {
                var otherPlayer = collision.gameObject.GetComponentInParent<Player>();
                Debug.Log("Impact! " + hitForce);
                if (otherPlayer == null || !otherPlayer._isBraking)
                {
                    //either it's a player (that isn't braking) or another rigidbody, apply force
                    otherRigidbody.AddForceAtPosition(hitForce, collision.GetContact(0).point, ForceMode.Impulse);
                    //if it's actually a player, stun them for a short duration
                    if(otherPlayer != null)
                        otherPlayer.Stun(GameVars.Player.stunDuration);
                }
                if(otherPlayer != null && otherPlayer._isBraking)
                {
                    //if the other player is braking, stun self, acts as a counter
                    Stun(GameVars.Player.stunDuration);
                }
            }
        }
    }

    public void Stun(float duration)
    {
        Debug.Log($"Stunned {name} for {duration} seconds!");
        stunDuration = Mathf.Max(duration, stunDuration);

        _isBraking = false;
        _isBoosting = false;
        isSpinning = false;
    }
}