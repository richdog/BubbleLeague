using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinObserver : MonoBehaviour
{
    public int slotId;
    public GameObject joinedImage;

    private void OnEnable()
    {
        MatchManager.Instance.OnPlayerJoinChange += PlayerChange;
    }

    private void OnDisable()
    {
        MatchManager.Instance.OnPlayerJoinChange -= PlayerChange;
    }

    public void PlayerChange()
    {
        var playerInputs = FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (MatchManager.Instance.PlayerExists(slotId))
            joinedImage.SetActive(true);
        else
            joinedImage.SetActive(false);
    }
}