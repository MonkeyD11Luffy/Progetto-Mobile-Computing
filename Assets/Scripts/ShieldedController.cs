using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShieldedController : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.3f;

    [Header("Scudo")]
    [SerializeField] private Transform shieldVisual;
    [SerializeField] private float shieldArc = 0.3f;
    [SerializeField] private float shieldRotationSpeed = 80f;

    private Vector2 shieldDirection = Vector2.right;

    private void Update()
    {
        if (isDead || player == null) return;

        Vector2 targetDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;

        shieldDirection = Vector3.RotateTowards(
            shieldDirection,
            targetDirection,
            shieldRotationSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f
        );

        if (shieldVisual != null)
        {
            float angle = Mathf.Atan2(shieldDirection.y, shieldDirection.x) * Mathf.Rad2Deg;
            shieldVisual.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public bool IsBlocked(Vector2 hitPosition)
    {
        Vector2 fromEnemyToHit = (hitPosition - (Vector2)transform.position).normalized;
        return Vector2.Dot(fromEnemyToHit, shieldDirection) > shieldArc;
    }
}