using UnityEngine;

public class ProjectileCaster : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float hoverTime = 0.5f;

    bool canCast = true;
    ReturningProjectile currentProjectile;
    
    private CharacterManager manager;
    
    public void Init(CharacterManager manager)
    {
        this.manager = manager;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canCast)
        {
            Cast();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            transform.position = currentProjectile.transform.position;
            currentProjectile.Collect();
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            if (currentProjectile != null)
            {
                currentProjectile.state = ReturningProjectile.State.Returning;
            }
        }
    }

    void Cast()
    {
        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        GameObject proj = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        currentProjectile = proj.GetComponent<ReturningProjectile>();
        currentProjectile.Init(
            firePoint.position,
            mouseWorld-firePoint.position,
            transform,
            OnProjectileReturned
        );

        canCast = false;
    }

    void OnProjectileReturned()
    {
        canCast = true;
    }
}