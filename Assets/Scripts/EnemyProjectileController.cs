using UnityEngine;

public class EnemyProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Il proiettile e' figlio della stanza: quando ActivateOnly la spegne al
    // cambio stanza verrebbe congelato a mezz'aria invece di sparire.
    // La guardia su scene.isLoaded evita che scatti all'uscita dal Play mode
    // o durante lo scaricamento della scena.
    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) return;

        Destroy(gameObject);
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