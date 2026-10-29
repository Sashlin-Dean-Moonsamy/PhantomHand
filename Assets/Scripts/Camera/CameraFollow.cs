using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector2 offset;
    public float smoothSpeed = 5f;

    [Header("Ground Constraint")]
    public LayerMask groundLayer;
    public float raycastDistance = 20f;
    public float cameraHeightBuffer = 2f;

    private bool cinematicMode = false;

    private Coroutine shakeCoroutine;

    private void LateUpdate()
    {
        if (cinematicMode)
            return;

        if (!target)
            return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y,
            transform.position.z
        );

        RaycastHit2D hit = Physics2D.Raycast(
            desired,
            Vector2.down,
            raycastDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            float minY = hit.point.y + cameraHeightBuffer;

            if (desired.y < minY)
                desired.y = minY;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desired,
            smoothSpeed * Time.deltaTime
        );
    }

    public void SetCinematicMode(bool enabled)
    {
        cinematicMode = enabled;
    }

    public void Shake(
        float duration,
        float strength
    )
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(
            ShakeRoutine(duration, strength)
        );
    }

    private IEnumerator ShakeRoutine(
        float duration,
        float strength
    )
    {
        Vector3 originalPosition =
            transform.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            // Strong at the beginning, then quickly settles.
            float falloff = 1f - progress;

            Vector2 offset =
                Random.insideUnitCircle *
                strength *
                falloff;

            transform.position =
                new Vector3(
                    originalPosition.x + offset.x,
                    originalPosition.y + offset.y,
                    originalPosition.z
                );

            yield return null;
        }

        transform.position = originalPosition;

        shakeCoroutine = null;
    }
}