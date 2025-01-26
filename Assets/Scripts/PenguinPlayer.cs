using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.Windows;
using System.Collections;

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

    private Vector2 _movementInput;
    private Vector2 _prevMovementInput;
    private bool _isBraking;
    private PlayerInput _playerInput;
    private bool _isBoosting;
    private bool _isCharging;
    private float _currBoostBubble = 1f;
    private float _boostBubbleCharge = 0.1f;

    private float _currentBrakeDrag = 0f;

    [SerializeField] private Rigidbody wingL;
    [SerializeField] private Rigidbody wingR;

    [SerializeField] private GameObject _bubbleBar;

    private bool isSpinning;
    private float spinStartTime;

    public bool isDebug;
    public int playerId;

    void Start()
    {
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

    void FixedUpdate()
    {
        _rigidbody.mass = GameVars.Player.playerBodyMass;
        wingL.mass = GameVars.Player.playerWingMass;
        wingR.mass = GameVars.Player.playerWingMass;
        
        var totalMass = _rigidbody.mass + wingL.mass + wingR.mass;

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

        if (!isSpinning)
        {
            Quaternion lookDir = Quaternion.FromToRotation(_rigidbody.transform.up, _movementInput);
            lookDir.ToAngleAxis(out float angle, out Vector3 axis);
            float dampenFactor = GameVars.Player.rotDampening * totalMass;
            _rigidbody.AddTorque(-_rigidbody.angularVelocity * dampenFactor, ForceMode.Force);
            float adjustFactor = GameVars.Player.rotAcceleration;
            _rigidbody.AddTorque(axis.normalized * angle * adjustFactor * totalMass, ForceMode.Force);
        }        

        if (State == PenguinState.WATER)
        {
            var force =  _movementInput * GameVars.Player.acceleration * totalMass;
            _rigidbody.AddForce(force, ForceMode.Force);
            _rigidbody.AddForce(Physics.gravity * -GameVars.Player.penguinBuoyancy * totalMass, ForceMode.Force);

            if (_isBoosting && _currBoostBubble > 0)
            {
                _rigidbody.AddForce(GameVars.Player.boostForce * _movementInput * totalMass, ForceMode.Force);
                _currBoostBubble -= GameVars.Player.boostBubbleBurn;
            }
            else if (_boostBubbleCharge <= 0)
            {
                _isBoosting = false;
            }

            if (_isCharging)
            {
                _currBoostBubble += _boostBubbleCharge;
            }
        }

        _currBoostBubble = math.clamp(_currBoostBubble, 0, 1);
        var drag = CalcDrag();
        _rigidbody.linearDamping = drag;
        wingL.linearDamping = drag;
        wingR.linearDamping = drag;

        _bubbleBar.transform.localScale = new Vector3(math.lerp(_bubbleBar.transform.localScale.x, _currBoostBubble, 0.5f), _bubbleBar.transform.localScale.y, _bubbleBar.transform.localScale.z);
        HandleWings();
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
        _prevMovementInput = _movementInput;
        _movementInput = ctx.ReadValue<Vector2>();
    }

    private void CancelMove(InputAction.CallbackContext ctx)
    {
        _movementInput = Vector2.zero;
    }

    private void Brake(InputAction.CallbackContext ctx)
    {
        _isBraking = true;
    }

    private void CancelBrake(InputAction.CallbackContext ctx)
    {
        _isBraking = false;
    }

    private void Boost(InputAction.CallbackContext ctx)
    {
        _isBoosting = true;
    }

    private void SpinLeft(InputAction.CallbackContext ctx)
    {
        spinDir = 1;
    }

    private void SpinRight(InputAction.CallbackContext ctx)
    {
        spinDir = -1;
    }

    private float spinDir;

    private void CancelBoost(InputAction.CallbackContext ctx)
    {
        _isBoosting = false;
    }

    void HandleWings()
    {
        var torque = _isBraking || isSpinning ? -GameVars.Player.wingOpenTorqueAmt : GameVars.Player.wingCloseTorqueAmt;

        wingL.AddRelativeTorque(0, 0, torque, ForceMode.Acceleration);
        wingR.AddRelativeTorque(0, 0, -torque, ForceMode.Acceleration);
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
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            State = PenguinState.WATER;
        }

        var charger = other.GetComponent<BubbleCharger>();
        if (charger != null)
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
        var otherPlayer = collision.gameObject.GetComponentInParent<Player>();
        if (otherPlayer != null)
        {
            var magnitudeDifference = _rigidbody.linearVelocity.magnitude - otherPlayer._rigidbody.linearVelocity.magnitude;

            if (magnitudeDifference > 0)
            {
                var hitForce = collision.impulse.normalized * Mathf.Sqrt(magnitudeDifference) * GameVars.Player.playerHitForce;

                Debug.Log("Impact! " + hitForce);
                if(!otherPlayer._isBraking)
                {
                    otherPlayer._rigidbody.AddForce(-hitForce/*, collision.GetContact(0).point*/, ForceMode.Impulse);
                }
                
                if(!_isBraking)
                    _rigidbody.AddForce(hitForce/*, collision.GetContact(0).point*/, ForceMode.Impulse);
            }
        }
    }
}