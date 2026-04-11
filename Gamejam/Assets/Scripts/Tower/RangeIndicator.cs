using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeIndicator : MonoBehaviour
{
    public int segments = 64;
    public float width = 0.05f;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.startWidth = width;
        line.endWidth = width;
        line.positionCount = segments;
    }

    public void SetRadius(float radius)
    {
        if (line == null) line = GetComponent<LineRenderer>();
        line.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
        }
    }
}