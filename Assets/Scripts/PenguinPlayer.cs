using UnityEngine;
using UnityEngine.InputSystem;

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

    private GameObject _wingL;
    private GameObject _wingR;

    [SerializeField, Range(0, 2)] private float _buoyancy = 1;

    [SerializeField, Range(0, 10)] private float _rotDampening = 2f;
    [SerializeField, Range(0, 10)] private float _rotAcceleration = 0.2f;

    [SerializeField, Range(0, 1)] private float _brakeWingPivotHeight = 0.3f;

    [SerializeField] private Rigidbody wingL;
    [SerializeField] private Rigidbody wingR;

    [SerializeField] private float wingOpenTorqueAmt = 300f;
    [SerializeField] private float wingCloseTorqueAmt = 500f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();        

        _playerInput.actions["Move"].performed += ctx =>
        {
            _prevMovementInput = _movementInput;
            _movementInput = ctx.ReadValue<Vector2>();
        };
        _playerInput.actions["Move"].canceled += ctx => _movementInput = Vector2.zero;

        _playerInput.actions["Brake"].performed += ctx =>
        {
            _isBraking = true;
        };
        _playerInput.actions["Brake"].canceled += ctx =>
        {
            _isBraking = false;
        };
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
        }

        _rigidbody.linearDamping = CalcDrag();
        HandleWings();
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
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        var water = other.GetComponent<Water>();
        if (water != null)
        {
            State = PenguinState.AIR;
            _waterDrag = 0;
        }
    }
    
}
