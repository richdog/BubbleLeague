using UnityEngine;
using UnityEngine.VFX;

public class SkinInit : MonoBehaviour
{
    public Player player;
    public SpriteRenderer bodyRender;
    public SpriteRenderer leftWindRender;
    public SpriteRenderer rightWindRender;
    public VisualEffect bubbleEffect;

    public Sprite p1Body;
    public Sprite p1Wing;
    public Color p1BubbleColor;

    public Sprite p2Body;
    public Sprite p2Wing;
    public Color p2BubbleColor;

    public Sprite p3Body;
    public Sprite p3Wing;
    public Color p3BubbleColor;

    public Sprite p4Body;
    public Sprite p4Wing;
    public Color p4BubbleColor;

    public void Start()
    {
        SetPlayerSkin(player.playerId);
    }

    public void SetPlayerSkin(int id)
    {
        switch (id)
        {
            case 0:
                bodyRender.sprite = p1Body;
                leftWindRender.sprite = p1Wing;
                rightWindRender.sprite = p1Wing;
                bubbleEffect.SetVector4("BubbleColor", p1BubbleColor);
                break;

            case 1:
                bodyRender.sprite = p2Body;
                leftWindRender.sprite = p2Wing;
                rightWindRender.sprite = p2Wing;
                bubbleEffect.SetVector4("BubbleColor", p2BubbleColor);
                break;

            case 2:
                bodyRender.sprite = p3Body;
                leftWindRender.sprite = p3Wing;
                rightWindRender.sprite = p3Wing;
                bubbleEffect.SetVector4("BubbleColor", p3BubbleColor);
                break;

            case 3:
                bodyRender.sprite = p4Body;
                leftWindRender.sprite = p4Wing;
                rightWindRender.sprite = p4Wing;
                bubbleEffect.SetVector4("BubbleColor", p4BubbleColor);
                break;
        }
    }
}