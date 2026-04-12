using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 15f;
    public float maxLifetime = 5f;
    [Header("Rotation")]
    public bool facesTarget = true;
    public float rotationOffset = 0f;

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
        if (facesTarget)
        {
            FaceDirection(direction);
        }
    }

    private void FaceDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);
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