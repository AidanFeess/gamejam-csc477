using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

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

    [Header("Temporary Effects")]
    public float tempRangeBuff = 1f;
    public float tempAttackSpeedBuff = 1f;

    [Header("Visual")]
    public Slider masterySlider;
    public GameObject masteryRoot; // before any mastery is gained

    [Header("Mastery")]
    public float effectiveDamage => data.damage * (1f + mastery * data.damagePerMastery);
    public float effectiveRange => tempRangeBuff * data.range * (1f + mastery * data.rangePerMastery);
    public float effectiveFireRate => tempAttackSpeedBuff * data.fireRate * (1f + mastery * data.fireRatePerMastery);


    private GameObject activePriestEffect;
    private float priestEffectTimer = 0f;
    private GameObject activeEffect;
    private bool isGhost;
    private List<Tower> lastBuffedTowers = new List<Tower>();
    private float mastery = 0f;

    public int GetSellValue()
    {
        return data.sellValue;
    }
    public void Sell()
    {
        GameController.Instance.TryTransaction(data.sellValue);
        Destroy(gameObject);
    }
    public void AddMasteryFromKill()
    {
        mastery = Mathf.Min(mastery + data.masteryPerKill, data.masteryCap);
        masterySlider.value = mastery / data.masteryCap;
        masteryRoot.SetActive(mastery > 0);
        GameController.Instance.AddMastery(data.masteryPerKill);
    }
    private void AddMasteryFromShot()
    {
        mastery = Mathf.Min(mastery + data.masteryPerShot, data.masteryCap);
        masterySlider.value = mastery / data.masteryCap;
        masteryRoot.SetActive(mastery > 0);
        GameController.Instance.AddMastery(data.masteryPerShot);
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
                indicator.SetRadius(effectiveRange);
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

        this.isGhost = isGhost;

        if (!isGhost && data.towerName == "Sleep Tower" && sleepEffectPrefab != null)
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            activeEffect = Instantiate(sleepEffectPrefab, transform.position + offset, Quaternion.identity);
            activeEffect.transform.SetParent(transform);
        }
    }

    public void UnghostTower()
    {
        isGhost = false;
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
        UpdateRangeIndicator(); // for buffs
        if (isGhost) return;
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

        if (!data.isSupport)
        {
            Enemy target = FindTarget();
            if (target != null && fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1 / (effectiveFireRate);
            }
        } 
        else
        {
            Tower target = FindTowerTarget();
            if (target != null && fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1 / (effectiveFireRate);
            }
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
            if (dist > effectiveRange) continue;

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

    private Tower FindTowerTarget()
    {
        GameObject[] towerObjects = GameObject.FindGameObjectsWithTag("Tower");

        foreach (GameObject obj in towerObjects)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist > effectiveRange) continue;
            return obj.GetComponent<Tower>();
        }

        return null;
    }

    private List<Enemy> FindAllTargetsInRange()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach (GameObject obj in enemyObjects)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist > effectiveRange) continue;

            Enemy e = obj.GetComponent<Enemy>();
            if (e == null) continue;

            enemiesInRange.Add(e);
        }
        return enemiesInRange;
    }

    private List<Tower> FindAllTowersInRange()
    {
        GameObject[] towerObjects = GameObject.FindGameObjectsWithTag("Tower");
        List<Tower> towersInRange = new List<Tower>();

        foreach (GameObject obj in towerObjects)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist > effectiveRange) continue;

            Tower t = obj.GetComponent<Tower>();
            if (t == null || t == this) continue;

            towersInRange.Add(t);
        }
        return towersInRange;
    }

    private void Shoot(MonoBehaviour target)
    {
        AddMasteryFromShot();
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

        Vector3 targetPos = target.transform.position;

        if (data.isSupport)
        {
            if (!data.isAOE)
            {
                // doesn't exist
            }
            else
            {
                List<Tower> nearbyTowers = FindAllTowersInRange();
                foreach (Tower tower in nearbyTowers)
                {
                    tower.tempAttackSpeedBuff = data.attackRangeBuff;
                    tower.tempRangeBuff = data.attackRangeBuff;

                    // sort out towers that were just buffed
                    if (tower != null && lastBuffedTowers.Contains(tower))
                    {
                        print("Tower found in lastbuffedtowers");
                        lastBuffedTowers.Remove(tower);
                    }
                }

                // cleanup old towers no longer in range (ghosts for example)
                foreach (Tower tower in lastBuffedTowers)
                {
                    tower.tempAttackSpeedBuff = 1;
                    tower.tempRangeBuff = 1;
                }

                // list resets at the end
                lastBuffedTowers = new List<Tower>(nearbyTowers);
            }
        }
        else if (data.isDebuff)
        {
            GameObject proj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            p.Initialize(targetPos, effectiveDamage, data.projectileSpeed, this, data.speedDebuff, data.debuffTime);
        }
        else
        {
            if (!data.isAOE)
            {
                GameObject proj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
                Projectile p = proj.GetComponent<Projectile>();
                p.Initialize(targetPos, effectiveDamage, data.projectileSpeed, this);
            }
            else
            {
                List<Enemy> nearbyEnemies = FindAllTargetsInRange();
                foreach (Enemy enemy in nearbyEnemies)
                {
                    enemy.TakeDamage(effectiveDamage, this);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // clean up buffs, kinda shit implementation but whatever
        if (!isGhost)
        {
           foreach (Tower tower in FindAllTowersInRange())
            {
                tower.tempAttackSpeedBuff = 1;
                tower.tempRangeBuff = 1;
            } 
        } 
    }
}