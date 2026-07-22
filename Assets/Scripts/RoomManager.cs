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

    // NUOVO
    enemiesRemaining = CountEnemiesInRoom(currentRoom);
    SetDoorsActive(currentRoom, enemiesRemaining <= 0);
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

    // NUOVO: controlla nemici nella stanza appena entrata
    enemiesRemaining = CountEnemiesInRoom(newRoom);
    SetDoorsActive(newRoom, enemiesRemaining <= 0);
}

    private void ActivateOnly(GameObject roomToActivate)
    {
        // Disattiva tutte le stanze figlie di questo RoomManager
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.gameObject == roomToActivate);
        }
    }

    private int enemiesRemaining;

public void RegisterEnemyDeath()
{
    enemiesRemaining--;

    if (enemiesRemaining <= 0)
    {
        SetDoorsActive(currentRoom, true);
    }
}

private void SetDoorsActive(GameObject room, bool active)
{
    foreach (Transform child in room.transform)
    {
        if (child.CompareTag("Door"))
        {
            child.gameObject.SetActive(active);
        }
    }
}

private int CountEnemiesInRoom(GameObject room)
{
    int count = 0;
    foreach (Transform child in room.transform)
    {
        if (child.CompareTag("Enemy"))
        {
            count++;
        }
    }
    return count;
}

}
