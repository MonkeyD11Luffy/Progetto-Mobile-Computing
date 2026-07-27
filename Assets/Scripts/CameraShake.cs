using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPosition;
    private Coroutine activeShake;

    private void Awake()
    {
        Instance = this;
        originalPosition = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (activeShake != null) StopCoroutine(activeShake);
        activeShake = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 offset = Random.insideUnitCircle * magnitude;
            transform.localPosition = originalPosition + (Vector3)offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        activeShake = null;
    }
}
