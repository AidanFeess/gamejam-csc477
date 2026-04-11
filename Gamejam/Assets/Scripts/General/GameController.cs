using System;
using TMPro;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

// Wave stuff
[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public float delayBeforeGroup = 1f;
    public int count;
    public float spawnInterval = 1f;
}

[System.Serializable]
public class Wave
{
    public EnemyGroup[] groups;
    public float timeBeforeNextWave = 5f;
}

// Main game controller stuff
public class GameController : MonoBehaviour
{

    public static GameController Instance { get; private set; }

    [Header("Player Stats")]
    public float maxHP = 100f;
    public float maxSpeed = 2f;
    public int startingCash = 60;

    [Header("References")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;
    public EnemySpawner spawner;

    [Header("Custom Waves")]
    public Wave[] waves;

    [Header("Infinite Waves")]
    public GameObject[] enemyPool;
    public int waveIncrease = 20; // how many units to add to the next wave

    // Player live stats    
    private float currentSpeed = 1f;
    private float currentHP = 100f;
    private int currentCash = 60;

    // Game values
    private int currentWaveIndex = 0;
    private bool waveInProgress = false;

    void Start()
    {
        currentHP = maxHP;
        currentCash = startingCash;
        UpdateHealthUI();
        UpdateMoneyUI();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Speed

    public void SetSpeed(float speed)
    {
        currentSpeed = math.clamp(speed, 0, maxSpeed);
        Time.timeScale = currentSpeed;
    }

    // Waves

    private IEnumerator SpawnWave(Wave wave)
    {
        foreach (EnemyGroup group in wave.groups)
        {
            yield return new WaitForSeconds(group.delayBeforeGroup);

            for (int i = 0; i < group.count; i++)
            {
                spawner.SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }
    }

    private Wave GetNextWave()
    {
        if (currentWaveIndex < waves.Length)
        {
            return waves[currentWaveIndex];
        }

        return GenerateWave(currentWaveIndex); // infinite waves past the initial 10
    }

    private Wave GenerateWave(int waveNumber)
    {
        int wavesPast = waveNumber - waves.Length + 1;

        Wave wave = new Wave();
        int groupCount = UnityEngine.Random.Range(2, 5);
        wave.groups = new EnemyGroup[groupCount];

        for (int i = 0; i < groupCount; i++)
        {
            EnemyGroup group = new EnemyGroup();
            group.enemyPrefab = enemyPool[UnityEngine.Random.Range(0, enemyPool.Length)];
            group.count = 3 + (wavesPast * waveIncrease / 10);
            group.spawnInterval = 1f;
            group.delayBeforeGroup = i == 0 ? 0f : 1f;
            wave.groups[i] = group;
        }

        wave.timeBeforeNextWave = 5f;
        return wave;
    }

    private IEnumerator RunWaves()
    {
        while (currentHP > 0)
        {
            Wave wave = GetNextWave();
            waveInProgress = true;
            yield return StartCoroutine(SpawnWave(wave));
            waveInProgress = false;

            yield return new WaitForSeconds(wave.timeBeforeNextWave);
            currentWaveIndex++;
        }
    }

    public void StartGame()
    {
        StartCoroutine(RunWaves());
    }

    // Public Stats Interfaces
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        UpdateHealthUI();
        if (currentHP <= 0)
        {
            // TODO: implement game over;
            Debug.Log("Game over");
            Time.timeScale = 0f;
        }
    }

    public bool TryTransaction(int amount)
    {
        int transactCash = currentCash + amount; // as long as amount is negative for losing money, it should work
        if (transactCash < 0) {
            return false;
        } 
        else
        {
            currentCash = transactCash;
            UpdateMoneyUI();
            return true;
        }
    }

    // UI
    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHP}";
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Neurons: {currentCash}";
        }
    }
}