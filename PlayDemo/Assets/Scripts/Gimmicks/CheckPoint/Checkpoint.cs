using UnityEngine;
public interface IInteractable
{
    void Interact();
}
public class Checkpoint : MonoBehaviour, IInteractable
{
    bool activated = false;

    public void Interact()
    {
        GameManager.Instance.UI.ShowCheckPoint();
        if (activated) return;

        activated = true;
        GetComponent<SpriteRenderer>().color = Color.green;
    }
}

