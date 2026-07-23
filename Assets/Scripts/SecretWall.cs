using UnityEngine;

public class SecretWall : MonoBehaviour
{
    [Header("Destinazione")]
    [SerializeField] private GameObject targetRoom;
    [SerializeField] private Vector2 playerSpawnPosition;

    private bool isRevealed = false;

    public void Destroy()
    {
        if (isRevealed) return; // evita di rifare tutto se colpito più volte

        isRevealed = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.3f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isRevealed) return; // il muro deve essere già stato rotto dalla bomba

        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.GoToRoom(targetRoom, playerSpawnPosition);
        }
    }
}