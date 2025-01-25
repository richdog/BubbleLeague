using UnityEngine;
using UnityEngine.InputSystem;

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
    //private Vector2 _smoothedMovementInput;
    private bool _isBraking;
    private InputSystem_Actions _playerInputActions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody.useGravity = false;
        _playerInputActions = new InputSystem_Actions();

        _playerInputActions.Player.Move.performed += ctx => _movementInput = ctx.ReadValue<Vector2>();
        _playerInputActions.Player.Move.canceled += ctx => _movementInput = Vector2.zero;

        _playerInputActions.Player.Brake.performed += ctx => _isBraking = true;
        _playerInputActions.Player.Brake.canceled += ctx => _isBraking = false;
        
        _playerInputActions.Enable();
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    //physics update
    void FixedUpdate()
    {
        
        if (State == PenguinState.WATER)
        {
            _rigidbody.useGravity = false;
            var force = _movementInput * _acceleration;
            _rigidbody.AddForce(force, _forceMode);
        } else if (State == PenguinState.AIR)
        {
            _rigidbody.useGravity = true;
        }
        _rigidbody.linearDamping = CalcDrag();
        
        _rigidbody.MoveRotation(Quaternion.LookRotation(Vector3.forward, _rigidbody.linearVelocity.normalized));
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
