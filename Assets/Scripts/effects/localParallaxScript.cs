using UnityEngine;

public class LocalParallax : MonoBehaviour
{
    [Tooltip("How much the layer should move compared to the camera. Smaller = farther away.")]
    public float parallaxFactor = 0.5f;

    private Transform cam;
    private Vector3 startPos;

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
    }

    void LateUpdate()
    {
        // Difference between camera’s current and starting position
        Vector3 camMovement = cam.position - startPos;

        // Apply a scaled offset (only on X, optional on Y)
        transform.position = new Vector3(
            startPos.x + camMovement.x * parallaxFactor,
            startPos.y + camMovement.y * parallaxFactor,
            transform.position.z
        );
    }
}
