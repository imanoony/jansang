using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Body : MonoBehaviour
{
    [SerializeField] private Boss1Manage bossManage;
    [SerializeField] private GameObject shockwave;
    [SerializeField] private List<Sprite> shockwaveSprites;
    private Boss1Shockwave boss1Shockwave;
    private SpriteRenderer shockwaveRenderer;
    private Transform shockwaveTransform;

    private void Awake()
    {
        if (bossManage == null) bossManage = GetComponentInParent<Boss1Manage>();
        if (shockwave == null) {
            shockwave = transform.Find("Shockwave").gameObject;
        }
        shockwaveRenderer = shockwave.GetComponent<SpriteRenderer>();
        shockwaveTransform = shockwave.GetComponent<Transform>();
        boss1Shockwave = shockwave.GetComponent<Boss1Shockwave>();

        shockwave.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Boss1Shockwave(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shockwaveTransform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        StartCoroutine(ShockwaveCoroutine());
    }

    private IEnumerator ShockwaveCoroutine()
    {
        shockwave.SetActive(true);
        
        shockwaveRenderer.sprite = shockwaveSprites[0];
        yield return new WaitForSeconds(0.5f);
        shockwaveRenderer.sprite = shockwaveSprites[1];
        boss1Shockwave.CheckHit();
        yield return new WaitForSeconds(0.5f);
        shockwave.SetActive(false);

        bossManage.currentBodyPattern = BodyPattern.Idle;
        bossManage.bodyPatternTimer = 4.0f;
    }

    public void Boss1Judgement(GameObject targetAltar)
    {
        
    }
}
