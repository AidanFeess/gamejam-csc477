using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Player Stats")]
    public float maxHP = 100f;
    public float maxSpeed = 2f;

    [Header("References")]
    public TextMeshProUGUI HealthText;

    // Player live stats    
    private float currentSpeed = 1f;
    private float currHP = 100f;

    // Game data
    private int currentWave = 1;

    private void NextWave()
    {
        currentWave ++;
        // Call the SpawnWave function here
    }
}
