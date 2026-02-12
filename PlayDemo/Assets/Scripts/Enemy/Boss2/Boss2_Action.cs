using Unity.VisualScripting;
using UnityEngine;

public class Boss2_Action : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Boss2_Manage bossManage;
    private SpriteRenderer bossSR;

    void Awake()
    {
        bossManage = GetComponentInParent<Boss2_Manage>();
        bossSR = GetComponent<SpriteRenderer>();
    }


}
