using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SplitterController : EnemyBase
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.2f;

    [Header("Divisione")]
    [SerializeField] private GameObject splitPrefab;
    [SerializeField] private int splitCount = 2;
    [SerializeField] private float splitSpread = 0.6f;

    private void FixedUpdate()
    {
        if (isDead) return;

        MoveTowardsPlayer(moveSpeed);
    }

    protected override void Die()
    {
        Split();
        base.Die(); // conteggio morte + Destroy
    }

    private void Split()
    {
        // Registra da sé lo spawn nel RoomManager
        SpawnEnemiesAroundSelf(splitPrefab, splitCount, splitSpread);
    }
}