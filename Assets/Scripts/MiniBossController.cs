using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class MinibossController : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveDuration = 1.5f;

    [Header("Telegraph")]
    [SerializeField] private float telegraphDuration = 0.4f;
    [SerializeField] private Color telegraphColor = Color.white;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private int projectilesPerBurst = 5;
    [SerializeField] private int aimedBurstCount = 3;
    [SerializeField] private float aimedSpread = 12f;

    [Header("Carica (fase rabbia)")]
    [SerializeField] private float chargeSpeed = 7f;
    [SerializeField] private float chargeDuration = 0.4f;

    [Header("Rabbia (salute critica)")]
    [SerializeField] [Range(0f, 1f)] private float enrageHealthRatio = 0.4f;
    [SerializeField] private float enrageSpeedMultiplier = 1.4f;
    [SerializeField] private Color enrageColor = new Color(1f, 0.3f, 0.1f);

    [Header("UI Barra Vita")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject healthBarRoot;

    [Header("Feedback morte")]
    [SerializeField] private float deathShakeDuration = 0.25f;
    [SerializeField] private float deathShakeMagnitude = 0.12f;

    private enum MinibossState { Moving, Telegraph, Charging }
    private enum MinibossAction { Radial, Aimed, Charge }

    private SpriteRenderer sr;
    private Color baseColor;
    private MinibossState currentState;
    private MinibossAction nextAction;
    private MinibossAction lastAction = MinibossAction.Radial;
    private float stateTimer;
    private Vector2 chargeDirection;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    protected override void Start()
    {
        base.Start();

        currentState = MinibossState.Moving;
        stateTimer = moveDuration;

        if (healthBarRoot != null) healthBarRoot.SetActive(true);
        UpdateHealthBarUI();
    }

    private void UpdateHealthBarUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdateHealthBarUI();
    }

    private bool IsEnraged()
    {
        return (float)currentHealth / maxHealth <= enrageHealthRatio;
    }

    private void Update()
    {
        if (isDead) return;

        UpdateEnrageVisual();

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        switch (currentState)
        {
            case MinibossState.Moving:
                BeginTelegraph();
                break;

            case MinibossState.Telegraph:
                ExecuteAction();
                break;

            case MinibossState.Charging:
                EndCharge();
                break;
        }
    }

    private void UpdateEnrageVisual()
    {
        if (sr == null || currentState == MinibossState.Telegraph || isFlashing) return;
        sr.color = IsEnraged() ? enrageColor : baseColor;
    }

    protected override void RestoreBaseColor()
    {
        if (sr != null) sr.color = IsEnraged() ? enrageColor : baseColor;
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (currentState == MinibossState.Moving)
        {
            float speed = IsEnraged() ? moveSpeed * enrageSpeedMultiplier : moveSpeed;
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else if (currentState != MinibossState.Charging)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void BeginTelegraph()
    {
        currentState = MinibossState.Telegraph;
        stateTimer = telegraphDuration;
        rb.linearVelocity = Vector2.zero;

        nextAction = ChooseAction();

        if (nextAction == MinibossAction.Charge && player != null)
        {
            chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        if (sr != null) sr.color = telegraphColor;
    }

    private MinibossAction ChooseAction()
    {
        MinibossAction[] pool = IsEnraged()
            ? new[] { MinibossAction.Radial, MinibossAction.Aimed, MinibossAction.Charge }
            : new[] { MinibossAction.Radial, MinibossAction.Aimed };

        MinibossAction chosen;
        int attempts = 0;
        do
        {
            chosen = pool[Random.Range(0, pool.Length)];
            attempts++;
        }
        while (chosen == lastAction && pool.Length > 1 && attempts < 6);

        lastAction = chosen;
        return chosen;
    }

    private void ExecuteAction()
    {
        switch (nextAction)
        {
            case MinibossAction.Radial:
                FireRadial();
                ReturnToMoving();
                break;

            case MinibossAction.Aimed:
                FireAimed();
                ReturnToMoving();
                break;

            case MinibossAction.Charge:
                StartCharge();
                break;
        }
    }

    private void ReturnToMoving()
    {
        currentState = MinibossState.Moving;
        stateTimer = IsEnraged() ? moveDuration * 0.6f : moveDuration;
        if (sr != null) sr.color = IsEnraged() ? enrageColor : baseColor;
    }

    private void StartCharge()
    {
        currentState = MinibossState.Charging;
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
        for (int i = 0; i < projectilesPerBurst; i++)
        {
            float angle = (360f / projectilesPerBurst) * i * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnProjectile(projectilePrefab, transform.position, direction, projectileSpeed);
        }
    }

    private void FireAimed()
    {
        if (player == null) return;

        Vector2 aim = ((Vector2)player.position - (Vector2)transform.position).normalized;
        int half = aimedBurstCount / 2;

        for (int i = -half; i <= half; i++)
        {
            Vector2 direction = i == 0 ? aim : Rotate(aim, aimedSpread * i);
            SpawnProjectile(projectilePrefab, transform.position, direction, projectileSpeed);
        }
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

    protected override void Die()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.SpawnRandomPermanentUpgrade(transform.position, transform.parent.gameObject);
        }

        if (healthBarRoot != null) healthBarRoot.SetActive(false);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(deathShakeDuration, deathShakeMagnitude);

        base.Die();
    }
}
