using UnityEngine;

public class BombController : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float fuseTime = 2f;

    [Header("Esplosione")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private int explosionDamage = 3;

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Danneggia muri segreti
            SecretWall secretWall = hit.GetComponent<SecretWall>();
            if (secretWall != null)
            {
                secretWall.Destroy();
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
