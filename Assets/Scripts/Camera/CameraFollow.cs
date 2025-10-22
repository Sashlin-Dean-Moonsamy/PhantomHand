using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;        // Player
    public Vector2 offset;          // Offset from player
    public float smoothSpeed = 5f;  // Camera smoothing speed

    [Header("Ground Constraint")]
    public LayerMask groundLayer;
    public float raycastDistance = 20f; // How far down to check for ground
    public float cameraHeightBuffer = 2f; // Keep camera this far above ground

    private void LateUpdate()
    {
        if (!target) return;

        // 1️⃣ Desired position based on player
        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        // 2️⃣ Ground check: cast a ray downward from desired position
        RaycastHit2D hit = Physics2D.Raycast(desired, Vector2.down, raycastDistance, groundLayer);

        if (hit.collider != null)
        {
            // Keep camera above ground by a buffer
            float minY = hit.point.y + cameraHeightBuffer;
            if (desired.y < minY)
                desired.y = minY;
        }

        // 3️⃣ Smooth follow
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
