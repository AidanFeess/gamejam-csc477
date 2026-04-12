using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 10f;
    public float speed = 3f;
    public float damage = 10f;
    public int worth = 5;
    public bool doesRotate = true;

    [Header("Unlock Info")]
    public int unlockWave = 0;
    public Sprite infoSprite;
    [TextArea(3, 6)] public string infoDescription;

    [Header("Animation")]
    public string animationStateName;

    [Header("References")]
    public Transform[] waypoints;

    [Header("HP Bar")]
    public UnityEngine.UI.Slider hpSlider;
    public GameObject hpBarRoot;
    public Vector3 hpBarOffset = new Vector3(0, 0.6f, 0);

    [Header("Sounds")]
    [SerializeField] private AudioClip damageSoundClip;
    [SerializeField] private AudioClip deathSoundClip;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Animator animator;
    private float currentHP;
    private float speedMultiplier = 1f;
    private int currentWaypoint = 0;
    private Vector3 spawnPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
    }

    void Start()
    {
        maxHP = maxHP * 1 + (math.clamp(GameController.Instance.GetCurrentWave() - 10, 0, 1000) / 10);
        currentHP = maxHP;
        speed = speed * 1 + (math.clamp(GameController.Instance.GetCurrentWave() - 10, 0, 1000) / 20);
        UpdateHPBar();
        if (waypoints != null && waypoints.Length > 0)
        {
            FaceWaypoint(waypoints[0]);
        }

        // play the animation if this enemy has one
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(animationStateName))
            {
                animator.enabled = true;
                animator.Play(animationStateName);
            }
            else
            {
                animator.enabled = false;
            }
        }
    }

    void Update()
    {
        // kill if at end
        if (currentWaypoint >= waypoints.Length)
        {
            OnReachedEnd();
            return;
        }
        // move the enemy
        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[currentWaypoint].position,
            speed * speedMultiplier * Time.deltaTime
        );
        
        if (Vector2.Distance(transform.position, waypoints[currentWaypoint].position) < 0.01f)
        {
            currentWaypoint++;
            if (currentWaypoint < waypoints.Length)
            {
                FaceWaypoint(waypoints[currentWaypoint]);
            }
        }
    }

    void LateUpdate()
    {
        if (hpBarRoot != null)
        {
            // pin position to the enemy's center plus a fixed world-space offset
            hpBarRoot.transform.position = transform.position + hpBarOffset;
            // keep rotation upright regardless of enemy rotation
            hpBarRoot.transform.rotation = Quaternion.identity;
        }
    }

    private void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }

        // hide bar at full HP
        if (hpBarRoot != null)
        {
            hpBarRoot.SetActive(currentHP < maxHP);
        }
    }

    public void TakeDamage(float damage, Tower attacker) 
    {
        currentHP -= damage;
        UpdateHPBar();
        if (currentHP <= 0)
        {
            OnDeath(attacker);
        } else
        {
            OnHit();
        }
    }

    public void Debuff(float speedDebuff, float debuffTime)
    {
        StartCoroutine(DebuffRoutine(speedDebuff, debuffTime));
    }

    private IEnumerator DebuffRoutine(float speedDebuff, float debuffTime)
    {
        float originalMultiplier = speedMultiplier;
        speedMultiplier = speedDebuff;

        yield return new WaitForSeconds(debuffTime);

        speedMultiplier = originalMultiplier;
    }

    private void OnHit() 
    {
        if (SoundFXManager.Instance != null && damageSoundClip != null)
        {
            SoundFXManager.Instance.PlaySoundFXClip(damageSoundClip, transform, 1f);
        }
        else
        {
            Debug.LogWarning("Enemy tried to play onhit sound, but SoundFXManager or damageSoundClip is missing!");
        }
    }

    private void OnDeath(Tower killer) 
    {
        if (killer != null)
        {
            killer.AddMasteryFromKill();
        }

        if (GameController.Instance != null)
        {
            GameController.Instance.TryTransaction(worth);
        }
        else
        {
            Debug.LogWarning("Enemy tried to give money, but GameController.Instance is missing!");
        }

        if (SoundFXManager.Instance != null && deathSoundClip != null) // death noise
        {
            SoundFXManager.Instance.PlaySoundFXClip(deathSoundClip, transform, 1f);
        }
        else
        {
            Debug.LogWarning("Enemy tried to play death sound, but SoundFXManager or deathSoundClip is missing!");
        }

        Destroy(gameObject);
    }

    private void OnReachedEnd()
    {
        GameController.Instance.TakeDamage(damage);
        Destroy(gameObject);
    }

    private void FaceWaypoint(Transform waypoint)
    {
        if (!doesRotate || waypoint == null) return;

        Vector2 direction = (Vector2)waypoint.position - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public float GetPathProgress()
    {
        if (waypoints == null || waypoints.Length == 0) return 0f;
        if (currentWaypoint >= waypoints.Length) return float.MaxValue;

        Vector3 prevPos = currentWaypoint == 0 
            ? spawnPosition 
            : waypoints[currentWaypoint - 1].position;
        Vector3 nextPos = waypoints[currentWaypoint].position;

        float segmentLength = Vector2.Distance(prevPos, nextPos);
        if (segmentLength <= 0) return currentWaypoint;

        float remaining = Vector2.Distance(transform.position, nextPos);
        float fractionRemaining = Mathf.Clamp01(remaining / segmentLength);

        return currentWaypoint + (1f - fractionRemaining);
    }

    public float GetHP()
    {
        return currentHP;
    }
}