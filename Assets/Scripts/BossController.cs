using System.Collections;
using UnityEngine;

// Boss finale a tre fasi. Struttura degli stati, carica, rabbia, barra vita
// e morte stanno in BossBase: qui restano solo le azioni e i colori di fase.
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : BossBase
{
    [Header("Proiettili")]
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
    [SerializeField] private float minionSpawnRadius = 1.2f;

    [Header("Rabbia (salute critica)")]
    [SerializeField] private float enragePulseSpeed = 6f;

    [Header("Colori fase")]
    [SerializeField] private Color phase2Color = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color phase3Color = new Color(1f, 0.2f, 0.2f);

    private enum BossAction { Radial, Volley, Charge, Summon, Spiral }

    private BossAction nextAction;
    private BossAction lastAction = BossAction.Radial;
    private int chargeChainCount;

    private int GetPhase()
    {
        float ratio = HealthRatio();
        if (ratio > 0.66f) return 1;
        if (ratio > 0.33f) return 2;
        return 3;
    }

    // --- Parametri per fase ---

    protected override float EffectiveMoveSpeed()
    {
        float speed = GetPhase() >= 2 ? moveSpeed * 1.5f : moveSpeed;
        if (IsEnraged()) speed *= enrageSpeedMultiplier;

        return speed;
    }

    protected override float EffectiveMoveDuration()
    {
        if (IsEnraged()) return moveDuration * 0.5f;
        if (GetPhase() >= 2) return moveDuration * 0.75f;

        return moveDuration;
    }

    protected override float EffectiveTelegraphDuration()
    {
        return IsEnraged() ? telegraphDuration * 0.6f : telegraphDuration;
    }

    // --- Azioni ---

    protected override void ChooseNextAction()
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

        nextAction = PickAction(pool, lastAction);
        lastAction = nextAction;

        if (nextAction == BossAction.Charge) AimChargeAtPlayer();
    }

    protected override void ExecuteAction()
    {
        RestorePhaseColor();

        switch (nextAction)
        {
            case BossAction.Radial:
                FireRadialBurst(projectilePrefab, radialCount, projectileSpeed);
                ReturnToMoving();
                break;

            case BossAction.Volley:
                FireVolley();
                ReturnToMoving();
                break;

            case BossAction.Summon:
                SpawnEnemiesAroundSelf(minionPrefab, SummonCount(), minionSpawnRadius);
                ReturnToMoving();
                break;

            case BossAction.Spiral:
                // L'azione dura più frame: il timer resta fermo finché la coroutine non finisce
                currentState = BossState.Acting;
                stateTimer = Mathf.Infinity;
                StartCoroutine(SpiralBarrageRoutine());
                break;

            case BossAction.Charge:
                StartCharge();
                break;
        }
    }

    protected override void ReturnToMoving()
    {
        chargeChainCount = 0;
        base.ReturnToMoving();
    }

    protected override void EndCharge()
    {
        rb.linearVelocity = Vector2.zero;

        bool canChain = GetPhase() == 3 && chargeChainCount < maxChargeChain - 1 && player != null;

        if (canChain && Random.value < chargeChainChance)
        {
            chargeChainCount++;
            nextAction = BossAction.Charge;
            AimChargeAtPlayer();

            currentState = BossState.Telegraph;
            stateTimer = EffectiveTelegraphDuration();
            if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
        }
        else
        {
            ReturnToMoving();
        }
    }

    private void FireVolley()
    {
        if (player == null) return;

        Vector2 aim = DirectionToPlayer();

        SpawnProjectile(projectilePrefab, transform.position, aim, projectileSpeed);
        SpawnProjectile(projectilePrefab, transform.position, VectorUtils.Rotate(aim, volleySpread), projectileSpeed);
        SpawnProjectile(projectilePrefab, transform.position, VectorUtils.Rotate(aim, -volleySpread), projectileSpeed);

        if (GetPhase() == 3)
        {
            SpawnProjectile(projectilePrefab, transform.position, VectorUtils.Rotate(aim, volleySpread * 2f), projectileSpeed);
            SpawnProjectile(projectilePrefab, transform.position, VectorUtils.Rotate(aim, -volleySpread * 2f), projectileSpeed);
        }
    }

    private IEnumerator SpiralBarrageRoutine()
    {
        float angleOffset = 0f;

        for (int wave = 0; wave < spiralWaveCount; wave++)
        {
            // Il boss resta in scena per la durata dell'animazione di morte:
            // senza questo controllo può sparare un'altra ondata da morto
            if (isDead) yield break;

            FireRadialBurst(projectilePrefab, spiralBulletsPerWave, spiralProjectileSpeed, angleOffset);

            angleOffset += spiralRotationStep;
            yield return new WaitForSeconds(spiralWaveInterval);
        }

        ReturnToMoving();
    }

    private int SummonCount()
    {
        int count = minionCount;
        if (GetPhase() == 3) count++;
        if (IsEnraged()) count++;

        return count;
    }

    // --- Colori ---

    protected override void UpdateEnrageVisual()
    {
        if (spriteRenderer == null || !IsEnraged() || currentState == BossState.Telegraph || isFlashing) return;

        float t = (Mathf.Sin(Time.time * enragePulseSpeed) + 1f) * 0.5f;
        spriteRenderer.color = Color.Lerp(phase3Color, enrageColor, t);
    }

    private void RestorePhaseColor()
    {
        if (spriteRenderer == null || isFlashing) return;

        int phase = GetPhase();
        if (phase == 3) spriteRenderer.color = phase3Color;
        else if (phase == 2) spriteRenderer.color = phase2Color;
        else spriteRenderer.color = baseSpriteColor;
    }

    protected override void RestoreBaseColor()
    {
        // In Telegraph vince il colore di preavviso (gestito da BossBase)
        if (currentState == BossState.Telegraph)
        {
            base.RestoreBaseColor();
            return;
        }

        RestorePhaseColor();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount); // aggiorna anche la barra della vita

        if (!isDead && currentState != BossState.Telegraph)
        {
            RestorePhaseColor();
        }
    }
}
