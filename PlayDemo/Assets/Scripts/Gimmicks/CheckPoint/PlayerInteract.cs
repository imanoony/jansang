using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] InputActionReference interactAction;
    [SerializeField] private CameraFollow2D cameraFollow;
    IInteractable current;
    private Vector3 position;
    private void OnEnable()
    {
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (current != null)
        {
            cameraFollow.StartFocus(position);
        }
        current?.Interact();
        current = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.TryGetComponent(out current);
        if (current != null)
        {
            position = other.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<IInteractable>() == current)
            current = null;
    }
}