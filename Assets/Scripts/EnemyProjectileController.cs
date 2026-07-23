using UnityEngine;

public class EnemyProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(1);
            }
            Destroy(gameObject);
        }
    }
}
