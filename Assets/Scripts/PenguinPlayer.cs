using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

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
    
    [SerializeField, Range(1, 30)] private float _acceleration = 25;
    [SerializeField] private float _brakeDrag = 20;
    private float _waterDrag = 0;
    
    private Vector2 _movementInput;
    private Vector2 _prevMovementInput;
    private bool _isBraking;
    private PlayerInput _playerInput;
    private bool _isBoosting;
    private bool _isCharging;
    private float _currBoostBubble = 1f;
    private float _boostBubbleCharge = 0.1f;
    

    [SerializeField, Range(0, 2)] private float _buoyancy = 1;

    [SerializeField, Range(0, 10)] private float _rotDampening = 2f;
    [SerializeField, Range(0, 10)] private float _rotAcceleration = 0.2f;

    [SerializeField, Range(0, 1)] private float _brakeWingPivotHeight = 0.3f;

    [SerializeField] private Rigidbody wingL;
    [SerializeField] private Rigidbody wingR;

    [SerializeField] private float wingOpenTorqueAmt = 300f;
    [SerializeField] private float wingCloseTorqueAmt = 500f;

    
    [SerializeField, Range(0,1)] private float _boostBubbleBurn = 0.1f;
    [SerializeField] private float _boostForce = 50;

    [SerializeField] private GameObject _bubbleBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /* void Start()
     {
         //playerInput = GetComponent<PlayerInput>();        

         playerInput.actions["Move"].performed += ctx =>
         {
             _prevMovementInput = _movementInput;
             _movementInput = ctx.ReadValue<Vector2>();
         };
         playerInput.actions["Move"].canceled += ctx => _movementInput = Vector2.zero;

         playerInput.actions["Brake"].performed += ctx =>
         {
             _isBraking = true;
         };
         playerInput.actions["Brake"].canceled += ctx =>
         {
             _isBraking = false;
         };
     }*/
    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= ctx =>
        {
            _prevMovementInput = _movementInput;
            _movementInput = ctx.ReadValue<Vector2>();
        };
        playerInput.actions["Move"].canceled -= ctx => _movementInput = Vector2.zero;

        playerInput.actions["Brake"].performed -= ctx =>
        {
            _isBraking = true;
        };
        playerInput.actions["Brake"].canceled -= ctx =>
        {
            _isBraking = false;
        };

        _playerInput.actions["Boost"].performed += ctx => { _isBoosting = true; };
        _playerInput.actions["Boost"].canceled += ctx => { _isBoosting = false; };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //physics update
    void FixedUpdate()
    {
        Quaternion lookDir = Quaternion.FromToRotation(_rigidbody.transform.up, _movementInput);
        lookDir.ToAngleAxis(out float angle, out Vector3 axis);
        float dampenFactor = _rotDampening;
        _rigidbody.AddTorque(-_rigidbody.angularVelocity * dampenFactor, ForceMode.Acceleration);
        float adjustFactor = _rotAcceleration;
        _rigidbody.AddTorque(axis.normalized * angle * adjustFactor, ForceMode.Acceleration);

        Debug.Log($"Axis: {axis}, Angle: {angle}");


        if (State == PenguinState.WATER)
        {
            var force =_movementInput * _acceleration;
            _rigidbody.AddForce(force, _forceMode);
            _rigidbody.AddForce(Physics.gravity * -_buoyancy);

            if (_isBoosting && _currBoostBubble > 0)
            {
                _rigidbody.AddForce(_boostForce * _movementInput, ForceMode.Impulse);
                _currBoostBubble -= _boostBubbleBurn;
            } else if (_boostBubbleCharge <= 0)
            {
                _isBoosting = false;
            }

            if (_isCharging)
            {
                _currBoostBubble += _boostBubbleCharge;
            }
        }

        _currBoostBubble = math.clamp(_currBoostBubble, 0, 1);
        _rigidbody.linearDamping = CalcDrag();

        _bubbleBar.transform.localScale = new Vector3(math.lerp(_bubbleBar.transform.localScale.x, _currBoostBubble, 0.5f), _bubbleBar.transform.localScale.y, _bubbleBar.transform.localScale.z);
        _rigidbody.linearDamping = CalcDrag();
        HandleWings();
    }

    public void ConnectPlayerInput(PlayerInput input)
    {
        playerInput = input;
        playerInput.actions["Move"].performed += ctx =>
        {
            _prevMovementInput = _movementInput;
            _movementInput = ctx.ReadValue<Vector2>();
        };
        playerInput.actions["Move"].canceled += ctx => _movementInput = Vector2.zero;

        playerInput.actions["Brake"].performed += ctx =>
        {
            _isBraking = true;
        };
        playerInput.actions["Brake"].canceled += ctx =>
        {
            _isBraking = false;
        };
    }

    void HandleWings()
    {
        var torque = _isBraking ? -wingOpenTorqueAmt : wingCloseTorqueAmt;

        wingL.AddRelativeTorque(0, 0, torque, ForceMode.Acceleration);
        wingR.AddRelativeTorque(0, 0, -torque, ForceMode.Acceleration);
    }

    float CalcDrag()
    {
        float drag = 0;

        if (State == PenguinState.WATER)
            drag += _waterDrag;
        
        if (_isBraking)
            drag += _brakeDrag;
        
        return drag;
    }

    private void OnTriggerEnter(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            State = PenguinState.WATER;
            _waterDrag = water.WaterDrag;
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
            _waterDrag = 0;
        }
        
        var charger = other.GetComponent<BubbleCharger>();
        if (charger != null)
        {
            _isCharging = false;
            _boostBubbleCharge = 0;
        }
    }
    
}
