using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public TowerData data;
    public GameObject rangeIndicator;

    private float fireCooldown = 0f;
    private SpriteRenderer spriteRenderer;
    private static Tower selectedTower;

    void Awake()
    {
        SetSelected(false); // hidden by default
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

    public void Initialize(TowerData newData)
    {
        data = newData;
        if (spriteRenderer != null && data.towerSprite != null)
        {
            spriteRenderer.sprite = data.towerSprite;
        }
        UpdateRangeIndicator();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(selected);
        }
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

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
        if (data.isSupport)
        {
            
        }
        else if (data.isDebuff)
        {
            // for now assume every debuff tower is single target
            GameObject proj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            p.Initialize(target.transform.position, data.damage, data.projectileSpeed);
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