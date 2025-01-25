using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera camera = null; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (camera = null)
        {
            camera = GetComponentInChildren<Camera>();
        }
    }

 


}
