using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
public class MagicBall : MonoBehaviour
{
    private GameObject player;
    private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float destroyTime;
    [SerializeField] private Collider2D ball;
    [SerializeField] private Collider2D detectBoundary;
    [SerializeField] [CanBeNull] private BallCollisionDetection ballCollision;
    [SerializeField] [CanBeNull] private BallCollisionDetection boundaryCollision;
    public void Init(GameObject player, float speed)
    {
        this.player = player;
        this.speed = speed;
        float angle = Mathf.Atan2(
            player.transform.position.y - transform.position.y, 
            player.transform.position.x - transform.position.x);
        ballCollision?.onCollision.AddListener(Explosion);
        boundaryCollision?.onCollision.AddListener(Explosion);
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        DestroyTimerAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }
    private void Update()
    {
        Vector2 toPlayer = player.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f; 
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        transform.position += transform.up * (speed * Time.deltaTime);
    }
    private async UniTask DestroyTimerAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: token);
        Destroy(gameObject);
    }
    private void Explosion(Collision2D collision)
    {
        Debug.Log("EXPLODE!!!!");
        Destroy(gameObject);
    }
}

