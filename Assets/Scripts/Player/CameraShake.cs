using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    private Vector3 originalPos;

    private void OnEnable()
    {
        EventManager.OnPlayerDetected += HandlePlayerDetected;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDetected -= HandlePlayerDetected;
    }

    private void HandlePlayerDetected(bool detected)
    {
        if (detected)
        {
            StopAllCoroutines();
            StartCoroutine(Shake(shakeDuration, shakeMagnitude));
        }
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            if (Time.timeScale > 0)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
