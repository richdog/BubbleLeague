using UnityEngine;

public class BubbleCharger : MonoBehaviour
{
    
    [SerializeField, Range(0,1)] private float _chargeRate;
    public float ChargeRate => _chargeRate;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
