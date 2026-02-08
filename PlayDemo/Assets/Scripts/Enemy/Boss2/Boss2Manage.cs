using UnityEngine;

public enum Boss2Pattern
{
    Dash = 0,
    Slash = 1,
    Counter = 2,
    Laser = 3
}

public class Boss2Manage : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody2D bossRB;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float breakSpeed = 1f;


    [Header("Player References")]
    public GameObject playerObject;
    public Transform playerTransform;
    public PlayerHitCheck playerHitCheck;
    public PlayerMovement2D playerMovement;
    public Rigidbody2D playerRigidbody;
    public LayerMask attackLayer;
    


    [Header("Pattern")]
    [SerializeField] private Boss2Pattern currentPattern;

    [Header("Pattern Timer")]
    [SerializeField] private float patternTimer = 2f;
    

    [Header("Pattern Cooldown")]
    [SerializeField] private float patternCooldown = 4f;



    private void Awake()
    {
        bossRB = GetComponent<Rigidbody2D>();

        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject.GetComponent<Transform>();
        playerMovement = playerObject.GetComponent<PlayerMovement2D>();
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        playerHitCheck = playerObject.GetComponent<PlayerHitCheck>();
    }


    private void Update()
    {
        BossMoveManage();
        // BodyPatternManage();
    }

    public void TakeDamage(int damage)
    {
        
    }

    private void BossMoveManage()
    {
        
    }


    // private void BodyPatternManage()
    // {
    //     switch (currentBodyPattern)
    //     {
    //         case BodyPattern.Shockwave:
            
    //             break;
    //         case BodyPattern.Judgement:
            
    //             break;
    //     }
    // }


}
