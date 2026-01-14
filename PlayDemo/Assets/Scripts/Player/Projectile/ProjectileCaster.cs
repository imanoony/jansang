using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileCaster : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float hoverTime = 0.5f;

    bool canCast = true;
    ReturningProjectile currentProjectile;
    PlayerMovement2D movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement2D>();
    }
    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started)
            if (canCast)
            {
                Cast();
            }
            else
            {
                transform.position = currentProjectile.transform.position;
                currentProjectile.Collect();
            }
    }
    public void OnReceive(InputAction.CallbackContext context)
    {
        if (context.started)
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
        movement.ResetJump();
    }
}