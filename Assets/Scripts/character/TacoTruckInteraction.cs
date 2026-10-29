using UnityEngine;
using UnityEngine.InputSystem;

public class TacoTruckInteraction : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    private bool playerInRange;
    private bool interactionUsed;

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (interactionUsed)
            return;

        interactionUsed = true;

        playerAnimator.SetTrigger("ChowTaco");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
    }
}