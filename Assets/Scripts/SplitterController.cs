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
        if (isDead || player == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    protected override void Die()
    {
        Split();
        base.Die(); // conteggio morte + Destroy
    }

    private void Split()
    {
        if (splitPrefab == null || splitCount <= 0) return;

        for (int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * splitSpread;

            Instantiate(splitPrefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemySpawn(splitCount);
        }
    }
}