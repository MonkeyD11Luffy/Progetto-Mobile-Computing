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
        SpawnProjectile(projectilePrefab, transform.position, DirectionToPlayer(), projectileSpeed);
    }
}