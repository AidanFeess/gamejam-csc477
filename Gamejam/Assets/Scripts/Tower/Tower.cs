using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public TowerData data;
    public GameObject rangeIndicator;

    private float fireCooldown = 0f;
    private SpriteRenderer spriteRenderer;
    private static Tower selectedTower;

    [Header("Effects")]
    public GameObject sleepEffectPrefab;
    public GameObject priestAttackEffectPrefab;
    public float priestEffectDuration = 0.5f;

    private GameObject activePriestEffect;
    private float priestEffectTimer = 0f;
    private GameObject activeEffect;

    public int GetSellValue()
    {
        return data.sellValue;
    }
    public void Sell()
    {
        GameController.Instance.TryTransaction(data.sellValue);
        Destroy(gameObject);
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetSelected(false);
    }

    private void UpdateRangeIndicator()
    {
        if (rangeIndicator != null && data != null)
        {
            RangeIndicator indicator = rangeIndicator.GetComponent<RangeIndicator>();
            if (indicator != null)
            {
                indicator.SetRadius(data.range);
            }
        }
    }

    public void Initialize(TowerData newData, bool isGhost = false)
    {
        data = newData;
        if (spriteRenderer != null && data.towerSprite != null)
        {
            spriteRenderer.sprite = data.towerSprite;
        }
        UpdateRangeIndicator();
        SetSelected(false);

        if (!isGhost && data.towerName == "Sleep Tower" && sleepEffectPrefab != null)
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            activeEffect = Instantiate(sleepEffectPrefab, transform.position + offset, Quaternion.identity);
            activeEffect.transform.SetParent(transform);
        }
    }

    public void SetSelected(bool selected)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(selected);
        }
    }

    public void SetPriestEffect(GameObject effect)
    {
        activePriestEffect = effect;
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        // hide the priest attack effect after its duration expires
        if (activePriestEffect != null && activePriestEffect.activeSelf)
        {
            priestEffectTimer -= Time.deltaTime;
            if (priestEffectTimer <= 0f)
            {
                activePriestEffect.SetActive(false);
            }
        }

        Enemy target = FindTarget();
        if (target != null && fireCooldown <= 0f)
        {
            Shoot(target);
            fireCooldown = 1f / data.fireRate;
        }
    }

    // Picks the farthest target along the track
    private Enemy FindTarget()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        Enemy best = null;
        float bestProgress = float.MinValue;

        foreach (GameObject obj in enemyObjects)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist > data.range) continue;

            Enemy e = obj.GetComponent<Enemy>();
            if (e == null) continue;

            float progress = e.GetPathProgress();
            if (progress > bestProgress)
            {
                best = e;
                bestProgress = progress;
            }
        }
        return best;
    }

    private List<Enemy> FindAllTargetsInRange()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach (GameObject obj in enemyObjects)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist > data.range) continue;

            Enemy e = obj.GetComponent<Enemy>();
            if (e == null) continue;

            enemiesInRange.Add(e);
        }
        return enemiesInRange;
    }

    private void Shoot(Enemy target)
    {
        // show priest attack effect if this tower has one
        if (activePriestEffect != null)
        {
            activePriestEffect.SetActive(true);
            priestEffectTimer = priestEffectDuration;

            // restart the animation from the beginning each time the tower fires
            Animator effectAnim = activePriestEffect.GetComponent<Animator>();
            if (effectAnim != null)
            {
                effectAnim.Play(0, -1, 0f);
            }
        }

        if (data.isSupport)
        {
            
        }
        else if (data.isDebuff)
        {
            // for now assume every debuff tower is single target
            GameObject proj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            p.Initialize(target.transform.position, data.damage, data.projectileSpeed, data.speedDebuff, data.debuffTime);
        }
        else
        {
            // Attack type behavior
            if (!data.isAOE)
            {
                GameObject proj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
                Projectile p = proj.GetComponent<Projectile>();
                p.Initialize(target.transform.position, data.damage, data.projectileSpeed);
            }
            else // isAOE
            {
                List<Enemy> nearbyEnemies = FindAllTargetsInRange();
                foreach (Enemy enemy in nearbyEnemies)
                {
                    enemy.TakeDamage(data.damage);
                }
            }
        }
    }
}