using UnityEngine;

public class SecretWall : MonoBehaviour
{
    [Header("Destinazione")]
    [SerializeField] private GameObject targetRoom;
    [SerializeField] private Vector2 playerSpawnPosition;

    private bool isRevealed = false;

    // NB: non chiamare questo metodo "Destroy": nasconderebbe Object.Destroy
    // dentro questa classe e renderebbe impossibile chiamarlo senza qualificarlo.
    public void Reveal()
    {
        if (isRevealed) return;

        isRevealed = true;

        // Il collider resta solido: il passaggio avviene per collisione, non
        // per trigger. Rendendolo trigger, un GoToRoom ignorato (doorIgnoreTimer
        // ancora attivo, o player già dentro al collider) lasciava passare il
        // giocatore attraverso il muro, fuori dai confini della stanza.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.3f);
        }

        // Spawna un potenziamento casuale al centro della stanza segreta
        if (RoomManager.Instance != null && targetRoom != null)
        {
            RoomManager.Instance.SpawnRandomPermanentUpgrade(targetRoom.transform.position, targetRoom);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryEnterSecretRoom(collision.gameObject);
    }

    // Se GoToRoom viene ignorato per doorIgnoreTimer, al frame successivo il
    // player è ancora appoggiato al muro e il passaggio riparte da solo,
    // invece di essere perso per sempre.
    private void OnCollisionStay2D(Collision2D collision)
    {
        TryEnterSecretRoom(collision.gameObject);
    }

    private void TryEnterSecretRoom(GameObject other)
    {
        if (!isRevealed) return; // il muro deve essere già stato rotto dalla bomba

        if (!other.CompareTag("Player")) return;

        if (RoomManager.Instance == null || targetRoom == null) return;

        RoomManager.Instance.GoToRoom(targetRoom, playerSpawnPosition);
    }
}
