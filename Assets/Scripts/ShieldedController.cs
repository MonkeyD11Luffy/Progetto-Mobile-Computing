using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShieldedController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.3f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 3;

    [Header("Danno da contatto")]
    [SerializeField] private int contactDamage = 1;

    [Header("Scudo")]
    [SerializeField] private Transform shieldVisual;    // il "pezzo" grafico dello scudo, figlio di questo oggetto
    [SerializeField] private float shieldArc = 0.3f;    // quanto è ampio il cono protetto (0 = mezzo cerchio, valori più alti = cono più stretto)
    [SerializeField] private float shieldRotationSpeed = 80f; // gradi al secondo

    private Rigidbody2D rb;
    private Transform player;
    private int currentHealth;
    private Vector2 shieldDirection = Vector2.right;

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

   private void Update()
{
    if (player == null) return;

    Vector2 targetDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;

    // Lo scudo ruota gradualmente verso il player, non istantaneamente:
    // così è possibile aggirarlo muovendosi abbastanza in fretta
    shieldDirection = Vector3.RotateTowards(
        shieldDirection,
        targetDirection,
        shieldRotationSpeed * Mathf.Deg2Rad * Time.deltaTime,
        0f
    );

    if (shieldVisual != null)
    {
        float angle = Mathf.Atan2(shieldDirection.y, shieldDirection.x) * Mathf.Rad2Deg;
        shieldVisual.rotation = Quaternion.Euler(0, 0, angle);
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

    // Chiamato via SendMessage da ProjectileController e BombController
    private void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Il proiettile chiede se il colpo è stato parato, prima di infliggere danno
    public bool IsBlocked(Vector2 hitPosition)
    {
        Vector2 fromEnemyToHit = (hitPosition - (Vector2)transform.position).normalized;
        return Vector2.Dot(fromEnemyToHit, shieldDirection) > shieldArc;
    }

    private void Die()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemyDeath();
        }

        Destroy(gameObject);
    }
}
