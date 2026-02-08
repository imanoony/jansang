using System.Collections.Generic;
using UnityEngine;

public class Boss1Altar : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private List<Sprite> altarSprites;

    [Header("Health Settings")]
    [SerializeField] private int health;
    public bool active = true;
    private int currentHealth;

    [SerializeField] private Boss1Manage bossManage;
    private SpriteRenderer altarSR;

    private void Awake()
    {
        altarSR = GetComponent<SpriteRenderer>();
        health = altarSprites.Count - 1;
    }

    private void Start()
    {
        active = true;
        currentHealth = health;
        altarSR.sprite = altarSprites[0];
    }

    public void ResetAltar()
    {
        active = true;
        currentHealth = health;
        altarSR.sprite = altarSprites[0];
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bossManage.currentBodyPattern != BodyPattern.Judgement) return;

        if ((bossManage.attackLayer.value & (1 << other.gameObject.layer)) > 0 && currentHealth > 0)
        {
            currentHealth--;
            altarSR.sprite = altarSprites[health-currentHealth];

            if(currentHealth <= 0)
            {
                active = false;
            }
        }
    }

}
