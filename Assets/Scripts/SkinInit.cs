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
        [ColorUsage(false, true)]
        public Color bubbleColor;
        public Sprite token;
    }

    public Player player;
    public PlayerCanvas canvas;
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
            canvas.Token.sprite = skin.token;
            bubbleEffect.SetVector4("BubbleColor", skin.bubbleColor);
        }
    }

    public void SetBodySprite(float value)
    {
        bodyRender.sprite = value switch
        {
            < -0.9f => activeSkin.bodyHardTurn,
            < -0.15f => activeSkin.bodyTurn,
            < 0.15f => activeSkin.body,
            < 0.9f => activeSkin.bodyTurn,
            _ => activeSkin.bodyHardTurn
        };
        bodyRender.flipX = value < 0;
    }
}