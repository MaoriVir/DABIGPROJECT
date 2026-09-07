using UnityEngine;

public class lightningProjectilePlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Push the projectile forward based on its right-facing direction
        rb.linearVelocity = transform.right * speed;

        // Destroy the projectile automatically after lifeTime seconds to prevent clutter
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for Enemy 1
        Enemy1Script enemy1 = collision.GetComponent<Enemy1Script>();
        if (enemy1 != null)
        {
            enemy1.TakeDamage(damage);
            Destroy(gameObject); // Destroy projectile on hit
            return;
        }

        // Check for Enemy 2
        Enemy2Script enemy2 = collision.GetComponent<Enemy2Script>();
        if (enemy2 != null)
        {
            enemy2.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Check for Ship Spawner
        ShipSpawner spawner = collision.GetComponent<ShipSpawner>();
        if (spawner != null)
        {
            spawner.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}

