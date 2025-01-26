using System.Collections.Generic;
using UnityEngine;

public class DestroyOnRestart : MonoBehaviour
{
    static List<DestroyOnRestart> instances = new List<DestroyOnRestart>();

    private void Awake()
    {
        instances.Add(this);
    }

    public static void DestroyAll()
    {
        foreach (var instance in instances)
        {
            if(instance != null)
                Destroy(instance.gameObject);
        }

        instances.Clear();
    }
}
