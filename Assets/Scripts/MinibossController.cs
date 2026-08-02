using UnityEngine;

// Versione ridotta del boss: due azioni, più la carica quando è in rabbia.
// Stati, carica, barra vita e morte stanno in BossBase.
[RequireComponent(typeof(Rigidbody2D))]
public class MinibossController : BossBase
{
    [Header("Sparo")]
    [SerializeField] private int projectilesPerBurst = 5;
    [SerializeField] private int aimedBurstCount = 3;
    [SerializeField] private float aimedSpread = 12f;

    private enum MinibossAction { Radial, Aimed, Charge }

    private MinibossAction nextAction;
    private MinibossAction lastAction = MinibossAction.Radial;

    protected override void ChooseNextAction()
    {
        MinibossAction[] pool = IsEnraged()
            ? new[] { MinibossAction.Radial, MinibossAction.Aimed, MinibossAction.Charge }
            : new[] { MinibossAction.Radial, MinibossAction.Aimed };

        nextAction = PickAction(pool, lastAction);
        lastAction = nextAction;

        if (nextAction == MinibossAction.Charge) AimChargeAtPlayer();
    }

    protected override void ExecuteAction()
    {
        switch (nextAction)
        {
            case MinibossAction.Radial:
                FireRadialBurst(projectilePrefab, projectilesPerBurst, projectileSpeed);
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

    private void FireAimed()
    {
        if (player == null) return;

        Vector2 aim = DirectionToPlayer();
        int half = aimedBurstCount / 2;

        for (int i = -half; i <= half; i++)
        {
            Vector2 direction = i == 0 ? aim : VectorUtils.Rotate(aim, aimedSpread * i);
            SpawnProjectile(projectilePrefab, transform.position, direction, projectileSpeed);
        }
    }

    protected override void UpdateEnrageVisual()
    {
        if (spriteRenderer == null || currentState == BossState.Telegraph || isFlashing) return;

        spriteRenderer.color = IsEnraged() ? enrageColor : baseSpriteColor;
    }

    protected override void RestoreBaseColor()
    {
        // In Telegraph vince il colore di preavviso (gestito da BossBase)
        if (currentState == BossState.Telegraph)
        {
            base.RestoreBaseColor();
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = IsEnraged() ? enrageColor : baseSpriteColor;
        }
    }
}
