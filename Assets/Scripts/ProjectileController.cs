using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private int damage = 1;
    private bool piercing = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    public void SetPiercing(bool value)
    {
        piercing = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            ShieldedController shielded = other.GetComponent<ShieldedController>();
            if (shielded != null && shielded.IsBlocked(transform.position))
            {
                Destroy(gameObject); // lo scudo ferma anche i proiettili perforanti
                return;
            }

            other.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            if (!piercing)
            {
                Destroy(gameObject);
            }
        }
    }
}