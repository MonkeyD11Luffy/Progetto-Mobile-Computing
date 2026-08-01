using UnityEngine;

public class TeleporterController : EnemyBase
{
    [Header("Tempi")]
    [SerializeField] private float idleDuration = 1.8f;
    [SerializeField] private float vanishDuration = 0.4f;

    [Header("Teletrasporto")]
    [SerializeField] private float minDistanceFromPlayer = 2.5f;
    [SerializeField] private float maxDistanceFromPlayer = 4f;

    [Header("Confini stanza")]
    [SerializeField] private float roomHalfWidth = 3f;
    [SerializeField] private float roomHalfHeight = 3f;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 7f;

    private enum TeleporterState { Idle, Vanishing }

    private Collider2D col;
    private TeleporterState currentState;
    private float stateTimer;

    protected override void Awake()
    {
        base.Awake(); // qui viene già preso lo SpriteRenderer

        col = GetComponent<Collider2D>();
    }

    protected override void Start()
    {
        base.Start();

        currentState = TeleporterState.Idle;
        stateTimer = idleDuration;
    }

    private void Update()
    {
        if (isDead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        if (currentState == TeleporterState.Idle)
        {
            StartVanish();
        }
        else
        {
            Reappear();
        }
    }

    private void StartVanish()
    {
        currentState = TeleporterState.Vanishing;
        stateTimer = vanishDuration;
        SetVisible(false);
    }

    private void Reappear()
    {
        currentState = TeleporterState.Idle;
        stateTimer = idleDuration;

        MoveToRandomPositionNearPlayer();
        SetVisible(true);
        Fire();
    }

    private void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (col != null) col.enabled = visible;
    }

    private void MoveToRandomPositionNearPlayer()
    {
        if (player == null) return;

        Vector2 roomCenter = transform.parent != null
            ? (Vector2)transform.parent.position
            : Vector2.zero;

        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

        Vector2 offset = VectorUtils.FromAngle(angle) * distance;
        Vector2 targetPosition = (Vector2)player.position + offset;

        targetPosition.x = Mathf.Clamp(targetPosition.x, roomCenter.x - roomHalfWidth, roomCenter.x + roomHalfWidth);
        targetPosition.y = Mathf.Clamp(targetPosition.y, roomCenter.y - roomHalfHeight, roomCenter.y + roomHalfHeight);

        transform.position = targetPosition;
    }

    private void Fire()
    {
        if (player == null) return;

        SpawnProjectile(projectilePrefab, transform.position, DirectionToPlayer(), projectileSpeed);
    }
}