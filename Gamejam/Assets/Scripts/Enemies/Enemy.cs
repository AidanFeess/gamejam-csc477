using UnityEngine;
using System.Collections;

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
        currentHP = maxHP;
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

    public void TakeDamage(float damage) 
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            OnDeath();
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
        // TODO: what to do when the enemy gets hit
    }

    private void OnDeath() 
    {
        GameController.Instance.TryTransaction(worth);
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