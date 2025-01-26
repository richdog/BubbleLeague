using UnityEngine;
using UnityEngine.VFX;

public class SkinInit : MonoBehaviour
{
    [System.Serializable]
    public struct PlayerSkin
    {
        public Sprite body;
        public Sprite bodyTurn;
        public Sprite bodyHardTurn;
        public Sprite wing;
        public Color bubbleColor;
    }

    public Player player;
    public SpriteRenderer bodyRender;
    public SpriteRenderer leftWindRender;
    public SpriteRenderer rightWindRender;
    public VisualEffect bubbleEffect;
    public PlayerSkin[] skins;

    public PlayerSkin activeSkin;

    public void Start()
    {
        SetPlayerSkin(player.playerId);
    }

    public void SetPlayerSkin(int id)
    {
        if (id >= 0 && id < skins.Length)
        {
            PlayerSkin skin = skins[id];
            activeSkin = skin;
            bodyRender.sprite = skin.body;
            leftWindRender.sprite = skin.wing;
            rightWindRender.sprite = skin.wing;
            bubbleEffect.SetVector4("BubbleColor", skin.bubbleColor);
        }
    }

    public void SetBodySprite(float value)
    {
        Debug.Log(value);

        bodyRender.sprite = value switch
        {
            < -0.6f => activeSkin.bodyHardTurn,
            < -0.2f => activeSkin.bodyTurn,
            < 0.2f => activeSkin.body,
            < 0.6f => activeSkin.bodyTurn,
            _ => activeSkin.bodyHardTurn
        };
        bodyRender.flipX = value < 0;
    }
}