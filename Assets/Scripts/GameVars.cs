using UnityEngine;

public class GameVars : MonoBehaviour
{
    [System.Serializable]
    public struct GeneralVars
    {
        public int pointsNeededToWin;
    }

    [System.Serializable]
    public struct PlayerVars
    {
        [Range(1, 30)] public float acceleration;
        public float brakeDrag;
        public float waterDrag;
        public float airDrag;
        [Range(0, 1)] public float brakeDragLerpSpeed;
        [Range(0, 2)] public float penguinBuoyancy;
        [Range(0, 10)] public float rotDampening;
        [Range(0, 10)] public float rotAcceleration;
        public float wingOpenTorqueAmt;
        public float wingCloseTorqueAmt;
        [Range(0, 1)] public float boostBubbleBurn;
        [Range(0, 1)] public float spinBubbleBurn;
        public float boostForce;
        public float playerHitForce;
        public float playerBodyMass;
        public float playerWingMass;
        public float spinInitialForce;
        public float spinContinuedForce;
        public float spinDuration;
        public float stunDuration;
        [Range(0, 1)] public float airBubbleGainSpeed;
        [Range(0, 1)] public float waterBubbleGainSpeed;
        [Range(0.5f, 2)] public float spinMassMultiplier;
        [Range(0.5f, 2)] public float brakeMassMultiplier;
    }

    [System.Serializable]
    public struct BallVars
    {
        [Range(0, 2)] public float waterDragModifier;
        [Range(0, 2)] public float buoyancy;
        public float ballMass;
        public float ballSize;
    }

    [SerializeField]
    private GeneralVars _general = new()
    {
        pointsNeededToWin = 3
    };

    [SerializeField]
    private PlayerVars _player = new()
    {
        acceleration = 25f,
        brakeDrag = 20f,
        waterDrag = 1f,
        airDrag = 1f,
        brakeDragLerpSpeed = 0.1f,
        penguinBuoyancy = 1f,
        rotDampening = 2f,
        rotAcceleration = 0.2f,
        wingOpenTorqueAmt = 300f,
        wingCloseTorqueAmt = 500f,
        boostBubbleBurn = 0.1f,
        spinBubbleBurn = 0.2f,
        boostForce = 50f,
        playerHitForce = 2f,
        playerBodyMass = 1f,
        playerWingMass = 0.5f,
        spinInitialForce = 5f,
        spinContinuedForce = 5f,
        spinDuration = 1f,
        stunDuration = 2f,
        spinMassMultiplier = 1.5f,
        brakeMassMultiplier = 1.5f,
        airBubbleGainSpeed = 0.1f,
        waterBubbleGainSpeed = 0.05f,
    };

    [SerializeField]
    private BallVars _ball = new()
    {
        waterDragModifier = 0.5f,
        buoyancy = 1f,
        ballMass = 1f,
        ballSize = 1f
    };

    private static GameVars _instance;
    public static GeneralVars General => _instance._general;
    public static PlayerVars Player => _instance._player;
    public static BallVars Ball => _instance._ball;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }
}