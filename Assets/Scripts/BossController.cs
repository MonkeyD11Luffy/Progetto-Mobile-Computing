using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("Vita")]
    [SerializeField] private int maxHealth = 25;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float chargeSpeed = 9f;

    [Header("Tempi")]
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float telegraphDuration = 0.5f;
    [SerializeField] private float chargeDuration = 0.5f;

    [Header("Proiettili")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private int radialCount = 8;
    [SerializeField] private float volleySpread = 15f;

    [Header("Evocazione")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionCount = 2;

    [Header("Danno da contatto")]
    [SerializeField] private int contactDamage = 1;

    [Header("Colori fase")]
    [SerializeField] private Color phase2Color = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color phase3Color = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color telegraphColor = Color.white;

    private enum BossState { Moving, Telegraph, Charging }
    private enum BossAction { Radial, Volley, Charge, Summon }

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private int currentHealth;
    private bool isDead = false;

    private BossState currentState;
    private BossAction nextAction;
    private float stateTimer;
    private Vector2 chargeDirection;
    private Color phaseColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        if (sr != null) phaseColor = sr.color;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        currentState = BossState.Moving;
        stateTimer = moveDuration;
    }

    private int GetPhase()
    {
        float ratio = (float)currentHealth / maxHealth;
        if (ratio > 0.66f) return 1;
        if (ratio > 0.33f) return 2;
        return 3;
    }

    private void Update()
    {
        if (isDead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        switch (currentState)
        {
            case BossState.Moving:
                BeginTelegraph();
                break;

            case BossState.Telegraph:
                ExecuteAction();
                break;

            case BossState.Charging:
                EndCharge();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (currentState == BossState.Moving)
        {
            float speed = GetPhase() >= 2 ? moveSpeed * 1.5f : moveSpeed;
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else if (currentState != BossState.Charging)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void BeginTelegraph()
    {
        currentState = BossState.Telegraph;
        stateTimer = telegraphDuration;
        rb.linearVelocity = Vector2.zero;

        nextAction = ChooseAction();

        // Se sta per caricare, memorizza subito la direzione: il giocatore
        // deve poter schivare leggendo il telegrafo
        if (nextAction == BossAction.Charge && player != null)
        {
            chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        if (sr != null) sr.color = telegraphColor;
    }

    private BossAction ChooseAction()
    {
        int phase = GetPhase();

        if (phase == 1) return BossAction.Radial;

        if (phase == 2)
        {
            return Random.value < 0.5f ? BossAction.Radial : BossAction.Volley;
        }

        int roll = Random.Range(0, 4);
        return (BossAction)roll;
    }

    private void ExecuteAction()
    {
        RestorePhaseColor();

        switch (nextAction)
        {
            case BossAction.Radial:
                FireRadial();
                ReturnToMoving();
                break;

            case BossAction.Volley:
                FireVolley();
                ReturnToMoving();
                break;

            case BossAction.Summon:
                SummonMinions();
                ReturnToMoving();
                break;

            case BossAction.Charge:
                StartCharge();
                break;
        }
    }

    private void ReturnToMoving()
    {
        currentState = BossState.Moving;
        stateTimer = moveDuration;
    }

    private void StartCharge()
    {
        currentState = BossState.Charging;
        stateTimer = chargeDuration;
        rb.linearVelocity = chargeDirection * chargeSpeed;
    }

    private void EndCharge()
    {
        rb.linearVelocity = Vector2.zero;
        ReturnToMoving();
    }

    private void FireRadial()
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < radialCount; i++)
        {
            float angle = (360f / radialCount) * i * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnProjectile(direction);
        }
    }

    private void FireVolley()
    {
        if (projectilePrefab == null || player == null) return;

        Vector2 aim = ((Vector2)player.position - (Vector2)transform.position).normalized;

        SpawnProjectile(aim);
        SpawnProjectile(Rotate(aim, volleySpread));
        SpawnProjectile(Rotate(aim, -volleySpread));
    }

    private void SummonMinions()
    {
        if (minionPrefab == null) return;

        for (int i = 0; i < minionCount; i++)
        {
            float angle = (360f / minionCount) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.2f;

            Instantiate(minionPrefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
        }

        // I rinforzi vanno contati, altrimenti la porta si sblocca in anticipo
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemySpawn(minionCount);
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null) projRb.linearVelocity = direction * projectileSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector2 Rotate(Vector2 direction, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }

    private void RestorePhaseColor()
    {
        if (sr == null) return;

        int phase = GetPhase();
        if (phase == 3) sr.color = phase3Color;
        else if (phase == 2) sr.color = phase2Color;
        else sr.color = phaseColor;
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
            return;
        }

        // Se non è in mezzo a un telegrafo, aggiorna il colore alla fase corrente
        if (currentState != BossState.Telegraph)
        {
            RestorePhaseColor();
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
