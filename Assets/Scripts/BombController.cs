using UnityEngine;

public class BombController : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float fuseTime = 2f;
    

    [Header("Esplosione")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private int explosionDamage = 3;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Animazione miccia")]
    // Dal più lungo al più corto: l'ultimo è quello mostrato allo scoppio
    [SerializeField] private Sprite[] fuseSprites;

    private SpriteRenderer sr;
    private float timer;

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);

        sr = GetComponent<SpriteRenderer>();
        timer = fuseTime;
    }

    private void Update()
    {
        if (sr == null || fuseSprites == null || fuseSprites.Length == 0) return;

        timer -= Time.deltaTime;

        // 0 all'inizio, 1 allo scoppio
        float t = Mathf.Clamp01(1f - timer / fuseTime);

        // Il Min evita di sforare l'array quando t vale esattamente 1
        int i = Mathf.Min(fuseSprites.Length - 1, Mathf.FloorToInt(t * fuseSprites.Length));

        sr.sprite = fuseSprites[i];
    }

    private void Explode()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();

        if (explosionEffectPrefab != null)
{
    Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
}
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Danneggia muri segreti
            SecretWall secretWall = hit.GetComponent<SecretWall>();
            if (secretWall != null)
            {
                secretWall.Reveal();
            }

            // Danneggia il player se troppo vicino
            if (hit.CompareTag("Player"))
            {
                PlayerController playerController = hit.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(explosionDamage);
                }
            }

            // Danneggia i nemici (Walker, Turret, Miniboss - tutti taggati "Enemy")
            if (hit.CompareTag("Enemy"))
            {
                // Ogni tipo di nemico ha un metodo TakeDamage privato diverso,
                // quindi usiamo SendMessage per chiamarlo qualunque sia lo script specifico
                hit.gameObject.SendMessage("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
            }
        }

        Destroy(gameObject);
    }

    // Utile per vedere il raggio esplosione mentre lavori in editor (visibile solo nella Scene view)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
