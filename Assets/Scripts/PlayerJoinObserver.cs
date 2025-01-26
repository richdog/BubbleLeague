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

    }
    private void OnDisable()
    {
        JoinPlayer.onJoinSuccess -= PlayerJoined;
    }

    public void PlayerJoined(int id)
    {
        Debug.LogError(id);
        if(id == slotId)
        {
            joinedImage.SetActive(true);
        }
    }
}
