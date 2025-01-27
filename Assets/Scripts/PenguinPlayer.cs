using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Player : MonoBehaviour
{
    public enum PenguinState
    {
        WATER,
        AIR
    }

    public PenguinState State { get; set; } = PenguinState.WATER;

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

    public float CurrBoostBubble => _currBoostBubble;

    private float _currentBrakeDrag = 0f;

    [SerializeField] private Rigidbody wingLRB;
    [SerializeField] private PlayerRigidbody wingL;
    [SerializeField] private Rigidbody wingRRB;
    [SerializeField] private PlayerRigidbody wingR;
    [SerializeField] private PlayerRigidbody bodyRigidbody;

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
        bodyRigidbody.CollisionEnter += OnCollisionEnter;
        bodyRigidbody.TriggerEnter += OnTriggerEnter;
        bodyRigidbody.TriggerExit += OnTriggerExit;

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

    private void OnDestroy()
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

    float maxAccumulatedAngle = 45;

    void FixedUpdate()
    {
        float massMultiplier = isSpinning ? GameVars.Player.spinMassMultiplier : _isBraking ? GameVars.Player.brakeMassMultiplier : 1;
        _rigidbody.mass = GameVars.Player.playerBodyMass * massMultiplier;
        wingLRB.mass = GameVars.Player.playerWingMass * massMultiplier;
        wingRRB.mass = GameVars.Player.playerWingMass * massMultiplier;

        float totalMass = _rigidbody.mass + wingLRB.mass + wingRRB.mass;

        if (spinDir != 0)
        {
            if (!isSpinning)
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

        if (isStunned)
        {
            stunDuration -= Time.fixedDeltaTime;
            if (stunDuration < 0)
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
            skin.SetBodySprite(_rigidbody.angularVelocity.z / 180f);
        }

        if (State == PenguinState.WATER)
        {
            Vector2 force = _movementInput * GameVars.Player.acceleration * totalMass;
            _rigidbody.AddForce(force, ForceMode.Force);
            _rigidbody.AddForce(Physics.gravity * -GameVars.Player.penguinBuoyancy * totalMass, ForceMode.Force);

            if (_isBoosting && _currBoostBubble > 0)
            {
                boostParticles.SetFloat("BubbleAmount", 256);

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
        else
        {
            boostParticles.SetFloat("BubbleAmount", 0);
        }

        _currBoostBubble += Time.fixedDeltaTime * (State == PenguinState.WATER ? GameVars.Player.waterBubbleGainSpeed : State == PenguinState.AIR ? GameVars.Player.airBubbleGainSpeed : 0);

        _currBoostBubble = math.clamp(_currBoostBubble, 0, 1);
        float drag = CalcDrag();
        _rigidbody.linearDamping = drag;
        wingLRB.linearDamping = drag;
        wingRRB.linearDamping = drag;

        HandleWings();

        _prevMovementInput = _movementInput;
    }

    public void ConnectPlayerInput(PlayerInput input)
    {
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

        if (!isSpinning)
            _movementInput = ctx.ReadValue<Vector2>();
    }

    private void CancelMove(InputAction.CallbackContext ctx)
    {
        _prevMovementInput = Vector3.zero;
        _movementInput = Vector2.zero;
    }

    private void Brake(InputAction.CallbackContext ctx)
    {
        if (!isStunned)
            _isBraking = true;
    }

    private void CancelBrake(InputAction.CallbackContext ctx)
    {
        _isBraking = false;
    }

    private void Boost(InputAction.CallbackContext ctx)
    {
        if (!isStunned)
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
        if (!isStunned)
            spinDir = 1;

    }

    private void SpinRight(InputAction.CallbackContext ctx)
    {
        if (!isStunned)
            spinDir = -1;
    }


    void HandleWings()
    {
        float torque = _isBraking || isSpinning ? -GameVars.Player.wingOpenTorqueAmt : GameVars.Player.wingCloseTorqueAmt;

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

        float targetBrakeDrag = _isBraking ? GameVars.Player.brakeDrag : 0;
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
        Water water = other.GetComponent<Water>();
        if (water != null)
        {
            State = PenguinState.AIR;
        }

        BubbleCharger charger = other.GetComponent<BubbleCharger>();
        if (charger != null)
        {
            _isCharging = false;
            _boostBubbleCharge = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contactPoint = collision.GetContact(0);
        Vector3 hitPoint = contactPoint.point;

        ImpactBubbles.PlayHitEffect(contactPoint.impulse.magnitude / 3, Color.white * 1.2f,
                hitPoint, 0.1f, contactPoint.normal, contactPoint.impulse.magnitude / 10, 0, 1.5f, 5);

        if (isSpinning)
        {
            float hitForceMagnitude = _rigidbody.angularVelocity.magnitude * GameVars.Player.playerHitForce;
            Vector3 hitForce = -contactPoint.normal * hitForceMagnitude;
            Vector3 hitForceNormalized = hitForce.normalized;

#if UNITY_EDITOR
            //debug hit direction and impact point
            Debug.DrawLine(hitPoint - Vector3.up * 0.25f, hitPoint + Vector3.up * 0.5f, Color.red, 5f);
            Debug.DrawLine(hitPoint - Vector3.right * 0.25f, hitPoint + Vector3.right * 0.5f, Color.red, 5f);
            Debug.DrawLine(hitPoint - Vector3.forward * 0.25f, hitPoint + Vector3.forward * 0.5f, Color.red, 5f);
            Debug.DrawLine(hitPoint, hitPoint + (hitForce / 10), Color.yellow, 5f);
#endif
            //apply some backwards force to self
            //if (!_isBraking)
            //    _rigidbody.AddForceAtPosition(hitForce / 2, collision.GetContact(0).point, ForceMode.Impulse);


            //remove some force since something was hit
            _rigidbody.AddTorque(-_rigidbody.angularVelocity / 4, ForceMode.Impulse);

            ImpactBubbles.PlayHitEffect(hitForceMagnitude / 4 * 5f, skin.activeSkin.bubbleColor * 0.95f,
                hitPoint, 0.1f, -hitForceNormalized, hitForceMagnitude / 2 * 0.75f, 0.25f);

            Rigidbody otherRigidbody = collision.gameObject.GetComponentInParent<Rigidbody>();
            if (otherRigidbody != null)
            {
                Player otherPlayer = collision.gameObject.GetComponentInParent<Player>();
                Debug.Log($"{name} hit {otherRigidbody.name} with force: {hitForce}");
                if (otherPlayer == null || !otherPlayer._isBraking)
                {
                    ImpactBubbles.PlayHitEffect(hitForceMagnitude * 2.5f, skin.activeSkin.bubbleColor * 1.1f,
                        hitPoint, 0.1f, hitForceNormalized, hitForceMagnitude * 0.75f, 0.75f);

                    //either it's a player (that isn't braking) or another rigidbody, apply force
                    otherRigidbody.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);
                    //if it's actually a player, stun them for a short duration
                    if (otherPlayer != null)
                    {
                        Debug.Log($"{name} stunned {otherPlayer.name}!");
                        otherPlayer.Stun(GameVars.Player.stunDuration);
                    }
                }
                if (otherPlayer != null && otherPlayer._isBraking)
                {
                    //if the other player is braking, stun self, acts as a counter
                    Debug.Log($"{name} stunned self, due to counter by {otherPlayer.name}!");
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