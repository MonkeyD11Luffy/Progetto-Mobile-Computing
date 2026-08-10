using UnityEngine;

// Gemello di PlayerVisuals per i nemici: nessun input, la direzione arriva da
// EnemyBase.MoveDirection (con ripiego sulla velocità del Rigidbody2D per gli
// oggetti che non hanno un EnemyBase).
//
// Tocca solo sr.sprite e sr.flipX. Colore e visibilità restano di chi li gestisce
// già: EnemyBase (lampo di danno), DasherController (telegrafo),
// BossBase/BossController (colori di fase e rabbia), TeleporterController (sparizione).
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisuals : MonoBehaviour
{
    [Header("Sprite direzionali")]
    [SerializeField] private Sprite[] walkDown;   // 4 frame
    [SerializeField] private Sprite[] walkUp;     // 4 frame
    [SerializeField] private Sprite[] walkSide;   // 4 frame
    [SerializeField] private float frameRate = 6f;
    [SerializeField] private int idleFrame = 0;
    [SerializeField] private float moveThreshold = 0.01f;

    private const int CycleLength = 4;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private EnemyBase enemy;
    private Vector2 lastFacing = Vector2.down;
    private float frameTimer;
    private int cycleStep;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyBase>();
    }

    private void Update()
    {
        // Chi non ha né EnemyBase né Rigidbody2D (es. torrette statiche) non ha
        // una direzione da cui partire
        if (enemy == null && rb == null) return;

        // 1) Direzione del movimento. EnemyBase la espone perché nel ramo con
        // pathfinding il movimento avviene con MovePosition e il Rigidbody2D
        // resta fermo a velocità zero.
        Vector2 movement = enemy != null ? enemy.MoveDirection : rb.linearVelocity;
        bool isMoving = movement.sqrMagnitude > moveThreshold;

        if (isMoving) lastFacing = movement.normalized;

        // 2) Frame del ciclo di camminata
        if (isMoving)
        {
            frameTimer += Time.deltaTime;

            if (frameRate > 0f && frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                cycleStep = (cycleStep + 1) % CycleLength;
            }
        }
        else
        {
            frameTimer = 0f;
            cycleStep = 0;
        }

        int frame = isMoving ? cycleStep : idleFrame;

        // 3) Array direzionale e flip
        Sprite[] frames;

        if (Mathf.Abs(lastFacing.y) > Mathf.Abs(lastFacing.x))
        {
            frames = lastFacing.y > 0f ? walkUp : walkDown;
            sr.flipX = false;
        }
        else
        {
            frames = walkSide;
            sr.flipX = lastFacing.x < 0f;
        }

        if (frames != null && frame >= 0 && frame < frames.Length && frames[frame] != null)
        {
            sr.sprite = frames[frame];
        }
    }
}
