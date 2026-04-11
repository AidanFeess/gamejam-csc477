using UnityEngine;

public class FloatingSprite : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobAmplitude = 0.5f;   // Height of up/down movement
    public float bobFrequency = 1f;     // Speed of bobbing

    [Header("Swing Settings")]
    public float swingAmplitude = 15f;  // Degrees of rotation
    public float swingFrequency = 1f;   // Speed of swinging

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float time = Time.time;

        // Bobbing (Y movement)
        float newY = startPosition.y + Mathf.Sin(time * bobFrequency) * bobAmplitude;

        // Apply position
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Swinging (Z rotation for 2D)
        float angle = Mathf.Sin(time * swingFrequency) * swingAmplitude;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
