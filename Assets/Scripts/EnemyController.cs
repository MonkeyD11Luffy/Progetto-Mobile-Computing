using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 2f;

    private void FixedUpdate()
    {
        if (isDead) return;

        MoveTowardsPlayer(moveSpeed);
    }
}