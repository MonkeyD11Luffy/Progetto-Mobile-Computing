using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Stanze")]
    [SerializeField] private GameObject startingRoom;

    private GameObject currentRoom;

    private void Awake()
    {
        // Singleton semplice: un solo RoomManager nella scena
        Instance = this;
    }

    private void Start()
    {
        currentRoom = startingRoom;
        ActivateOnly(currentRoom);
    }

    public void GoToRoom(GameObject newRoom, Vector2 playerSpawnPosition)
    {
        ActivateOnly(newRoom);
        currentRoom = newRoom;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerSpawnPosition;
        }
    }

    private void ActivateOnly(GameObject roomToActivate)
    {
        // Disattiva tutte le stanze figlie di questo RoomManager
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.gameObject == roomToActivate);
        }
    }
}
