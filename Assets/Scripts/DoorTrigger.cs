using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public enum Direction { North, South, East, West }

    [Header("Posizione nella stanza")]
    [SerializeField] private Direction direction;
    // Dove va messo il player quando entra IN QUESTA stanza attraverso
    // QUESTA porta (cioè arrivando dalla stanza adiacente).
    [SerializeField] private Vector2 arrivalPosition;

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

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player") && RoomManager.Instance != null)
    {
        RoomManager.Instance.GoToRoom(targetRoom, playerSpawnPosition);
    }
}
}
