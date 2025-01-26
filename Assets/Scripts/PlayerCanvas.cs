using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    [SerializeField]
    private Player _player;

    [SerializeField] private TMP_Text _playerNumberText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _bubbleImage;

    public Image Token => _backgroundImage;

    //void Start()
    //{
    //    _playerNumberText.text = $"P{_player.playerId}";
    //}

    // Update is called once per frame
    void Update()
    {
        _bubbleImage.fillAmount = _player.CurrBoostBubble;
    }
}
