using System;
using UnityEngine;
public class EnemyAlertEmitter : MonoBehaviour
{
    public event Action Alerted;
    public void Register(Action action)
    {
        Alerted += action;
    }
    public void Unregister(Action action)
    {
        Alerted -= action;
    }
    public void Emit()
    {
        Alerted?.Invoke();
    }
}

