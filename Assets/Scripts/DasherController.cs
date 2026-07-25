using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DasherController : MonoBehaviour
{
    [Header("Tempi")]
    [SerializeField] private float idleDuration = 1.5f;
    [SerializeField] private float telegraphDuration = 0.6f;
    [SerializeField] private float dashDuration = 0.4f;


    [Header("Scatto")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float idleMoveSpeed = 1f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 3;

    [Header("Danno da contatto")]
    [SerializeField] private int contactDamage = 1;

    [Header("Telegrafo")]
    [SerializeField] private Color telegraphColor = Color.white;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private int currentHealth;
    private bool isDead = false;
    private enum DasherState { Idle, Telegraph, Dashing }
    private DasherState currentState;
    private float stateTimer;
    private Vector2 dashDirection;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        currentState = DasherState.Idle;
        stateTimer = idleDuration;
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f) return;

        switch (currentState)
        {
            case DasherState.Idle:
                StartTelegraph();
                break;

            case DasherState.Telegraph:
                StartDash();
                break;

            case DasherState.Dashing:
                StopDash();
                break;
        }
    }

    private void FixedUpdate()
{
    if (currentState == DasherState.Dashing) return;

    if (currentState == DasherState.Idle && player != null)
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * idleMoveSpeed;
    }
    else
    {
        rb.linearVelocity = Vector2.zero; // fermo durante il telegrafo
    }
}

    private void StartTelegraph()
    {
        currentState = DasherState.Telegraph;
        stateTimer = telegraphDuration;
        rb.linearVelocity = Vector2.zero;

        // Mira verso il player e "punta" la direzione, che resta fissa durante lo scatto
        if (player != null)
        {
            dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        if (sr != null)
        {
            sr.color = telegraphColor;
        }
    }

    private void StartDash()
    {
        currentState = DasherState.Dashing;
        stateTimer = dashDuration;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

    private void StopDash()
    {
        currentState = DasherState.Idle;
        stateTimer = idleDuration;
        rb.linearVelocity = Vector2.zero;
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
    if (isDead) return;

    currentHealth -= amount;

    if (currentHealth <= 0)
    {
        isDead = true;
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
