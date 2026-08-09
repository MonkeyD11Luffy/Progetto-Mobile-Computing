using UnityEngine;

public class TurretController : EnemyBase
{
    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float fireInterval = 2f;

    [Header("Sprite direzionali")]
    // 8 elementi, in ordine antiorario partendo da destra:
    // 0 destra, 1 alto-destra, 2 alto, 3 alto-sinistra,
    // 4 sinistra, 5 basso-sinistra, 6 basso, 7 basso-destra
    [SerializeField] private Sprite[] directionSprites;

    private float fireTimer;

    protected override void Start()
    {
        base.Start();
        fireTimer = fireInterval;
    }

    private void Update()
    {
        if (isDead) return;

        UpdateFacing();

        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f && player != null)
        {
            Fire();
            fireTimer = fireInterval;
        }
    }

    private void UpdateFacing()
    {
        if (player == null || spriteRenderer == null) return;
        if (directionSprites == null || directionSprites.Length < 8) return;

        Vector2 dir = DirectionToPlayer();
        if (dir == Vector2.zero) return;

        float ang = VectorUtils.ToAngle(dir);
        if (ang < 0f) ang += 360f;

        int i = Mathf.RoundToInt(ang / 45f) % 8;
        spriteRenderer.sprite = directionSprites[i];
    }

    private void Fire()
    {
        SpawnProjectile(projectilePrefab, transform.position, DirectionToPlayer(), projectileSpeed);
    }
}