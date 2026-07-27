using System.Collections;
using UnityEngine;

// Classe base comune a tutti i nemici: vita, morte, danno da contatto,
// riferimento al player e utilità per sparare.
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Vita")]
    [SerializeField] protected int maxHealth = 3;

    [Header("Danno da contatto")]
    [SerializeField] protected int contactDamage = 1; // 0 = non fa danno toccando il player

    [Header("Feedback")]
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private float deathEffectDuration = 0.2f;

    protected Rigidbody2D rb;
    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    protected int currentHealth;
    protected bool isDead;
    protected bool isFlashing;

    private Color baseSpriteColor;
    private Coroutine flashCoroutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) baseSpriteColor = spriteRenderer.color;
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Pubblico: viene chiamato via SendMessage da ProjectileController e BombController
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        PlayHitFlash();

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    private void PlayHitFlash()
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        isFlashing = true;
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(hitFlashDuration);

        isFlashing = false;
        RestoreBaseColor();
        flashCoroutine = null;
    }

    // Sovrascritto da nemici che gestiscono il proprio colore (fasi, rabbia, ecc.)
    // per evitare che il flash bianco venga ripristinato con il colore sbagliato.
    protected virtual void RestoreBaseColor()
    {
        if (spriteRenderer != null) spriteRenderer.color = baseSpriteColor;
    }

    protected virtual void Die()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemyDeath();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathPopRoutine());
        Destroy(gameObject, deathEffectDuration);
    }

    private IEnumerator DeathPopRoutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 popScale = startScale * 1.3f;
        float half = deathEffectDuration * 0.5f;

        float t = 0f;
        while (t < half)
        {
            transform.localScale = Vector3.Lerp(startScale, popScale, t / half);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            transform.localScale = Vector3.Lerp(popScale, Vector3.zero, t / half);
            t += Time.deltaTime;
            yield return null;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected void TryDamagePlayer(GameObject other)
    {
        if (isDead || contactDamage <= 0) return;
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeContactDamage(contactDamage);
        }
    }

    // Utilità condivisa: usata da Turret, Teleporter, Miniboss e Boss
    protected void SpawnProjectile(GameObject prefab, Vector2 origin, Vector2 direction, float speed)
    {
        if (prefab == null) return;

        GameObject projectile = Instantiate(prefab, origin, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
