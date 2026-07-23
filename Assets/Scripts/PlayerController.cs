using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    [Header("Bombe")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int maxBombs = 3;
    [SerializeField] private float bombCooldown = 1f;

    private int currentBombs;
    private float bombTimer;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireCooldown = 0.3f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private float invulnerabilityDuration = 0.8f;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private float fireTimer;
    private int currentHealth;
    private bool isInvulnerable;
    private float invulnerabilityTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentBombs = maxBombs;
        UpdateHealthUI(); // NUOVO
    }

    private void Update()
    {
        HandleInput();
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

private void HandleFiring()
{
    fireTimer -= Time.deltaTime;

    Vector2 aimDirection = GetCardinalAimDirection();

    if (aimDirection != Vector2.zero && fireTimer <= 0f)
    {
        Fire(aimDirection);
        fireTimer = fireCooldown;
    }
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
    if (projectilePrefab == null || firePoint == null)
    {
        Debug.LogWarning("Assegna projectilePrefab e firePoint nell'Inspector.");
        return;
    }

    GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
    Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
    if (projRb != null)
    {
        projRb.linearVelocity = direction * projectileSpeed;
    }

    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
}


    private void HandleInvulnerability()
    {
        if (!isInvulnerable) return;

        invulnerabilityTimer -= Time.deltaTime;
        if (invulnerabilityTimer <= 0f)
        {
            isInvulnerable = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;
        UpdateHealthUI();
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        Debug.Log($"Player colpito. Vita rimanente: {currentHealth}");

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyProjectile"))
        {
            TakeDamage(1);
        }
    }

    private void UpdateHealthUI()
{
    if (healthText != null)
    {
        healthText.text = $"Vita: {currentHealth}/{maxHealth}";
    }
}

public void Heal(int amount)
{
    currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    UpdateHealthUI();
}

public void AddBomb(int amount)
{
    currentBombs = Mathf.Min(currentBombs + amount, maxBombs);
}

public void ApplySpeedBoost(float multiplier, float duration)
{
    StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
}

private System.Collections.IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
{
    float originalSpeed = moveSpeed;
    moveSpeed *= multiplier;

    yield return new WaitForSeconds(duration);

    moveSpeed = originalSpeed;
}

}
