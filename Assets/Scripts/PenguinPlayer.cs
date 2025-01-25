using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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

    private GameObject _wingL;
    private GameObject _wingR;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody.useGravity = false;
        _playerInput = GetComponent<PlayerInput>();
        
        _wingL = GameObject.Find("Wing L");
        _wingR = GameObject.Find("Wing R");
        
        

        _playerInput.actions["Move"].performed += ctx =>
        {
            _prevMovementInput = _movementInput;
            _movementInput = ctx.ReadValue<Vector2>();
        };
        _playerInput.actions["Move"].canceled += ctx => _movementInput = Vector2.zero;

        _playerInput.actions["Brake"].performed += ctx =>
        {
            _isBraking = true;
            _wingL.transform.RotateAround((_wingL.transform.position + 0.3f*_wingL.transform.up), Vector3.forward, -75);
            _wingR.transform.RotateAround((_wingR.transform.position + 0.3f*_wingR.transform.up), Vector3.forward, 75);
        };
        _playerInput.actions["Brake"].canceled += ctx =>
        {
            _isBraking = false;
            _wingL.transform.RotateAround((_wingL.transform.position + 0.3f*_wingL.transform.up), Vector3.forward, 75);
            _wingR.transform.RotateAround((_wingR.transform.position + 0.3f*_wingR.transform.up), Vector3.forward, -75);
        };
        
        
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

        float lerpAmt = 0.8f;
        if (Vector2.Angle(_movementInput, _prevMovementInput) < 90 &&
            Vector2.Angle(_rigidbody.linearVelocity, _rigidbody.transform.up) > 90)
        {
            lerpAmt = 0.3f;
        }
        Quaternion lookDir = Quaternion.LookRotation(Vector3.forward, Vector2.Lerp(_movementInput.normalized, _rigidbody.linearVelocity.normalized, lerpAmt));
        _rigidbody.MoveRotation(lookDir);
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
