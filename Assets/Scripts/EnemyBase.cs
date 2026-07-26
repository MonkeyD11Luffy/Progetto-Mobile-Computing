using UnityEngine;

// Classe base comune a tutti i nemici: vita, morte, danno da contatto,
// riferimento al player e utilità per sparare.
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Vita")]
    [SerializeField] protected int maxHealth = 3;

    [Header("Danno da contatto")]
    [SerializeField] protected int contactDamage = 1; // 0 = non fa danno toccando il player

    protected Rigidbody2D rb;
    protected Transform player;
    protected int currentHealth;
    protected bool isDead;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Pubblico: viene chiamato via SendMessage da ProjectileController e BombController
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    protected virtual void Die()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemyDeath();
        }

        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected void TryDamagePlayer(GameObject other)
    {
        if (isDead || contactDamage <= 0) return;
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(contactDamage);
        }
    }

    // Utilità condivisa: usata da Turret, Teleporter, Miniboss e Boss
    protected void SpawnProjectile(GameObject prefab, Vector2 origin, Vector2 direction, float speed)
    {
        if (prefab == null) return;

        GameObject projectile = Instantiate(prefab, origin, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
