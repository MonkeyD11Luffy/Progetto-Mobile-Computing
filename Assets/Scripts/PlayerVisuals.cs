using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisuals : MonoBehaviour
{
    [Header("Sprite direzionali")]
    [SerializeField] private Sprite[] walkDown;   // 3 frame
    [SerializeField] private Sprite[] walkUp;     // 3 frame
    [SerializeField] private Sprite[] walkSide;   // 3 frame
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private int idleFrame = 1;

    [Header("Arma")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private Sprite[] weaponSprites; // 0 Single, 1 Spread, 2 Piercing, 3 Melee
    [SerializeField] private float weaponOrbitRadius = 0.35f;
    [SerializeField] private int weaponSortingOffset = 1;
    [SerializeField] private float[] muzzleForward = { 0.5625f, 0.5f, 0.6875f, 0.75f };
    [SerializeField] private float muzzleUp = 0.19f;
    [SerializeField] private float weaponHideDelay = 0.15f; // quanto resta visibile dopo l'ultimo colpo

    // Ciclo di camminata ping-pong: 0, 1, 2, 1
    private static readonly int[] CycleFrames = { 0, 1, 2, 1 };

    private SpriteRenderer sr;
    private PlayerController player;
    private Vector2 lastFacing = Vector2.down;
    private float frameTimer;
    private int cycleStep;
    private float weaponVisibleTimer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null) return;

        Vector2 moveInput = player.MoveInput;
        Vector2 aimInput = player.AimInput;

        // 1) Direzione: mira > movimento > ultima direzione nota
        if (aimInput != Vector2.zero) lastFacing = aimInput;
        else if (moveInput != Vector2.zero) lastFacing = moveInput;

        // 2) Frame del ciclo di camminata
        bool isMoving = moveInput != Vector2.zero;

        if (isMoving)
        {
            frameTimer += Time.deltaTime;

            if (frameRate > 0f && frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                cycleStep = (cycleStep + 1) % CycleFrames.Length;
            }
        }
        else
        {
            frameTimer = 0f;
            cycleStep = 0;
        }

        int frame = isMoving ? CycleFrames[cycleStep] : idleFrame;

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

        // 4) Arma orbitante
        UpdateWeapon();
    }

    private void UpdateWeapon()
    {
        // L'arma si vede solo mentre si spara: fuori dal fuoco resta nascosta
        if (player.AimInput != Vector2.zero) weaponVisibleTimer = weaponHideDelay;
        else weaponVisibleTimer -= Time.deltaTime;

        bool weaponVisible = weaponVisibleTimer > 0f;

        if (weaponPivot == null)
        {
            if (weaponRenderer != null) weaponRenderer.enabled = weaponVisible;
            return;
        }

        Vector2 aim = player.LastAimDirection;

        weaponPivot.localPosition = aim * weaponOrbitRadius;

        weaponPivot.localRotation = Quaternion.Euler(0f, 0f, VectorUtils.ToAngle(aim));

        if (weaponRenderer == null) return;

        weaponRenderer.enabled = weaponVisible;

        int weaponIndex = player.CurrentWeaponIndex;

        if (weaponSprites != null && weaponIndex >= 0 && weaponIndex < weaponSprites.Length)
        {
            weaponRenderer.sprite = weaponSprites[weaponIndex];
        }

        weaponRenderer.flipY = aim.x < 0f;
        weaponRenderer.sortingOrder = sr.sortingOrder + (aim.y > 0f ? -weaponSortingOffset : weaponSortingOffset);
    }

    // Punta della canna: da dove deve partire il proiettile
    public Vector2 MuzzlePosition
    {
        get
        {
            if (player == null) return transform.position;

            Vector2 aim = player.LastAimDirection;

            float forward = weaponOrbitRadius;
            if (muzzleForward != null && muzzleForward.Length > player.CurrentWeaponIndex)
            {
                forward += muzzleForward[player.CurrentWeaponIndex];
            }

            Vector2 perp = new Vector2(-aim.y, aim.x);
            float side = aim.x < 0f ? -1f : 1f;

            return (Vector2)transform.position + aim * forward + perp * (muzzleUp * side);
        }
    }
}
