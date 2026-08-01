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

        shieldDirection = Vector3.RotateTowards(
            shieldDirection,
            DirectionToPlayer(),
            shieldRotationSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f
        );

        if (shieldVisual != null)
        {
            shieldVisual.rotation = Quaternion.Euler(0, 0, VectorUtils.ToAngle(shieldDirection));
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        MoveTowardsPlayer(moveSpeed);
    }

    public bool IsBlocked(Vector2 hitPosition)
    {
        Vector2 fromEnemyToHit = (hitPosition - (Vector2)transform.position).normalized;
        return Vector2.Dot(fromEnemyToHit, shieldDirection) > shieldArc;
    }
}