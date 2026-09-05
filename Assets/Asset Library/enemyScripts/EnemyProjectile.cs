using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class EnemyProjectile : MonoBehaviour
{
   
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private float direction = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetupDirection(float faceDirection)
    {
        direction = faceDirection;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        
        if (direction > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Successfully finds your specific movement and health component
            PlayerMove1 playerMovement = other.GetComponent<PlayerMove1>();
            
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) 
        {
            Destroy(gameObject);
        }
    }
}
