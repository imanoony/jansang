using System.Collections.Generic;
using UnityEngine;

public class Boss2_FoxFire : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private List<Sprite> foxFireSprites;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.2f;
    [SerializeField] private bool damaged = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        damaged = false;
    }

    void Update()
    {
        if (!damaged)
        {
            if(Physics2D.OverlapCircle(transform.position, 0.3f, playerLayer) != null)
            {
                damaged = true;
                PlayerHitCheck playerHitCheck = GameObject.FindWithTag("Player").GetComponent<PlayerHitCheck>();
                playerHitCheck.TakeDamage(1);
            }
        }

        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= spriteChangeInterval)
        {
            sr.sprite = foxFireSprites[Random.Range(0, foxFireSprites.Count)];
            spriteChangeTimer = 0f;
        }
    } 

}
