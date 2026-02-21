using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(EnemyAlertEmitter))]
public class SummonerEnemy : EnemyBase
{
    #region  parameters
    [Header("Detection!")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float combatRadius = 1.5f;
    [SerializeField] private LayerMask sightMask;   
    [SerializeField] private LayerMask playerMask;
    [Header("Summon!")]
    [SerializeField] private GameObject[] summonees;
    [SerializeField] private float[] summonWeights;
    #endregion
    #region components
    [SerializeField] private EnemyAlertEmitter alertEmitter;
    #endregion
    #region status
    private bool automaticFlip;
    private float sumWeights;
    private float[] summonProbabilities;
    #endregion
    protected override void Start()
    {
        base.Start();
        if (alertEmitter == null) alertEmitter = GetComponent<EnemyAlertEmitter>();
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
        base.Update();
        if (automaticFlip && Player != null)
        {
            if (Player.position.x > transform.position.x) FlipByDirection(1);
            else if (Player.position.x < transform.position.x) FlipByDirection(-1);
        }
        if (!alerted)
        {
            alerted = DetectPlayer(detectionRadius, sightMask);
        }
        else
        {
            UpdateFound(combatRadius, sightMask);
        }
    }
    protected override async UniTask RunAIAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => alerted, cancellationToken: token);
        ChangeDirection(0);
        SetBaseColor(new Color(0f, 1f, 0f, 1f));
        alertEmitter?.Emit();
        currentState = State.Alert;
        CurrentTarget = Player;
        await AlertedActionAsync(token);
    }
    private async UniTask AlertedActionAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        while (!token.IsCancellationRequested)
        {
            await SummonRoutineAsync(token);
        }
    }
    private async UniTask SummonRoutineAsync(CancellationToken token)
    {
        if (CurrentTarget == null) return;
        SetBaseColor(new Color(0f, 1f, 1f, 1f));
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
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
        SetBaseColor(new Color(1f, 0f, 0f, 1f));
        Summon(cur);
        await UniTask.Delay(TimeSpan.FromSeconds(5f), cancellationToken: token);
    }
    private void Summon(int num)
    {
        var summon = Instantiate(summonees[num], transform.position + Vector3.left, Quaternion.identity);
    }
}
