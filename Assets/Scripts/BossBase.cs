using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Struttura comune a Boss e Miniboss: macchina a stati (movimento -> telegrafo ->
// azione), carica, fase di rabbia, barra della vita e morte con ricompensa.
// Le azioni concrete e i colori restano nelle sottoclassi.
//
// NB: i valori di default qui sotto valgono solo per oggetti nuovi. Boss e
// Miniboss già in scena mantengono i valori salvati nei rispettivi Prefab.
public abstract class BossBase : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] protected float moveSpeed = 1.5f;
    [SerializeField] protected float moveDuration = 1.5f;

    [Header("Telegraph")]
    [SerializeField] protected float telegraphDuration = 0.4f;
    [SerializeField] protected Color telegraphColor = Color.white;

    [Header("Proiettili")]
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected float projectileSpeed = 5f;

    [Header("Carica")]
    [SerializeField] protected float chargeSpeed = 7f;
    [SerializeField] protected float chargeDuration = 0.4f;

    [Header("Rabbia (salute critica)")]
    [SerializeField] [Range(0f, 1f)] protected float enrageHealthRatio = 0.4f;
    [SerializeField] protected float enrageSpeedMultiplier = 1.4f;
    [SerializeField] protected Color enrageColor = new Color(1f, 0.3f, 0.1f);

    [Header("UI Barra Vita")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject healthBarRoot;

    [Header("Feedback morte")]
    [SerializeField] private float deathShakeDuration = 0.25f;
    [SerializeField] private float deathShakeMagnitude = 0.12f;

    // Acting serve ai boss che eseguono un'azione lunga tramite coroutine
    protected enum BossState { Moving, Telegraph, Charging, Acting }

    protected BossState currentState;
    protected float stateTimer;
    protected Vector2 chargeDirection;

    protected override void Start()
    {
        base.Start();

        currentState = BossState.Moving;
        stateTimer = moveDuration;

        if (healthBarRoot != null) healthBarRoot.SetActive(true);
        UpdateHealthBarUI();
    }

    protected virtual void Update()
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

    protected virtual void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (currentState == BossState.Moving)
        {
            MoveTowardsPlayer(EffectiveMoveSpeed());
        }
        else if (currentState != BossState.Charging)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // --- Stati ---

    protected void BeginTelegraph()
    {
        currentState = BossState.Telegraph;
        stateTimer = EffectiveTelegraphDuration();
        rb.linearVelocity = Vector2.zero;

        ChooseNextAction();

        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
    }

    protected virtual void ReturnToMoving()
    {
        currentState = BossState.Moving;
        stateTimer = EffectiveMoveDuration();
        RestoreBaseColor();
    }

    protected void StartCharge()
    {
        currentState = BossState.Charging;
        stateTimer = chargeDuration;
        rb.linearVelocity = chargeDirection * chargeSpeed;
    }

    protected virtual void EndCharge()
    {
        rb.linearVelocity = Vector2.zero;
        ReturnToMoving();
    }

    protected void AimChargeAtPlayer()
    {
        if (player != null) chargeDirection = DirectionToPlayer();
    }

    // --- Da implementare nelle sottoclassi ---

    // Sceglie l'azione da eseguire a fine telegrafo (e ne prepara i dati)
    protected abstract void ChooseNextAction();

    // Esegue l'azione scelta e riporta il boss in movimento
    protected abstract void ExecuteAction();

    // Colorazione durante la fase di rabbia
    protected abstract void UpdateEnrageVisual();

    // --- Parametri effettivi (le sottoclassi li affinano per fase) ---

    protected virtual float EffectiveMoveSpeed()
    {
        return IsEnraged() ? moveSpeed * enrageSpeedMultiplier : moveSpeed;
    }

    protected virtual float EffectiveMoveDuration()
    {
        return IsEnraged() ? moveDuration * 0.6f : moveDuration;
    }

    protected virtual float EffectiveTelegraphDuration()
    {
        return telegraphDuration;
    }

    // --- Utilità ---

    protected float HealthRatio()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }

    protected bool IsEnraged()
    {
        return HealthRatio() <= enrageHealthRatio;
    }

    // Estrae un'azione dal pool evitando, quando può, di ripetere l'ultima
    protected T PickAction<T>(T[] pool, T lastAction) where T : struct
    {
        T chosen;
        int attempts = 0;

        do
        {
            chosen = pool[Random.Range(0, pool.Length)];
            attempts++;
        }
        while (EqualityComparer<T>.Default.Equals(chosen, lastAction) && pool.Length > 1 && attempts < 6);

        return chosen;
    }

    protected void UpdateHealthBarUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = HealthRatio();
        }
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdateHealthBarUI();
    }

    protected override void Die()
    {
        StopAllCoroutines(); // ferma azioni lunghe ancora in corso (es. la spirale)

        if (RoomManager.Instance != null && transform.parent != null)
        {
            RoomManager.Instance.SpawnRandomPermanentUpgrade(transform.position, transform.parent.gameObject);
        }

        if (healthBarRoot != null) healthBarRoot.SetActive(false);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(deathShakeDuration, deathShakeMagnitude);

        base.Die();
    }
}
