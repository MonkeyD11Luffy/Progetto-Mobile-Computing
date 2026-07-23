using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}