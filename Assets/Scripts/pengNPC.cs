using UnityEngine;

public class pengNPC : MonoBehaviour
{

    [SerializeField] private new SpriteRenderer spriteRenderer;
    [SerializeField] public new Sprite normalSprite;
    [SerializeField] public new Sprite quackSprite;
    [SerializeField] public new Sprite cheerSprite;
    
    private float _actionTimer;

    private float _hopTimer = -0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale *= Random.Range(0.8f, 1.2f);

        MatchManager.Instance.OnGoalScored += CheerOnGoal;
    }
    
    private void OnDestroy()
    {
        MatchManager.Instance.OnGoalScored -= CheerOnGoal;
    }

    // Update is called once per frame
    void Update()
    {
        if (_actionTimer <= 0)
        {
            float rand = Random.Range(0f, 1f);
            if (rand < 0.6f)
            {
                _hopTimer = 0.3f;
            }

            if (rand < 0.2f)
            {
                spriteRenderer.sprite = quackSprite;
            }
            else if (rand < 0.6)
            {
                spriteRenderer.sprite = normalSprite;
            }
            _actionTimer = Random.Range(0.8f, 2.5f);
        }

        if (_hopTimer > -0.3f)
        {
            transform.position += Vector3.up * _hopTimer * Time.deltaTime * 3;
            _hopTimer -= Time.deltaTime;
        }
        
        
        _actionTimer -= Time.deltaTime;
    }
    
    private void CheerOnGoal()
    {
        spriteRenderer.sprite = cheerSprite;
    }
}


