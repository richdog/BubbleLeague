using FMODUnity;
using UnityEngine;

public class TestFMOD : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        RuntimeManager.PlayOneShot("event:/foghorn");
    }

    // Update is called once per frame
    private void Update()
    {
    }
}