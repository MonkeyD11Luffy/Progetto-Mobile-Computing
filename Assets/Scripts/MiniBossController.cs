using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MinibossController : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float pauseDuration = 1f;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private int projectilesPerBurst = 5;

    private enum BossState { Moving, Pausing }

    private BossState currentState;
    private float stateTimer;

    protected override void Start()
    {
        base.Start();

        currentState = BossState.Moving;
        stateTimer = moveDuration;
    }

    private void Update()
    {
        if (isDead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        if (currentState == BossState.Moving)
        {
            currentState = BossState.Pausing;
            stateTimer = pauseDuration;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            FireBurst();
            currentState = BossState.Moving;
            stateTimer = moveDuration;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (currentState == BossState.Moving)
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    private void FireBurst()
    {
        for (int i = 0; i < projectilesPerBurst; i++)
        {
            float angle = (360f / projectilesPerBurst) * i * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnProjectile(projectilePrefab, transform.position, direction, projectileSpeed);
        }
    }

    protected override void Die()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.SpawnRandomPermanentUpgrade(transform.position, transform.parent.gameObject);
        }

        base.Die();
    }
}