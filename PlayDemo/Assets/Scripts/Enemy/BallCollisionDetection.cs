using System;
using UnityEngine;
using UnityEngine.Events;
public class BallCollisionDetection : MonoBehaviour
{
    public LayerMask detectMask;
    public UnityEvent<Collision2D> onCollision = new UnityEvent<Collision2D>();
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & detectMask.value) != 0)
            onCollision.Invoke(other);
    }
}

