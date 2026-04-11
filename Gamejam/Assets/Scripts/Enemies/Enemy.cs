using UnityEngine;
using Unity;

public class Enemy : MonoBehaviour
{
    private float maxHP = 10f;
    private float currHP = 10f;
    private float speed = 3f;
    private float speedMultiplier = 1.5f;
    private float damage = 10f;
    private int currentWaypoint = 0;

    [Header("References")]
    public Transform[] waypoints;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // move the enemy
        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[currentWaypoint].position,
            speed * speedMultiplier * Time.deltaTime
        );
        
        if (Vector2.Distance(transform.position, waypoints[currentWaypoint].position) < 0.01f && currentWaypoint < waypoints.Length - 1)
        {
            currentWaypoint++;
        }
        else if (currentWaypoint >= waypoints.Length - 1)
        {
            Debug.Log("I kill you");
        }
    }

    public void TakeDamage(float damage) 
    {
        // take damage then check if we're dead
        currHP -= damage;
        if (currHP <= 0){
            OnDeath();
        }
    }

    private void OnHit() 
    {
        // what to do when the enemy gets hit (could play special animations or audio)
    }

    private void OnDeath() 
    {
        // same as OnHit but on death instead
        Destroy(gameObject);
    }

    private void OnReachedEnd()
    {
        Destroy(gameObject);
    }
}
