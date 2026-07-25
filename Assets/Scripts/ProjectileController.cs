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
        // Se il nemico ha uno scudo, controlla se il colpo arriva dal lato protetto
        ShieldedController shielded = other.GetComponent<ShieldedController>();
        if (shielded != null && shielded.IsBlocked(transform.position))
        {
            Destroy(gameObject); // il proiettile si distrugge ma non fa danno
            return;
        }

        other.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
}