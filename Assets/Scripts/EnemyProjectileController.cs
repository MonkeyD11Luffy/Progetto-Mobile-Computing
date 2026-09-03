using UnityEngine;

public class EnemyProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Il player si riconosce dal componente, non dal tag: cosi' viene
        // intercettato anche quando il collider colpito e' un figlio (la
        // hurtbox), che il tag "Player" non ce l'ha.
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}