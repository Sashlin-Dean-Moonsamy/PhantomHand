using UnityEngine;

// Attach this script to any background GameObject that should parallax (clouds, smoke, etc.)
// The further back the layer, the smaller the parallaxFactor should be.
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor = 0.5f; // Lower value = slower movement = further away
    private Transform cam; // Main camera reference
    private Vector3 previousCamPosition; // Store previous frame’s position

    void Start()
    {
        cam = Camera.main.transform;
        previousCamPosition = cam.position; // Save starting position
    }

    void LateUpdate()
    {
        // Calculate how much the camera moved since last frame
        Vector3 delta = cam.position - previousCamPosition;

        // Move this layer only by a fraction of the camera's movement
        transform.position += delta * parallaxFactor;

        // Update stored camera position for next frame
        previousCamPosition = cam.position;
    }
}
