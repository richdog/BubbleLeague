using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJoinObserver: MonoBehaviour
{
    public int slotId;
    public PlayerInputManager InputManager;
    public GameObject joinedImage;

    private void OnEnable()
    {
        JoinPlayer.onJoinSuccess += PlayerJoined;
        JoinPlayer.onJoinAborted += PlayerLeft;
    }
    private void OnDisable()
    {
        JoinPlayer.onJoinSuccess -= PlayerJoined;
        JoinPlayer.onJoinAborted -= PlayerLeft;
    }

    public void PlayerJoined(int id)
    {
        PlayerInput[] playerInputs = FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (PlayerInput playerInput in playerInputs)
        {
            if (playerInput.playerIndex == slotId)
            {
                joinedImage.SetActive(true);
            }
        }
       
    }

    public void PlayerLeft(int id)
    {
        if (id == slotId)
        {
            joinedImage.SetActive(false);
        }
        PlayerInput[] playerInputs = FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (PlayerInput playerInput in playerInputs)
        {
            if (playerInput.playerIndex == slotId && id != slotId)
            {
                joinedImage.SetActive(true);
            }
        }

    }
}
