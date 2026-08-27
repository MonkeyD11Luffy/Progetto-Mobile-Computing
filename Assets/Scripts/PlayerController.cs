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
    [SerializeField] private float healthRegenInterval = 5f;
    [SerializeField] private float healthRegenChance = 0f;
    [SerializeField] private int contactDamageReduction = 0;

    [Header("Potenziamenti Sparo")]
    [SerializeField] private int projectileBounces = 0;

    [Header("Feedback danno")]
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private float hitShakeDuration = 0.15f;
    [SerializeField] private float hitShakeMagnitude = 0.1f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI bombText;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private TextMeshProUGUI upgradePopupText;
    [SerializeField] private float upgradePopupDuration = 1.5f;
    [SerializeField] private Transform healthIconContainer;
    [SerializeField] private GameObject healthIconPrefab;

    [Header("Feedback")]
    [SerializeField] private Transform meleeVisual;
    [SerializeField] private float meleeVisualDuration = 0.1f;
    [SerializeField] private PlayerVisuals playerVisuals;


    private enum WeaponType { Single, Spread, Piercing, Melee }
    private WeaponType currentWeapon = WeaponType.Single;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastAimDirection = Vector2.right;
    // Ultima direzione di movimento, che resta anche a player fermo: lo scatto
    // parte da qui, e lastAimDirection non servirebbe perché cambia solo
    // sparando. In giù come le altre pose iniziali: è il verso in cui il player
    // guarda appena nato, quindi anche uno scatto senza storia va dove sembra.
    private Vector2 lastMoveDirection = Vector2.down;
    private float fireTimer;
    private int currentHealth;
    private int currentBombs;
    private int currentCredits;
    private float bombTimer;
    private bool isInvulnerable;
    private bool isDead;
    private float invulnerabilityTimer;
    private float healthRegenTimer;
    private Color baseSpriteColor;
    private Coroutine flashCoroutine;
    private Coroutine upgradePopupCoroutine;

    // Stato comandato da PlayerAbilities
    private bool hasMovementOverride;
    private Vector2 movementOverride;
    private int abilityImmunitySources;
    private float abilitySpeedBonus;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        currentBombs = maxBombs;
        healthRegenTimer = healthRegenInterval;
        if (sr != null) baseSpriteColor = sr.color;
        if (upgradePopupText != null) upgradePopupText.gameObject.SetActive(false);
        UpdateBombUI();
        UpdateWeaponUI();
        UpdateHealthUI();
        UpdateCreditUI();
    }

    private void Update()
    {
        // Time.timeScale = 0 ferma la fisica ma non Update(): senza questo
        // si può ancora sparare e cambiare arma dalle schermate di fine partita
        if (isDead || Time.timeScale == 0f) return;

        HandleInput();
        HandleWeaponSwitch();
        HandleFiring();
        HandleBombPlacement();
        HandleInvulnerability();
        HandleHealthRegen();
    }

    private void HandleHealthRegen()
    {
        if (healthRegenChance <= 0f || currentHealth >= maxHealth) return;

        healthRegenTimer -= Time.deltaTime;
        if (healthRegenTimer > 0f) return;

        healthRegenTimer = healthRegenInterval;

        if (Random.value < healthRegenChance)
        {
            Heal(1);
        }
    }

    private void FixedUpdate()
    {
        // Durante uno scatto il movimento normale è scavalcato: senza, questa
        // riga riscriverebbe la velocità dello scatto a ogni passo di fisica, e
        // chi vince dipenderebbe dall'ordine di esecuzione degli script
        rb.linearVelocity = hasMovementOverride ? movementOverride : moveInput * moveSpeed;
    }

    // --- comandi delle abilità (PlayerAbilities) -----------------------------

    // Movimento imposto dall'esterno, usato dallo scatto. Finché è attivo il
    // player non risponde a WASD: la velocità la decide chi ha chiamato.
    public void SetMovementOverride(Vector2 velocity)
    {
        movementOverride = velocity;
        hasMovementOverride = true;
    }

    public void ClearMovementOverride()
    {
        hasMovementOverride = false;
        movementOverride = Vector2.zero;
    }

    // Immunità concessa da un'abilità (scudo, scatto), tenuta separata da
    // isInvulnerable, che è quella dopo un colpo e si porta dietro lampeggio e
    // timer. È un contatore e non un bool perché scatto e scudo possono
    // sovrapporsi: con un bool, la fine del primo spegnerebbe anche il secondo.
    public void SetAbilityImmunity(bool active)
    {
        abilityImmunitySources = Mathf.Max(0, abilityImmunitySources + (active ? 1 : -1));
    }

    public bool IsImmune => abilityImmunitySources > 0;

    // Moltiplicatore di velocità a comando, per compensare il rallentamento del
    // tempo. Diverso da ApplySpeedBoost, che se lo toglie da solo dopo una
    // durata: qui la fine la decide l'abilità.
    public void SetAbilitySpeedMultiplier(float multiplier)
    {
        // Il bonus precedente si toglie prima: due chiamate di fila non devono
        // sommarsi, e nel frattempo moveSpeed può essere cambiato da un pickup
        ClearAbilitySpeedMultiplier();

        abilitySpeedBonus = moveSpeed * (multiplier - 1f);
        moveSpeed += abilitySpeedBonus;
    }

    public void ClearAbilitySpeedMultiplier()
    {
        moveSpeed -= abilitySpeedBonus;
        abilitySpeedBonus = 0f;
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

        // Solo quando ci si muove davvero: al rilascio dei tasti moveInput torna
        // a zero, e sovrascriverla lì cancellerebbe proprio il ricordo che serve
        if (moveInput != Vector2.zero) lastMoveDirection = moveInput;
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

        if (aimDirection != Vector2.zero)
        {
            lastAimDirection = aimDirection;

            if (fireTimer <= 0f)
            {
                Fire(aimDirection);
                fireTimer = GetCurrentCooldown();
            }
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

    Vector2 origin = playerVisuals != null
        ? playerVisuals.MuzzlePosition
        : (Vector2)transform.position + direction * firePointDistance;

    switch (currentWeapon)
    {
        case WeaponType.Single:
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSingle();
            SpawnProjectile(origin, direction, false);
            break;

        case WeaponType.Spread:
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSpread();
            SpawnProjectile(origin, direction, false, spreadLifetime);
            SpawnProjectile(origin, VectorUtils.Rotate(direction, spreadAngle), false, spreadLifetime);
            SpawnProjectile(origin, VectorUtils.Rotate(direction, -spreadAngle), false, spreadLifetime);
            break;

        case WeaponType.Piercing:
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShootPierce();
            SpawnProjectile(origin, direction, true);
            break;
    }
}
   
    private void SpawnProjectile(Vector2 origin, Vector2 direction, bool piercing, float lifetime = -1f)
    {
        // Come la bomba: figlio della stanza corrente e non della radice della
        // scena. Sparando e cambiando stanza subito, un colpo appeso alla radice
        // continuerebbe a volare nella stanza nuova, dove ActivateOnly non può
        // spegnerlo perché non appartiene a nessuna stanza.
        GameObject projectile = Instantiate(projectilePrefab, origin, Quaternion.identity, CurrentRoomTransform());

        ProjectileController projController = projectile.GetComponent<ProjectileController>();
        if (projController != null)
        {
            projController.SetDamage(projectileDamage);
            projController.SetPiercing(piercing);
            projController.SetBounces(projectileBounces);

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

        projectile.transform.rotation = Quaternion.Euler(0, 0, VectorUtils.ToAngle(direction));
    }

    // Genitore degli oggetti creati a runtime (proiettili, bombe): senza
    // RoomManager in scena si torna alla radice, che è quello che Instantiate
    // fa già con un genitore nullo.
    private static Transform CurrentRoomTransform()
    {
        return RoomManager.Instance != null ? RoomManager.Instance.CurrentRoomTransform : null;
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
        meleeVisual.rotation = Quaternion.Euler(0, 0, VectorUtils.ToAngle(direction));
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

        // Figlia della stanza corrente: appesa alla radice della scena
        // continuerebbe a esplodere anche dopo essere passati di là
        Instantiate(bombPrefab, transform.position, Quaternion.identity, CurrentRoomTransform());
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

    public void TakeContactDamage(int amount)
    {
        int reduced = Mathf.Max(0, amount - contactDamageReduction);
        if (reduced <= 0) return;

        TakeDamage(reduced);
    }

    public void TakeDamage(int amount)
{
    // IsImmune è lo scudo (o lo scatto): assorbe tutto senza lampeggio e senza
    // consumare i frame di invulnerabilità post-colpo
    if (isInvulnerable || isDead || IsImmune) return;

    currentHealth -= amount;
    UpdateHealthUI();

    if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHurt();
    if (CameraShake.Instance != null) CameraShake.Instance.Shake(hitShakeDuration, hitShakeMagnitude);
    PlayHitFlash();

    isInvulnerable = true;
    invulnerabilityTimer = invulnerabilityDuration;

    if (currentHealth <= 0)
    {
        Die();
    }
}

    private void PlayHitFlash()
    {
        if (sr == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private System.Collections.IEnumerator HitFlashRoutine()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(hitFlashDuration);
        sr.color = baseSpriteColor;
        flashCoroutine = null;
    }

    private void Die()
    {
        isDead = true;
        moveInput = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Debug.Log("Game Over");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOver();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthIconContainer != null && healthIconPrefab != null)
        {
            // maxHealth cresce a runtime (IncreaseMaxHealth): le icone mancanti
            // vengono create qui, quelle gia' esistenti sono riusate.
            while (healthIconContainer.childCount < maxHealth)
            {
                Instantiate(healthIconPrefab, healthIconContainer);
            }

            for (int i = 0; i < healthIconContainer.childCount; i++)
            {
                healthIconContainer.GetChild(i).gameObject.SetActive(i < currentHealth);
            }
        }
        else if (healthText != null)
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

    private void UpdateCreditUI()
    {
        if (creditText != null)
        {
            creditText.text = $"Crediti: {currentCredits}";
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

    public int Credits => currentCredits;

    public void AddCredits(int amount)
    {
        if (amount <= 0) return;

        currentCredits += amount;
        UpdateCreditUI();
    }

    // false se non bastano, e in quel caso non toglie niente: sta al chiamante
    // (un negozio, una porta a pagamento) decidere cosa fare del rifiuto
    public bool SpendCredits(int amount)
    {
        if (amount <= 0) return true; // gratis: niente da pagare
        if (currentCredits < amount) return false;

        currentCredits -= amount;
        UpdateCreditUI();
        return true;
    }

    public void AddBomb(int amount)
    {
        currentBombs = Mathf.Min(currentBombs + amount, maxBombs);
        UpdateBombUI();
    }

    public void IncreaseHealthRegenChance(float amount)
    {
        healthRegenChance = Mathf.Clamp01(healthRegenChance + amount);
    }

    public void IncreaseContactDamageReduction(int amount)
    {
        contactDamageReduction += amount;
    }

    public void WidenMeleeArc(float amount)
    {
        meleeArc = Mathf.Max(-1f, meleeArc - amount);
    }

    public void IncreaseProjectileBounces(int amount)
    {
        projectileBounces += amount;
    }

    public void ShowUpgradePopup(string message)
    {
        if (upgradePopupText == null) return;

        if (upgradePopupCoroutine != null) StopCoroutine(upgradePopupCoroutine);
        upgradePopupCoroutine = StartCoroutine(UpgradePopupRoutine(message));
    }

    private System.Collections.IEnumerator UpgradePopupRoutine(string message)
    {
        upgradePopupText.text = message;
        upgradePopupText.gameObject.SetActive(true);

        yield return new WaitForSeconds(upgradePopupDuration);

        upgradePopupText.gameObject.SetActive(false);
        upgradePopupCoroutine = null;
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

    // Sola lettura, per l'animazione (PlayerVisuals)
    public Vector2 MoveInput => moveInput;
    public Vector2 AimInput => GetCardinalAimDirection();
    public Vector2 LastAimDirection => lastAimDirection;
    // La usa PlayerAbilities per lo scatto da fermo
    public Vector2 LastMoveDirection => lastMoveDirection;
    public int CurrentWeaponIndex => (int)currentWeapon;

    // La usa PlayerAbilities per la stessa guardia che c'è in cima a Update()
    public bool IsDead => isDead;
}