using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Destinazione")]
    [SerializeField] private GameObject targetRoom;
    [SerializeField] private Vector2 playerSpawnPosition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.GoToRoom(targetRoom, playerSpawnPosition);
        }
    }
}
