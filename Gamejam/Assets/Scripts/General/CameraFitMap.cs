using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitMap : MonoBehaviour
{
    [Header("Map Size (world units)")]
    public float mapWidth = 20f;
    public float mapHeight = 8.74f;
 
    [Header("Map Center")]
    public Vector2 mapCenter = Vector2.zero;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        FitToMap();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            FitToMap();
        }
    }

    void FitToMap()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float screenAspect = (float)Screen.width / Screen.height;
        float mapAspect = mapWidth / mapHeight;

        if (screenAspect >= mapAspect)
        {
            cam.orthographicSize = mapHeight / 2f;
        }
        else
        {
            cam.orthographicSize = (mapWidth / screenAspect) / 2f;
        }

        transform.position = new Vector3(mapCenter.x, mapCenter.y, transform.position.z);
    }
}