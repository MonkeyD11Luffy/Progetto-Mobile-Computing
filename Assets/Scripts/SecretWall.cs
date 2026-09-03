using UnityEngine;

public class SecretWall : MonoBehaviour
{
    [Header("Destinazione")]
    [SerializeField] private GameObject targetRoom;
    [SerializeField] private Vector2 playerSpawnPosition;

    // Oggetto del varco, tenuto spento finché il muro è integro e acceso da
    // Reveal(). Lo assegna il DungeonGenerator via SetRevealedObject: il
    // componente è aggiunto a runtime, quindi non è configurabile nell'Inspector.
    private GameObject revealedObject;

    // Trigger sulla porta rivelata, quando il suo prefab lo dichiara. Se c'è,
    // è lui a far entrare nella stanza segreta e il muro smette di reagire alle
    // proprie collisioni; se manca, si resta al passaggio per collisione.
    private SecretDoorTrigger doorTrigger;

    private bool isRevealed = false;

    // Come Connect: configurazione dall'esterno, per lo stesso motivo.
    public void SetRevealedObject(GameObject revealed)
    {
        revealedObject = revealed;

        // La porta è spenta fino a Reveal(), quindi va cercata anche inattiva
        if (revealed != null) SetDoorTrigger(revealed.GetComponentInChildren<SecretDoorTrigger>(true));
    }

    // Collegamento nei due sensi: il muro sa a chi ha delegato l'ingresso, la
    // porta sa in quale muro entrare. Con trigger null non cambia niente e il
    // passaggio resta quello per collisione.
    public void SetDoorTrigger(SecretDoorTrigger trigger)
    {
        doorTrigger = trigger;

        if (doorTrigger != null) doorTrigger.SetWall(this);
    }

    // Come in DoorTrigger: collega il muro da codice invece che nell'Inspector.
    public void Connect(GameObject room, Vector2 spawnPosition)
    {
        targetRoom = room;
        playerSpawnPosition = spawnPosition;
    }

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
        if (revealedObject != null)
        {
            // Il varco è già in posizione dalla generazione, spento: qui si
            // accende. Con una grafica dedicata il muro resta com'è: è il varco
            // a far vedere che si è aperto, non l'alpha abbassato.
            revealedObject.SetActive(true);
        }
        else
        {
            // Ripiego senza grafica del varco: il muro si smaterializza
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.3f);
            }
        }

        // Spawna un potenziamento casuale al centro della stanza segreta
        if (RoomManager.Instance != null && targetRoom != null)
        {
            RoomManager.Instance.SpawnRandomPermanentUpgrade(targetRoom.transform.position, targetRoom);
        }
    }

    // Passaggio per collisione: resta solo come ripiego, per un muro segreto
    // senza porta con SecretDoorTrigger. Con la porta collegata è lei a far
    // entrare, e il collider del muro torna a fare solo da muro.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryEnterFromWall(collision.gameObject);
    }

    // Se GoToRoom viene ignorato per doorIgnoreTimer, al frame successivo il
    // player è ancora appoggiato al muro e il passaggio riparte da solo,
    // invece di essere perso per sempre.
    private void OnCollisionStay2D(Collision2D collision)
    {
        TryEnterFromWall(collision.gameObject);
    }

    private void TryEnterFromWall(GameObject other)
    {
        if (doorTrigger != null) return; // ci pensa la porta

        if (!other.CompareTag("Player")) return;

        TryEnterSecretRoom();
    }

    // Pubblico: lo chiama anche il SecretDoorTrigger della porta rivelata.
    // Il riconoscimento del player sta a chi chiama, le guardie stanno qui.
    public void TryEnterSecretRoom()
    {
        if (!isRevealed) return; // il muro deve essere già stato rotto dalla bomba

        if (RoomManager.Instance == null || targetRoom == null) return;

        // Stessa serratura delle porte: con i nemici ancora vivi non si esce.
        // Le porte si chiudono da sole (SetDoorsActive spegne il collider), un
        // muro no, quindi senza questo il muro segreto sarebbe la via di fuga
        // da una stanza che dovrebbe essere chiusa.
        if (!RoomManager.Instance.IsCurrentRoomCleared) return;

        RoomManager.Instance.GoToRoom(targetRoom, playerSpawnPosition);
    }
}
