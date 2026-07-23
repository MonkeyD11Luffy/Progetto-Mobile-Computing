using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MinibossController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float pauseDuration = 1f;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private int projectilesPerBurst = 5;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 8;

    private Rigidbody2D rb;
    private Transform player;
    private int currentHealth;

    private enum BossState { Moving, Pausing, Attacking }
    private BossState currentState;
    private float stateTimer;

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

        currentState = BossState.Moving;
        stateTimer = moveDuration;
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            switch (currentState)
            {
                case BossState.Moving:
                    currentState = BossState.Pausing;
                    stateTimer = pauseDuration;
                    rb.linearVelocity = Vector2.zero;
                    break;

                case BossState.Pausing:
                    FireBurst();
                    currentState = BossState.Moving;
                    stateTimer = moveDuration;
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentState == BossState.Moving && player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    private void FireBurst()
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < projectilesPerBurst; i++)
        {
            float angle = (360f / projectilesPerBurst) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }

            float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, rotAngle);
        }
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
                playerController.TakeDamage(1);
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
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemyDeath();
            RoomManager.Instance.SpawnRandomPermanentUpgrade(transform.position, transform.parent.gameObject);
        }

        Destroy(gameObject);
    }
}