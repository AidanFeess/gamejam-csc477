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
    [Header("Player Stats")]
    public float maxHP = 100f;
    public float maxSpeed = 2f;

    [Header("References")]
    public TextMeshProUGUI healthText;
    public EnemySpawner spawner;

    [Header("Custom Waves")]
    public Wave[] waves;

    [Header("Infinite Waves")]
    public GameObject[] enemyPool;
    public int waveIncrease = 20; // how many units to add to the next wave

    // Player live stats    
    private float currentSpeed = 1f;
    private float currHP = 100f;

    // Game values
    private int currentWaveIndex = 0;
    private bool waveInProgress = false;

    void Start()
    {
        currHP = maxHP;
        UpdateHealthUI();
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = math.clamp(speed, 0, maxSpeed);
        Time.timeScale = currentSpeed;
    }

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
        while (currHP > 0)
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

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currHP}";
        }
    }
}