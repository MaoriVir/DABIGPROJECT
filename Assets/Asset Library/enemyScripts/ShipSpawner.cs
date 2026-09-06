using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(FlashEffect))] // <-- NEW: Guarantees FlashEffect is on the object
public class ShipSpawner : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;

    [Header("Spawning Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 4f;

    [Header("Components")]
    [SerializeField] private SpriteRenderer spawnerRenderer;
    private FlashEffect flashEffect; // <-- NEW: Cached reference to your flash script

    private void Start()
    {
        currentHealth = maxHealth;

        // Auto-grab SpriteRenderer if left unassigned in the inspector
        if (spawnerRenderer == null)
        {
            spawnerRenderer = GetComponent<SpriteRenderer>();
        }

        // NEW: Auto-grab the FlashEffect script attached to this object
        flashEffect = GetComponent<FlashEffect>();

        // Start the automated check and spawn cycle loop
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait for the configured delay before attempting the next spawn
            yield return new WaitForSeconds(spawnInterval);

            // Only spawn an enemy if the spawner is actively seen by the main camera
            if (spawnerRenderer != null && spawnerRenderer.isVisible)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            // Spawns the enemy at the exact coordinate location of this ship spawner
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }

    // This method is called directly by your PlayerMove1 script during a slam collision
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health remaining: {currentHealth}");

        // NEW: Trigger the flash effect whenever damage is sustained
        if (flashEffect != null)
        {
            flashEffect.Flash();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // Instantly clears the spawner object out of your game scene
        Destroy(gameObject);
    }
}
