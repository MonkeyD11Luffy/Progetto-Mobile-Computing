using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireCooldown = 0.3f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float firePointDistance = 0.5f; // quanto avanti al player parte il colpo4

    [Header("Armi")]
    [SerializeField] private float spreadAngle = 25f;
    [SerializeField] private float spreadCooldownMult = 2.4f;
    [SerializeField] private float pierceCooldownMult = 1.3f;
    [SerializeField] private float spreadLifetime = 0.35f; // gittata corta

    [Header("Corpo a corpo")]
    [SerializeField] private float meleeRange = 1.1f;
    [SerializeField] private float meleeArc = 0.3f;
    [SerializeField] private int meleeDamageMult = 2;
    [SerializeField] private float meleeCooldownMult = 1.4f;

    [Header("Bombe")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int maxBombs = 3;
    [SerializeField] private float bombCooldown = 1f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI bombText;
    [SerializeField] private TextMeshProUGUI weaponText;

    [Header("Feedback")]
    [SerializeField] private Transform meleeVisual;
    [SerializeField] private float meleeVisualDuration = 0.1f;


    private enum WeaponType { Single, Spread, Piercing, Melee }
    private WeaponType currentWeapon = WeaponType.Single;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float fireTimer;
    private int currentHealth;
    private int currentBombs;
    private float bombTimer;
    private bool isInvulnerable;
    private float invulnerabilityTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        currentBombs = maxBombs;
        UpdateBombUI();
        UpdateWeaponUI();
        UpdateHealthUI();
    }

    private void Update()
    {
        HandleInput();
        HandleWeaponSwitch();
        HandleFiring();
        HandleBombPlacement();
        HandleInvulnerability();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void HandleInput()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;

        moveInput = new Vector2(x, y).normalized;
    }

    private void HandleWeaponSwitch()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;

        currentWeapon = currentWeapon switch
        {
            WeaponType.Single => WeaponType.Spread,
            WeaponType.Spread => WeaponType.Piercing,
            WeaponType.Piercing => WeaponType.Melee,
            _ => WeaponType.Single
        };

            UpdateWeaponUI();  
  }

    private float GetCurrentCooldown()
    {
        return currentWeapon switch
        {
            WeaponType.Spread => fireCooldown * spreadCooldownMult,
            WeaponType.Piercing => fireCooldown * pierceCooldownMult,
            WeaponType.Melee => fireCooldown * meleeCooldownMult,
            _ => fireCooldown
        };
    }

    private void HandleFiring()
    {
        fireTimer -= Time.deltaTime;

        Vector2 aimDirection = GetCardinalAimDirection();

        if (aimDirection != Vector2.zero && fireTimer <= 0f)
        {
            Fire(aimDirection);
            fireTimer = GetCurrentCooldown();
        }
    }

    private Vector2 GetCardinalAimDirection()
    {
        if (Input.GetKey(KeyCode.UpArrow)) return Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow)) return Vector2.down;
        if (Input.GetKey(KeyCode.LeftArrow)) return Vector2.left;
        if (Input.GetKey(KeyCode.RightArrow)) return Vector2.right;
        return Vector2.zero;
    }

    private void Fire(Vector2 direction)
{
    if (currentWeapon == WeaponType.Melee)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMelee();
        MeleeAttack(direction);
        return;
    }

    if (projectilePrefab == null)
    {
        Debug.LogWarning("Assegna projectilePrefab nell'Inspector.");
        return;
    }

    Vector2 origin = (Vector2)transform.position + direction * firePointDistance;

    switch (currentWeapon)
    {
        case WeaponType.Single:
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSingle();
            SpawnProjectile(origin, direction, false);
            break;

        case WeaponType.Spread:
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSpread();
            SpawnProjectile(origin, direction, false, spreadLifetime);
            SpawnProjectile(origin, Rotate(direction, spreadAngle), false, spreadLifetime);
            SpawnProjectile(origin, Rotate(direction, -spreadAngle), false, spreadLifetime);
            break;

        case WeaponType.Piercing:
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShootPierce();
            SpawnProjectile(origin, direction, true);
            break;
    }
}
   
    private void SpawnProjectile(Vector2 origin, Vector2 direction, bool piercing, float lifetime = -1f)
{
    GameObject projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);

    ProjectileController projController = projectile.GetComponent<ProjectileController>();
    if (projController != null)
    {
        projController.SetDamage(projectileDamage);
        projController.SetPiercing(piercing);

        // Se non viene passato un lifetime, resta quello di default del Prefab
        if (lifetime > 0f)
        {
            projController.SetLifetime(lifetime);
        }
    }

    Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
    if (projRb != null)
    {
        projRb.linearVelocity = direction * projectileSpeed;
    }

    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
}

    private Vector2 Rotate(Vector2 direction, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }

    private void MeleeAttack(Vector2 direction)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("EnemyProjectile"))
            {
                Vector2 toProj = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                if (Vector2.Dot(toProj, direction) >= meleeArc)
                {
                    Destroy(hit.gameObject);
                }
                continue;
            }

            if (!hit.CompareTag("Enemy")) continue;

            Vector2 toTarget = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Dot(toTarget, direction) < meleeArc) continue;

            ShieldedController shielded = hit.GetComponent<ShieldedController>();
            if (shielded != null && shielded.IsBlocked(transform.position)) continue;

            hit.gameObject.SendMessage("TakeDamage", projectileDamage * meleeDamageMult, SendMessageOptions.DontRequireReceiver);
        }

        if (meleeVisual != null)
        {
            StartCoroutine(ShowMeleeVisual(direction));
        }
    }

    private System.Collections.IEnumerator ShowMeleeVisual(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meleeVisual.rotation = Quaternion.Euler(0, 0, angle);
        meleeVisual.localPosition = direction * 0.7f;

        meleeVisual.gameObject.SetActive(true);
        yield return new WaitForSeconds(meleeVisualDuration);
        meleeVisual.gameObject.SetActive(false);
    }

    private void HandleBombPlacement()
    {
        bombTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E) && bombTimer <= 0f && currentBombs > 0)
        {
            PlaceBomb();
            bombTimer = bombCooldown;
        }
    }

    private void PlaceBomb()
    {
        if (bombPrefab == null) return;

        Instantiate(bombPrefab, transform.position, Quaternion.identity);
        currentBombs--;
        UpdateBombUI();
    }

    private void HandleInvulnerability()
{
    if (!isInvulnerable)
    {
        if (sr != null) sr.enabled = true;
        return;
    }

    invulnerabilityTimer -= Time.deltaTime;

    // Alterna visibile/invisibile ~10 volte al secondo
    if (sr != null) sr.enabled = Mathf.FloorToInt(invulnerabilityTimer * 10f) % 2 == 0;

    if (invulnerabilityTimer <= 0f)
    {
        isInvulnerable = false;
        if (sr != null) sr.enabled = true;
    }
}

    public void TakeDamage(int amount)
{
    if (isInvulnerable) return;

    currentHealth -= amount;
    UpdateHealthUI();

    if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHurt();

    isInvulnerable = true;
    invulnerabilityTimer = invulnerabilityDuration;

    if (currentHealth <= 0)
    {
        Die();
    }
}

    private void Die()
    {
        Debug.Log("Game Over");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOver();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Vita: {currentHealth}/{maxHealth}";
        }
    }

    private void UpdateBombUI()
{
    if (bombText != null)
    {
        bombText.color = currentBombs > 0 ? Color.white : Color.red;
        bombText.text = $"Bombe: {currentBombs}";
    }
}

    private void UpdateWeaponUI()
{
    if (weaponText == null) return;

    string weaponName = currentWeapon switch
    {
        WeaponType.Spread => "Spread",
        WeaponType.Piercing => "Perforante",
        WeaponType.Melee => "Corpo a corpo",
        _ => "Singolo"
    };

    weaponText.text = $"Arma: {weaponName}";
}

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateHealthUI();
    }

    public void IncreaseDamage(int amount)
    {
        projectileDamage += amount;
    }

    public void IncreaseSpeed(float amount)
    {
        moveSpeed += amount;
    }

    public void DecreaseFireCooldown(float amount)
    {
        fireCooldown = Mathf.Max(0.05f, fireCooldown - amount);
    }

    public void AddBomb(int amount)
    {
        currentBombs = Mathf.Min(currentBombs + amount, maxBombs);
        UpdateBombUI();
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private System.Collections.IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        float bonus = moveSpeed * (multiplier - 1f);
        moveSpeed += bonus;

        yield return new WaitForSeconds(duration);

        moveSpeed -= bonus;
    }
}