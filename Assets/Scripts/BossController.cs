using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : EnemyBase
{
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

    [Header("Spirale (fase 2+)")]
    [SerializeField] private int spiralWaveCount = 3;
    [SerializeField] private int spiralBulletsPerWave = 5;
    [SerializeField] private float spiralWaveInterval = 0.3f;
    [SerializeField] private float spiralRotationStep = 20f;
    [SerializeField] private float spiralProjectileSpeed = 3.5f;

    [Header("Carica in serie (fase 3)")]
    [SerializeField] private int maxChargeChain = 2;
    [SerializeField] private float chargeChainChance = 0.5f;

    [Header("Evocazione")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionCount = 2;

    [Header("Rabbia (salute critica)")]
    [SerializeField] [Range(0f, 1f)] private float enrageHealthRatio = 0.15f;
    [SerializeField] private float enrageSpeedMultiplier = 1.3f;
    [SerializeField] private float enragePulseSpeed = 6f;
    [SerializeField] private Color enrageColor = new Color(1f, 0f, 0f);

    [Header("Colori fase")]
    [SerializeField] private Color phase2Color = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color phase3Color = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color telegraphColor = Color.white;

    [Header("UI Barra Vita")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject healthBarRoot;

    [Header("Feedback morte")]
    [SerializeField] private float deathShakeDuration = 0.3f;
    [SerializeField] private float deathShakeMagnitude = 0.15f;

    private enum BossState { Moving, Telegraph, Charging, Acting }
    private enum BossAction { Radial, Volley, Charge, Summon, Spiral }

    private SpriteRenderer sr;
    private BossState currentState;
    private BossAction nextAction;
    private BossAction lastAction = BossAction.Radial;
    private float stateTimer;
    private Vector2 chargeDirection;
    private Color phase1Color;
    private int chargeChainCount;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) phase1Color = sr.color;
    }

    protected override void Start()
    {
        base.Start();

        currentState = BossState.Moving;
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

    private int GetPhase()
    {
        float ratio = (float)currentHealth / maxHealth;
        if (ratio > 0.66f) return 1;
        if (ratio > 0.33f) return 2;
        return 3;
    }

    private bool IsEnraged()
    {
        return (float)currentHealth / maxHealth <= enrageHealthRatio;
    }

    private float EffectiveTelegraphDuration()
    {
        return IsEnraged() ? telegraphDuration * 0.6f : telegraphDuration;
    }

    private float EffectiveMoveDuration()
    {
        if (IsEnraged()) return moveDuration * 0.5f;
        if (GetPhase() >= 2) return moveDuration * 0.75f;
        return moveDuration;
    }

    private void Update()
    {
        if (isDead) return;

        UpdateEnrageVisual();

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

    private void UpdateEnrageVisual()
    {
        if (sr == null || !IsEnraged() || currentState == BossState.Telegraph || isFlashing) return;

        float t = (Mathf.Sin(Time.time * enragePulseSpeed) + 1f) * 0.5f;
        sr.color = Color.Lerp(phase3Color, enrageColor, t);
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (currentState == BossState.Moving)
        {
            float speed = GetPhase() >= 2 ? moveSpeed * 1.5f : moveSpeed;
            if (IsEnraged()) speed *= enrageSpeedMultiplier;

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
        stateTimer = EffectiveTelegraphDuration();
        rb.linearVelocity = Vector2.zero;

        nextAction = ChooseAction();

        if (nextAction == BossAction.Charge && player != null)
        {
            chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        if (sr != null) sr.color = telegraphColor;
    }

    private BossAction ChooseAction()
    {
        int phase = GetPhase();
        BossAction[] pool;

        if (phase == 1)
        {
            pool = new[] { BossAction.Radial };
        }
        else if (phase == 2)
        {
            pool = new[] { BossAction.Radial, BossAction.Volley, BossAction.Spiral };
        }
        else if (IsEnraged())
        {
            pool = new[] { BossAction.Charge, BossAction.Spiral, BossAction.Summon, BossAction.Volley };
        }
        else
        {
            pool = new[] { BossAction.Radial, BossAction.Volley, BossAction.Charge, BossAction.Summon, BossAction.Spiral };
        }

        BossAction chosen;
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

            case BossAction.Spiral:
                currentState = BossState.Acting;
                stateTimer = Mathf.Infinity;
                StartCoroutine(SpiralBarrageRoutine());
                break;

            case BossAction.Charge:
                StartCharge();
                break;
        }
    }

    private void ReturnToMoving()
    {
        chargeChainCount = 0;
        currentState = BossState.Moving;
        stateTimer = EffectiveMoveDuration();
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

        bool canChain = GetPhase() == 3 && chargeChainCount < maxChargeChain - 1 && player != null;
        if (canChain && Random.value < chargeChainChance)
        {
            chargeChainCount++;
            nextAction = BossAction.Charge;
            chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
            currentState = BossState.Telegraph;
            stateTimer = EffectiveTelegraphDuration();
            if (sr != null) sr.color = telegraphColor;
        }
        else
        {
            ReturnToMoving();
        }
    }

    private void FireRadial()
    {
        for (int i = 0; i < radialCount; i++)
        {
            float angle = (360f / radialCount) * i * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnProjectile(projectilePrefab, transform.position, direction, projectileSpeed);
        }
    }

    private void FireVolley()
    {
        if (player == null) return;

        Vector2 aim = ((Vector2)player.position - (Vector2)transform.position).normalized;

        SpawnProjectile(projectilePrefab, transform.position, aim, projectileSpeed);
        SpawnProjectile(projectilePrefab, transform.position, Rotate(aim, volleySpread), projectileSpeed);
        SpawnProjectile(projectilePrefab, transform.position, Rotate(aim, -volleySpread), projectileSpeed);

        if (GetPhase() == 3)
        {
            SpawnProjectile(projectilePrefab, transform.position, Rotate(aim, volleySpread * 2f), projectileSpeed);
            SpawnProjectile(projectilePrefab, transform.position, Rotate(aim, -volleySpread * 2f), projectileSpeed);
        }
    }

    private IEnumerator SpiralBarrageRoutine()
    {
        float angleOffset = 0f;

        for (int wave = 0; wave < spiralWaveCount; wave++)
        {
            for (int i = 0; i < spiralBulletsPerWave; i++)
            {
                float angle = ((360f / spiralBulletsPerWave) * i + angleOffset) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnProjectile(projectilePrefab, transform.position, direction, spiralProjectileSpeed);
            }

            angleOffset += spiralRotationStep;
            yield return new WaitForSeconds(spiralWaveInterval);
        }

        ReturnToMoving();
    }

    private void SummonMinions()
    {
        if (minionPrefab == null) return;

        int count = minionCount;
        if (GetPhase() == 3) count++;
        if (IsEnraged()) count++;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.2f;

            Instantiate(minionPrefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemySpawn(count);
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

    private void RestorePhaseColor()
    {
        if (sr == null || isFlashing) return;

        int phase = GetPhase();
        if (phase == 3) sr.color = phase3Color;
        else if (phase == 2) sr.color = phase2Color;
        else sr.color = phase1Color;
    }

    protected override void RestoreBaseColor()
    {
        RestorePhaseColor();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        UpdateHealthBarUI();

        if (!isDead && currentState != BossState.Telegraph)
        {
            RestorePhaseColor();
        }
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
