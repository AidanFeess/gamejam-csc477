using TMPro;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    [Header("Boss Waves")]
    public GameObject bossPrefab;
    public int baseBossCount = 2;

    [Header("Unlockables")]
    public TowerData[] allTowers;
    public GameObject[] allEnemies;

    [Header("Tower UI Buttons")]
    public Button[] towerButtons;

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
        UpdateCashUI();
        UpdateWaveUI();
        UpdateSpeedButtonSprites();
        UpdateWaveButtonSprites();

        if (allTowers != null)
        {
            for (int i = 0; i < allTowers.Length; i++)
            {
                TowerData tower = allTowers[i];
                if (tower == null) continue;
                tower.isLocked = tower.unlockWave > 1;

                if (!tower.isLocked)
                {
                    UnlockTowerButton(i);
                }
            }
        }

        SetPriceIndicators();
        CheckUnlocks(1);
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
           if (Keyboard.current.f12Key.wasPressedThisFrame)
            {
                UnlockAllTowers();
            }
            else if (Keyboard.current.f11Key.wasPressedThisFrame)
            {
                MoneyBags();
            } 
        }
        
    }
    
    // ───── Cheats ─────

    private void UnlockAllTowers()
    {
        if (allTowers == null) return;

        for (int i = 0; i < allTowers.Length; i++)
        {
            TowerData tower = allTowers[i];
            if (tower == null || !tower.isLocked) continue;

            tower.isLocked = false;
            UnlockTowerButton(i);
        }

        Debug.Log("[Dev] All towers unlocked");
    }

    private void MoneyBags()
    {
        currentCash = 99999;
        UpdateCashUI();

        Debug.Log("[Dev] Cash set to 99999");
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

    private void SetPriceIndicators()
    {
        if (allTowers == null || towerButtons == null) return;

        for (int i = 0; i < allTowers.Length && i < towerButtons.Length; i++)
        {
            TowerData tower = allTowers[i];
            Button button = towerButtons[i];
            if (tower == null || button == null) continue;

            TextMeshProUGUI priceText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = tower.cost.ToString();
            }
        }
    }

    private void CheckUnlocks(int waveNumber)
    {
        // check towers
        if (allTowers != null)
        {
            for (int i = 0; i < allTowers.Length; i++)
            {
                TowerData tower = allTowers[i];
                if (tower != null && tower.unlockWave == waveNumber)
                {
                    if (tower.isLocked)
                    {
                        tower.isLocked = false;
                        UnlockTowerButton(i);
                    }
                    InfoScreen.Instance?.Show(tower.infoDescription, tower.infoSprite);
                    return;
                }
            }
        }

        // check enemies
        if (allEnemies != null)
        {
            foreach (GameObject enemyPrefab in allEnemies)
            {
                if (enemyPrefab == null) continue;
                Enemy enemyScript = enemyPrefab.GetComponent<Enemy>();
                if (enemyScript != null && enemyScript.unlockWave == waveNumber)
                {
                    InfoScreen.Instance?.Show(enemyScript.infoDescription, enemyScript.infoSprite);
                    return;
                }
            }
        }
    }

    private void UnlockTowerButton(int index)
    {
        if (towerButtons == null || index < 0 || index >= towerButtons.Length) return;
        Button button = towerButtons[index];
        if (button == null) return;

        // reset button image color to white
        Image img = button.GetComponent<Image>();
        if (img != null) img.color = Color.white;

        // destroy the LockIcon child if it exists
        Transform lockIcon = button.transform.Find("LockIcon");
        if (lockIcon != null) Destroy(lockIcon.gameObject);
    }

    private IEnumerator RunSingleWave(Wave wave)
    {
        waveInProgress = true;
        yield return StartCoroutine(SpawnWave(wave));

        // wait until all enemies from this wave are gone
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return null;
        }

        waveInProgress = false;
        currentWaveIndex++;
        UpdateWaveUI();

        CheckUnlocks(currentWaveIndex + 1);

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

        bool isBossWave = wavesPast > 0 && wavesPast % 10 == 0;

        Wave wave = new Wave();

        int groupCount = UnityEngine.Random.Range(2, 5);
        int totalGroups = isBossWave && bossPrefab != null ? groupCount + 1 : groupCount;
        wave.groups = new EnemyGroup[totalGroups];

        for (int i = 0; i < groupCount; i++)
        {
            EnemyGroup group = new EnemyGroup();
            group.enemyPrefab = enemyPool[UnityEngine.Random.Range(0, enemyPool.Length)];
            group.count = 3 + (wavesPast * waveIncrease / 10);
            group.spawnInterval = 1f;
            group.delayBeforeGroup = i == 0 ? 0f : 1f;
            wave.groups[i] = group;
        }

        if (isBossWave && bossPrefab != null)
        {
            int bossWaveNumber = wavesPast / 10;
            int bossCount = baseBossCount * (int)Mathf.Pow(2, bossWaveNumber - 1);

            EnemyGroup bossGroup = new EnemyGroup();
            bossGroup.enemyPrefab = bossPrefab;
            bossGroup.count = bossCount;
            bossGroup.spawnInterval = 2f;
            bossGroup.delayBeforeGroup = 2f;
            wave.groups[totalGroups - 1] = bossGroup;
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
        UpdateCashUI();
        return true;
    }

    // ───── UI ─────

    private void UpdateHealthUI()
    {
        if (healthText != null) healthText.text = $"      {currentHP}";
    }

    private void UpdateCashUI()
    {
        if (moneyText != null) moneyText.text = $"      {currentCash}";
    }

    private void UpdateWaveUI()
    {
        if (waveText != null) waveText.text = $"Wave: {currentWaveIndex+1}";
    }
}