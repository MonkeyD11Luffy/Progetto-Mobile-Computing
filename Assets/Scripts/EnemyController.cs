using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 2f;

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
}