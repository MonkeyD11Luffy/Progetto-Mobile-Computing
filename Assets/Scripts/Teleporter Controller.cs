using UnityEngine;

public class TeleporterController : MonoBehaviour
{
    [Header("Tempi")]
    [SerializeField] private float idleDuration = 1.8f;
    [SerializeField] private float vanishDuration = 0.4f;

    [Header("Teletrasporto")]
    [SerializeField] private float minDistanceFromPlayer = 2.5f;
    [SerializeField] private float maxDistanceFromPlayer = 4f;
    
    [Header("Confini stanza")]
    [SerializeField] private float roomHalfWidth = 3f;
    [SerializeField] private float roomHalfHeight = 3f;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 7f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 2;

    private SpriteRenderer sr;
    private Collider2D col;
    private Transform player;
    private int currentHealth;

    private enum TeleporterState { Idle, Vanishing }
    private TeleporterState currentState;
    private float stateTimer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        currentState = TeleporterState.Idle;
        stateTimer = idleDuration;
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f) return;

        switch (currentState)
        {
            case TeleporterState.Idle:
                StartVanish();
                break;

            case TeleporterState.Vanishing:
                Reappear();
                break;
        }
    }

    private void StartVanish()
    {
        currentState = TeleporterState.Vanishing;
        stateTimer = vanishDuration;

        SetVisible(false);
    }

    private void Reappear()
    {
        currentState = TeleporterState.Idle;
        stateTimer = idleDuration;

        MoveToRandomPositionNearPlayer();
        SetVisible(true);
        Fire();
    }

    private void SetVisible(bool visible)
    {
        if (sr != null) sr.enabled = visible;
        if (col != null) col.enabled = visible;
    }

   private void MoveToRandomPositionNearPlayer()
{
    if (player == null) return;

    // Centro della stanza: il genitore del Teleporter (es. Room_3)
    Vector2 roomCenter = transform.parent != null
        ? (Vector2)transform.parent.position
        : Vector2.zero;

    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

    Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    Vector2 targetPosition = (Vector2)player.position + offset;

    // Blocca la posizione dentro i confini della stanza
    targetPosition.x = Mathf.Clamp(targetPosition.x, roomCenter.x - roomHalfWidth, roomCenter.x + roomHalfWidth);
    targetPosition.y = Mathf.Clamp(targetPosition.y, roomCenter.y - roomHalfHeight, roomCenter.y + roomHalfHeight);

    transform.position = targetPosition;
}

    private void Fire()
    {
        if (projectilePrefab == null || player == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
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
        }

        Destroy(gameObject);
    }
}