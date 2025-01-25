using Unity.VisualScripting;
using UnityEngine;

public class GameManager: MonoBehaviour
{
    // Static instance of the GameManager class
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of GameManager detected! Destroying this one.");
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        // Assign this instance to the static variable
        Instance = this;

        // Optionally, make this instance persist between scenes
        DontDestroyOnLoad(gameObject);
    }
}

