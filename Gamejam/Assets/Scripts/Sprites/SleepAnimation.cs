using UnityEngine;

public class SleepAnimation : MonoBehaviour
{
    public float maxWait;
    private float waitTime = 1f;
    public GameObject ZZZPrefab;
    public Transform ZSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (waitTime <= 0)
        {
            Instantiate(ZZZPrefab, ZSpawn);
            waitTime = maxWait;
        }
        else {
            waitTime -= Time.deltaTime;
        }
    }
}
