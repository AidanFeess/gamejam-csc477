using UnityEngine;
using Unity;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 10f;
    public float speed = 3f;
    public float damage = 10f;
    public int worth = 5;

    [Header("References")]
    public Transform[] waypoints;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float currentHP;
    private float speedMultiplier = 1f;
    private int currentWaypoint = 0;
    private Vector3 spawnPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        spawnPosition = transform.position;
    }

    void Start()
    {
        currentHP = maxHP;
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
        }
    }

    public void TakeDamage(float damage) 
    {
        // take damage then check if we're dead
        currentHP -= damage;
        if (currentHP <= 0){
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
        // TODO: what to do when the enemy gets hit (could play special animations or audio)
    }

    private void OnDeath() 
    {
        // same as OnHit but on death instead
        GameController.Instance.TryTransaction(worth);
        Destroy(gameObject);
    }

    private void OnReachedEnd()
    {
        GameController.Instance.TakeDamage(damage);
        Destroy(gameObject);
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

    private float NormalizedDistanceToNextWaypoint()
    {
        Transform next = waypoints[currentWaypoint];
        Transform prev = currentWaypoint == 0 ? null : waypoints[currentWaypoint - 1];

        if (prev == null)
        {
            // no previous waypoint, just use distance to next
            return Vector2.Distance(transform.position, next.position);
        }

        float segmentLength = Vector2.Distance(prev.position, next.position);
        float remaining = Vector2.Distance(transform.position, next.position);
        return segmentLength > 0 ? remaining / segmentLength : 0f;
    }
}
