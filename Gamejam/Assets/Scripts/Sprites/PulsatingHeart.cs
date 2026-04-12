using UnityEngine;
using System.Collections;
public class PulsatingHeart : MonoBehaviour
{
    [Header("Scale Settings")]
    public float pulseScale = 1.2f;   // How big it gets
    public float pulseSpeed = 8f;     // How fast the beat happens
    public float restTime = 0.5f;     // Time between beats

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        while (true)
        {
            // First beat (big thump)
            yield return ScaleTo(originalScale * pulseScale, pulseSpeed);

            // Return to normal
            yield return ScaleTo(originalScale, pulseSpeed);

            // Second smaller beat
            yield return ScaleTo(originalScale * (pulseScale * 0.9f), pulseSpeed);

            // Return again
            yield return ScaleTo(originalScale, pulseSpeed);

            // Rest before next heartbeat
            yield return new WaitForSeconds(restTime);
        }
    }

    IEnumerator ScaleTo(Vector3 target, float speed)
    {
        Vector3 start = transform.localScale;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * speed;
            transform.localScale = Vector3.Lerp(start, target, time);
            yield return null;
        }

        transform.localScale = target;
    }
}
