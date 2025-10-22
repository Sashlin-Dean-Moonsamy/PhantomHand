using UnityEngine;

public class StaticBackground : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    void LateUpdate()
    {
        if (!cameraTransform) return;
        transform.position = new Vector3(
            cameraTransform.position.x,
            cameraTransform.position.y,
            transform.position.z
        );
    }
}
