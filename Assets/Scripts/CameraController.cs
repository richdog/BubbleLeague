using UnityEngine;
using Random = UnityEngine.Random;

public class CameraController : MonoBehaviour
{
    [SerializeField] private new Camera camera;

    private Vector3 _currentVelocity;
    private float _needsNewTarget;
    private float _shakeAmount;
    private float _shakeTime;
    private Vector3 _target;

    public static CameraController Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _target = camera.transform.position;

        MatchManager.Instance.OnGoalScored += ShakeCameraOnGoal;
    }

    private void OnDestroy()
    {
        MatchManager.Instance.OnGoalScored -= ShakeCameraOnGoal;
    }

    private void Update()
    {
        //shake behavior here. gets started by calling shakeCamera and stops itself after the specified time.
        //if (_shakeTime > 0)
        //{
        if (_needsNewTarget >= 0.005f)
        {
            _target = new Vector3(Random.Range(-_shakeAmount, _shakeAmount),
                Random.Range(-_shakeAmount, _shakeAmount),
                camera.transform.position.z);
            _needsNewTarget -= 0.005f;
        }

        camera.transform.position =
            Vector3.SmoothDamp(camera.transform.position, _target, ref _currentVelocity, 0.03f);
        _shakeTime -= Time.deltaTime;
        _needsNewTarget += Time.deltaTime;
        if (_shakeTime <= 0)
        {
            _shakeTime = 0;
            _shakeAmount = 0;
        }
        //}
    }

    //Forces camera return to its home
    //not currently used
    private void ReturnToZero()
    {
        camera.transform.position = new Vector3(0, 0, 0);
    }

    //starts shaking the camera. Intensity of 1 is probably the highest you should go.
    //If called again before the previous shake is completed it will add time and use the higher intensity.
    //This behavior can be improved if we have time.
    public void ShakeCamera(float intensity, float howLong)
    {
        if (_shakeTime <= intensity) _shakeAmount = intensity;
        _shakeTime += howLong;
    }

    private void ShakeCameraOnGoal()
    {
        ShakeCamera(0.6f, 0.8f);
    }
}