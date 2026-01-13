using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class SummonerEnemy : EnemyBase
{
    #region  parameters
    
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   // Player + Wall
    [SerializeField] private LayerMask playerMask;
    
    [Header("Summon!")]
    [SerializeField] private GameObject[] summonees;
    [SerializeField] private float[] summonWeights;
    
    #endregion
    
    #region components

    private SpriteRenderer spriteRenderer;
    
    #endregion
    
    #region status

    private bool alerted = false;
    private bool found = false;
    
    private float directionTimeChangeElapsed;
    
    public GameObject player;
    private Transform currentTarget;

    private bool automaticFlip;

    private float sumWeights;
    private float[] summonProbabilities;
    
    #endregion
    protected override void Start()
    {
        base.Start();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(WaitForAlert());

        sumWeights = 0;

        for (int i = 0; i < summonWeights.Length; i++)
        {
            sumWeights += summonWeights[i];
        }

        if (summonWeights.Length < summonees.Length)
        {
            sumWeights += (summonees.Length - summonWeights.Length);
        }

        summonProbabilities = new float[summonees.Length];
        
        for (int i = 0; i < summonWeights.Length; i++)
        {
            if (i == 0) summonProbabilities[i] = summonWeights[i] / sumWeights;
            else summonProbabilities[i] = summonProbabilities[i - 1] + summonWeights[i] / sumWeights;
        }
        
        for (int i = summonWeights.Length - 1; i < summonees.Length; i++)
        {
            summonProbabilities[i] = summonProbabilities[i - 1] + 1f / sumWeights;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        if (automaticFlip)
        {
            //TODO: 하 이거 진짜;;;
            if (player.transform.position.x > transform.position.x) Flip(1);
            else if (player.transform.position.x < transform.position.x) Flip(-1);
        }
        
        if (!alerted)
        {
            
            alerted = DetectPlayer(detectionRadius);
        }
        else
        {
            found = DetectPlayer(combatRadius); 
        }
    }

    private void Flip(int direction)
    {
        //TODO: 좀 이상한데 급하게 하느라 이렇게 됨... 이거 수정해야함

        var x = Mathf.Abs(transform.localScale.x);
        if (direction == -1) transform.localScale = new Vector3(-x, transform.localScale.y, transform.localScale.z);
        else if (direction == 1) transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }
    
    private IEnumerator WaitForAlert()
    {
        yield return new WaitUntil(() => alerted);
        
        ChangeDirection(0);
        //TEST
        spriteRenderer.color = new Color(0f, 1f, 0f, 1f);
            
        currentState = State.Alert;
        currentTarget = player.transform;

        StartCoroutine(AlertedAction());
    }

    private IEnumerator AlertedAction()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            yield return StartCoroutine(SummonRoutine());
        }
    }
    
    private bool DetectPlayer(float range)
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > range) return false;
        
        Vector2 dir = (player.transform.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, sightMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;

        return false;
    }
    
    private IEnumerator SummonRoutine()
    {
        if (currentTarget ==null) yield break;
        
        spriteRenderer.color = new Color(0f, 1f, 1f, 1f);

        float prob = Random.Range(0f, 1f);
        int cur = summonProbabilities.Length / 2;
        while (true)
        {
            if (prob < summonProbabilities[cur])
            {
                if (cur <= 0 || prob > summonProbabilities[cur - 1]) break;
                cur /= 2;
            }
            else
            {
                if (cur >= summonProbabilities.Length - 1) break;
                cur = (cur + 1 + summonProbabilities.Length - 1) / 2;
            }
        }

        yield return new WaitForSeconds(1f);
        spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
        
        Summon(cur);
        
        yield return new WaitForSeconds(5f);
    }

    private void Summon(int num)
    {
        // summon 위치 고르기?
        var summon = Instantiate(summonees[num], transform.position + Vector3.left, Quaternion.identity);
    }
}
