using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public enum Direction { North, South, East, West }

    [Header("Posizione nella stanza")]
    [SerializeField] private Direction direction;
    // Dove va messo il player quando entra IN QUESTA stanza attraverso
    // QUESTA porta (cioè arrivando dalla stanza adiacente).
    [SerializeField] private Vector2 arrivalPosition;

    [Header("Aspetto")]
    // Le due facce della porta. Lasciarne una vuota significa "non cambiare
    // sprite in quello stato": la porta resta com'è invece di sparire.
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;

    [Header("Destinazione")]
    [SerializeField] private GameObject targetRoom;
    [SerializeField] private Vector2 playerSpawnPosition;

    public Direction Dir => direction;
    public Vector2 ArrivalPosition => arrivalPosition;

    // Una porta senza destinazione non porta da nessuna parte: il generatore
    // del dungeon la usa per riconoscere le porte da eliminare.
    public bool IsConnected => targetRoom != null;

    // Usato per collegare le porte da codice (generazione del dungeon):
    // in alternativa i due campi si assegnano a mano nell'Inspector.
    public void Connect(GameObject room, Vector2 spawnPosition)
    {
        targetRoom = room;
        playerSpawnPosition = spawnPosition;
    }

    // Apre o chiude la porta lasciando il GameObject acceso: cambia la faccia e
    // toglie di mezzo il collider. Spegnere l'oggetto, come si faceva prima,
    // farebbe sparire anche lo sprite, e una porta chiusa deve vedersi.
    public void SetOpen(bool open)
    {
        Sprite sprite = open ? openSprite : closedSprite;

        // Sprite non assegnato: si tiene quello corrente. Una porta senza le due
        // facce configurate resta com'era, invece di diventare invisibile.
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && sprite != null) spriteRenderer.sprite = sprite;

        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null) doorCollider.enabled = open;
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player") && RoomManager.Instance != null)
    {
        RoomManager.Instance.GoToRoom(targetRoom, playerSpawnPosition);
    }
}
}
