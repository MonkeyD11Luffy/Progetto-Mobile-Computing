using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SplitterController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.2f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 5;

    [Header("Danno da contatto")]
    [SerializeField] private int contactDamage = 1;

    [Header("Divisione")]
    [SerializeField] private GameObject splitPrefab;   // il pezzo piccolo generato alla morte
    [SerializeField] private int splitCount = 2;
    [SerializeField] private float splitSpread = 0.6f; // distanza dei figli dal punto di morte

    private Rigidbody2D rb;
    private Transform player;
    private int currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(contactDamage);
            }
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Split();

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemyDeath();
        }

        Destroy(gameObject);
    }

    private void Split()
    {
        if (splitPrefab == null || splitCount <= 0) return;

        for (int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * splitSpread;

            Instantiate(splitPrefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemySpawn(splitCount);
        }
    }
}
