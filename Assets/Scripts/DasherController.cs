using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DasherController : EnemyBase
{
    [Header("Tempi")]
    [SerializeField] private float idleDuration = 1.5f;
    [SerializeField] private float telegraphDuration = 0.6f;
    [SerializeField] private float dashDuration = 0.4f;

    [Header("Scatto")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float idleMoveSpeed = 1f;

    [Header("Telegrafo")]
    [SerializeField] private Color telegraphColor = Color.white;

    private enum DasherState { Idle, Telegraph, Dashing }

    private DasherState currentState;
    private float stateTimer;
    private Vector2 dashDirection;

    protected override void Start()
    {
        base.Start();

        currentState = DasherState.Idle;
        stateTimer = idleDuration;
    }

    private void Update()
    {
        if (isDead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        switch (currentState)
        {
            case DasherState.Idle:
                StartTelegraph();
                break;

            case DasherState.Telegraph:
                StartDash();
                break;

            case DasherState.Dashing:
                StopDash();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || currentState == DasherState.Dashing) return;

        if (currentState == DasherState.Idle && player != null)
        {
            MoveTowardsPlayer(idleMoveSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void StartTelegraph()
    {
        currentState = DasherState.Telegraph;
        stateTimer = telegraphDuration;
        rb.linearVelocity = Vector2.zero;

        if (player != null)
        {
            dashDirection = DirectionToPlayer();
        }

        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
    }

    private void StartDash()
    {
        currentState = DasherState.Dashing;
        stateTimer = dashDuration;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (spriteRenderer != null) spriteRenderer.color = baseSpriteColor;
    }

    private void StopDash()
    {
        currentState = DasherState.Idle;
        stateTimer = idleDuration;
        rb.linearVelocity = Vector2.zero;
    }

    protected override void RestoreBaseColor()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = currentState == DasherState.Telegraph
            ? telegraphColor
            : baseSpriteColor;
    }
}