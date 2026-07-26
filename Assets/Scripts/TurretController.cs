using UnityEngine;

public class TurretController : EnemyBase
{
    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float fireInterval = 2f;

    private float fireTimer;

    protected override void Start()
    {
        base.Start();
        fireTimer = fireInterval;
    }

    private void Update()
    {
        if (isDead) return;

        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f && player != null)
        {
            Fire();
            fireTimer = fireInterval;
        }
    }

    private void Fire()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        SpawnProjectile(projectilePrefab, transform.position, direction, projectileSpeed);
    }
}