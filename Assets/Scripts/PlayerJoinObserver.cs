using TMPro;
using UnityEngine;

public class PlayerJoinObserver : MonoBehaviour
{
    public TMP_Text Text;

    public int slotId;

    private void Start()
    {
        MatchManager.Instance.OnPlayerJoinChange += PlayerChange;
    }

    private void OnDisable()
    {
        MatchManager.Instance.OnPlayerJoinChange -= PlayerChange;
    }

    public void PlayerChange()
    {
        //var playerInputs = FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        var description = MatchManager.Instance.GetDescriptionForPlayer(slotId);

        Text.SetText(description);
    }
}