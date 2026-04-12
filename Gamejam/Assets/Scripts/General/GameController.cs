using TMPro;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

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

[System.Serializable]
public class ButtonSpriteSet
{
    public Sprite normal;
    public Sprite highlighted;
    public Sprite pressed;
}

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
    public TextMeshProUGUI waveText;
    public EnemySpawner spawner;

    [Header("Speed Button")]
    public Button speedButton;
    public ButtonSpriteSet standardSpeedSprites;
    public ButtonSpriteSet fastForwardSprites;

    [Header("Wave Button")]
    public Button waveButton;
    public ButtonSpriteSet nextWaveSprites;
    public ButtonSpriteSet autoAdvanceSprites;

    [Header("Custom Waves")]
    public Wave[] waves;

    [Header("Infinite Waves")]
    public GameObject[] enemyPool;
    public int waveIncrease = 20;

    // Player live stats
    private float currentSpeed = 1f;
    private float currentHP = 100f;
    private int currentCash = 60;

    // Wave state
    private int currentWaveIndex = 0;
    private bool waveInProgress = false;
    private bool autoAdvance = false;
    private Coroutine activeWaveRoutine;

    // Speed state
    private bool isFastForward = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentHP = maxHP;
        currentCash = startingCash;
        UpdateHealthUI();
        UpdateMoneyUI();
        UpdateWaveUI();
        UpdateSpeedButtonSprites();
        UpdateWaveButtonSprites();
    }

    // ───── Speed ─────

    public void ToggleSpeed()
    {
        isFastForward = !isFastForward;
        currentSpeed = isFastForward ? 2f : 1f;
        Time.timeScale = currentSpeed;
        UpdateSpeedButtonSprites();
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = math.clamp(speed, 0, maxSpeed);
        Time.timeScale = currentSpeed;
        isFastForward = currentSpeed >= 2f;
        UpdateSpeedButtonSprites();
    }

    private void UpdateSpeedButtonSprites()
    {
        if (speedButton == null) return;
        ApplySpriteSet(speedButton, isFastForward ? fastForwardSprites : standardSpeedSprites);
    }

    // ───── Waves ─────

    // Called by the wave button's OnClick.
    // In manual mode: starts the next wave.
    // In auto mode: toggles auto off (so clicking it again acts as a pause).
    public void OnWaveButtonClicked()
    {
        if (autoAdvance)
        {
            // click 3: turn auto off
            autoAdvance = false;
            UpdateWaveButtonSprites();
        }
        else if (waveInProgress)
        {
            // click 2: wave is running, enable auto so the next one chains
            autoAdvance = true;
            UpdateWaveButtonSprites();
        }
        else
        {
            // click 1: no wave running, start the next one
            StartNextWave();
        }
    }

    // Toggle between manual and auto-advance. Wire to a secondary button or keypress.
    public void ToggleAutoAdvance()
    {
        autoAdvance = !autoAdvance;
        UpdateWaveButtonSprites();

        if (autoAdvance && !waveInProgress && currentHP > 0)
        {
            StartNextWave();
        }
    }

    private void StartNextWave()
    {
        if (waveInProgress) return;
        if (currentHP <= 0) return;

        Wave wave = GetNextWave();
        activeWaveRoutine = StartCoroutine(RunSingleWave(wave));
        UpdateWaveUI();
    }

    private IEnumerator RunSingleWave(Wave wave)
    {
        waveInProgress = true;
        yield return StartCoroutine(SpawnWave(wave));
        waveInProgress = false;

        currentWaveIndex++;

        if (autoAdvance && currentHP > 0)
        {
            yield return new WaitForSeconds(wave.timeBeforeNextWave);
            StartNextWave();
        }
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
        return GenerateWave(currentWaveIndex);
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

    private void UpdateWaveButtonSprites()
    {
        if (waveButton == null) return;
        ApplySpriteSet(waveButton, autoAdvance ? autoAdvanceSprites : nextWaveSprites);
    }

    // ───── Button Sprite Helper ─────

    private void ApplySpriteSet(Button button, ButtonSpriteSet set)
    {
        if (set == null) return;

        // set the "resting" sprite on the Image
        Image img = button.GetComponent<Image>();
        if (img != null && set.normal != null)
        {
            img.sprite = set.normal;
        }

        // update the Sprite Swap transition's highlighted/pressed sprites
        SpriteState state = button.spriteState;
        if (set.highlighted != null) state.highlightedSprite = set.highlighted;
        if (set.pressed != null) state.pressedSprite = set.pressed;
        button.spriteState = state;
    }

    // ───── Player Stats ─────

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        UpdateHealthUI();
        if (currentHP <= 0)
        {
            Debug.Log("Game over");
            Time.timeScale = 0f;
        }
    }

    public bool CanAfford(int amount)
    {
        return (currentCash - amount) >= 0;
    }

    public bool TryTransaction(int amount)
    {
        int transactCash = currentCash + amount;
        if (transactCash < 0) return false;
        currentCash = transactCash;
        UpdateMoneyUI();
        return true;
    }

    // ───── UI ─────

    private void UpdateHealthUI()
    {
        if (healthText != null) healthText.text = $"HP: {currentHP}";
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null) moneyText.text = $"Neurons: {currentCash}";
    }

    private void UpdateWaveUI()
    {
        if (waveText != null) waveText.text = $"Wave: {currentWaveIndex+1}";
    }
}