using System.Collections.Generic;
using UnityEngine;

public class Boss2_FoxFireSpawn : MonoBehaviour
{
    [SerializeField] private GameObject foxFirePrefab;
    [SerializeField] private Boss2_Manage bossManage;
    [SerializeField] private List<GameObject> activeFoxFires = new List<GameObject>();
    [SerializeField] private float spawnTimer = 0f;
    [SerializeField] private float spawnInterval = 1f;

    void Update()
    {
        if(bossManage.fireOn)
        {
            spawnTimer += Time.deltaTime;
            if(spawnTimer < spawnInterval) { return; }
            int firePos = Random.Range(-10, 10);
            GameObject foxFire = Instantiate(foxFirePrefab, transform.position + new Vector3(firePos, 0f, 0f), Quaternion.identity, transform);
            activeFoxFires.Add(foxFire);
            spawnTimer = 0f;
        }
        else
        {
            foreach(GameObject fire in activeFoxFires){
                if (fire != null)
                {
                    Destroy(fire);
                }
            }
            activeFoxFires.Clear();
        }
    }



}
