using UnityEngine;

public class GameVars : MonoBehaviour
{
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
        public float boostForce;
        public float playerHitForce;
    }

    [System.Serializable]
    public struct BallVars
    {
        [Range(0, 2)] public float waterDragModifier;
        [Range(0, 2)] public float buoyancy;
    }

    [SerializeField]
    private PlayerVars _player = new PlayerVars
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
        boostForce = 50f,
        playerHitForce = 2f
    };

    [SerializeField]
    private BallVars _ball = new BallVars
    {
        waterDragModifier = 0.5f,
        buoyancy = 1f
    };

    private static GameVars _instance;
    public static PlayerVars Player => _instance._player;
    public static BallVars Ball => _instance._ball;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);
    }
}