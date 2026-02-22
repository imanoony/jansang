using System;
using UnityEngine;

public class StageClearChecker : MonoBehaviour
{
    public CameraFollow2D cameraFollow2D;
    public GameObject[] stageWalls;
    public Collider2D[] stageCameraBoundaryColliders;
    public int[] requestedEnemies;

    private int currentSection = 0;
    private int killEnemy = 0; 
    private void Start()
    {
        throw new NotImplementedException();
    }

    public void Kill()
    {
        killEnemy++;

        for (int i = currentSection; i < requestedEnemies.Length; i++)
        {
            if (killEnemy >= requestedEnemies[i])
            {
                stageWalls[i].gameObject.SetActive(false);
                cameraFollow2D.boundsCollider = stageCameraBoundaryColliders[i+1];
                currentSection = i + 1;
            }
        }
    }
}
