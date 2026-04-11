using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float maxLifetime = 5f;
    private float enemySpeedDebuff = 1f;
    private float enemyDebuffTime = .5f;

    private Vector2 direction;
    private float damage;
    private float lifetime = 0f;

    public void Initialize(Vector2 targetPosition, float damage, float projectileSpeed, float enemySpeedDebuff = 1f, float enemyDebuffTime = .5f)
    {
        this.damage = damage;
        this.speed = projectileSpeed;
        this.enemySpeedDebuff = enemySpeedDebuff;
        this.enemyDebuffTime = enemyDebuffTime;
        direction = ((Vector2)(targetPosition - (Vector2)transform.position)).normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            // debuffs
            if (enemySpeedDebuff < 1f)
            {
                enemy.Debuff(enemySpeedDebuff, enemyDebuffTime);
            }
            Destroy(gameObject);
        }
    }
}