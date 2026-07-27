using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private int damage = 1;
    private bool piercing = false;
    private int bouncesLeft = 0;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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

    public void SetLifetime(float value)
    {
        lifetime = value;
    }

    public void SetBounces(int amount)
    {
        bouncesLeft = amount;
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
            return;
        }

        if (other.CompareTag("Wall"))
        {
            BounceOffWall(other);
        }
    }

    private void BounceOffWall(Collider2D wall)
    {
        if (bouncesLeft <= 0)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 normal = ApproximateWallNormal(wall.bounds, transform.position);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);

            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        bouncesLeft--;
    }

    // Approssima la normale del muro colpito usando il bordo dell'AABB più vicino:
    // i proiettili sono trigger, quindi non arriva un ContactPoint2D con la normale reale.
    private Vector2 ApproximateWallNormal(Bounds wallBounds, Vector2 point)
    {
        float distLeft = Mathf.Abs(point.x - wallBounds.min.x);
        float distRight = Mathf.Abs(point.x - wallBounds.max.x);
        float distBottom = Mathf.Abs(point.y - wallBounds.min.y);
        float distTop = Mathf.Abs(point.y - wallBounds.max.y);

        float minDist = Mathf.Min(Mathf.Min(distLeft, distRight), Mathf.Min(distBottom, distTop));

        if (minDist == distLeft) return Vector2.left;
        if (minDist == distRight) return Vector2.right;
        if (minDist == distBottom) return Vector2.down;
        return Vector2.up;
    }
}