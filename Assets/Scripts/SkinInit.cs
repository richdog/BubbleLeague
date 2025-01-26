using UnityEngine;

public class SkinInit: MonoBehaviour
{
    public Player player;
    public SpriteRenderer bodyRender;
    public SpriteRenderer leftWindRender;
    public SpriteRenderer rightWindRender;

    public Sprite p1Body;
    public Sprite p1Wing;

    public Sprite p2Body;
    public Sprite p2Wing;

    public Sprite p3Body;
    public Sprite p3Wing;
    
    public Sprite p4Body;
    public Sprite p4Wing;

    public void Start()
    {
        SetPlayerSkin(player.playerId);
    }

    public void SetPlayerSkin(int id)
    {
        switch(id)
        {
            case 1:
                bodyRender.sprite = p1Body;
                leftWindRender.sprite = p1Wing;
                rightWindRender.sprite= p1Wing;
                break;

            case 2:
                bodyRender.sprite = p2Body;
                leftWindRender.sprite = p2Wing;
                rightWindRender.sprite = p2Wing;
                break;

            case 3:
                bodyRender.sprite = p3Body;
                leftWindRender.sprite = p3Wing;
                rightWindRender.sprite = p3Wing;
                break;

            case 4:
                bodyRender.sprite = p4Body;
                leftWindRender.sprite = p4Wing;
                rightWindRender.sprite = p4Wing;
                break;
        }
    }
}
