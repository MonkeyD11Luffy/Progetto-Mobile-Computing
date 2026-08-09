using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Stanze")]
    [SerializeField] private GameObject startingRoom;

    [Header("Vittoria")]
    [SerializeField] private GameObject finalRoom;

    [Header("Pickup")]
    [SerializeField] private GameObject[] pickupPrefabs;
    [SerializeField] [Range(0f, 1f)] private float pickupDropChance = 0.5f;

    [Header("Potenziamenti Permanenti")]
    [SerializeField] private GameObject[] permanentUpgradePrefabs;

    [Header("Camera")]
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;
    [SerializeField] private Vector2Int defaultReferenceResolution = new Vector2Int(240, 135);

    [Header("Transizioni")]
    // Le porte vengono riattivate quando la stanza si libera: se il player è
    // fermo sopra una porta, il suo trigger scatterebbe subito. Per questo
    // ogni cambio stanza e ogni sblocco ignorano le porte per qualche frame.
    [SerializeField] private float doorIgnoreDelay = 0.25f;

    private GameObject currentRoom;
    private int enemiesRemaining;
    private bool roomCleared;
    private bool victoryShown;
    private float doorIgnoreTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (startingRoom == null)
        {
            Debug.LogWarning("Assegna startingRoom nell'Inspector.");
            return;
        }

        EnterRoom(startingRoom);
    }

    private void Update()
    {
        // unscaled: il tempo si ferma sulle schermate di fine partita
        if (doorIgnoreTimer > 0f) doorIgnoreTimer -= Time.unscaledDeltaTime;
    }

    public void GoToRoom(GameObject newRoom, Vector2 playerSpawnPosition)
    {
        if (newRoom == null || doorIgnoreTimer > 0f) return;

        EnterRoom(newRoom);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerSpawnPosition;
        }
    }

    private void EnterRoom(GameObject room)
    {
        ActivateOnly(room);
        ApplyCameraFor(room);
        currentRoom = room;
        doorIgnoreTimer = doorIgnoreDelay;

        enemiesRemaining = CountEnemiesInRoom(room);
        roomCleared = enemiesRemaining <= 0;

        SetDoorsActive(room, roomCleared);

        // Una stanza finale già vuota (o ripulita in una visita precedente)
        // deve comunque far vincere la partita
        if (roomCleared) CheckVictory();
    }

    public void RegisterEnemyDeath()
    {
        if (enemiesRemaining > 0) enemiesRemaining--;

        // roomCleared evita che un conteggio sfasato faccia cadere più pickup
        if (enemiesRemaining > 0 || roomCleared || currentRoom == null) return;

        roomCleared = true;
        SetDoorsActive(currentRoom, true);
        doorIgnoreTimer = doorIgnoreDelay;

        SpawnRandomPickup(currentRoom);
        CheckVictory();
    }

    public void RegisterEnemySpawn(int amount)
    {
        if (amount <= 0) return;

        enemiesRemaining += amount;

        // Se la stanza risultava già libera, va richiusa
        if (roomCleared && currentRoom != null)
        {
            roomCleared = false;
            SetDoorsActive(currentRoom, false);
        }
    }

    private void CheckVictory()
    {
        if (victoryShown || finalRoom == null || currentRoom != finalRoom) return;

        victoryShown = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowVictory();
        }
    }

    public void SpawnRandomPermanentUpgrade(Vector3 position, GameObject parentRoom)
    {
        if (permanentUpgradePrefabs == null || permanentUpgradePrefabs.Length == 0) return;

        int index = Random.Range(0, permanentUpgradePrefabs.Length);
        GameObject chosenUpgrade = permanentUpgradePrefabs[index];

        Transform parent = parentRoom != null ? parentRoom.transform : null;
        Instantiate(chosenUpgrade, position, Quaternion.identity, parent);
    }

    // Ogni stanza può chiedere una risoluzione di riferimento diversa
    // aggiungendo un RoomCameraSettings; senza, vale quella di default.
    private void ApplyCameraFor(GameObject room)
    {
        if (pixelPerfectCamera == null) return;

        Vector2Int resolution = defaultReferenceResolution;

        RoomCameraSettings settings = room.GetComponent<RoomCameraSettings>();
        if (settings != null) resolution = settings.ReferenceResolution;

        pixelPerfectCamera.refResolutionX = resolution.x;
        pixelPerfectCamera.refResolutionY = resolution.y;
    }

    private void ActivateOnly(GameObject roomToActivate)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.gameObject == roomToActivate);
        }
    }

    private void SpawnRandomPickup(GameObject room)
    {
        if (pickupPrefabs == null || pickupPrefabs.Length == 0) return;

        if (Random.value > pickupDropChance) return;

        int index = Random.Range(0, pickupPrefabs.Length);
        GameObject chosenPickup = pickupPrefabs[index];

        Vector3 spawnPosition = room.transform.position;
        Instantiate(chosenPickup, spawnPosition, Quaternion.identity, room.transform);
    }

    // Porte e nemici vengono cercati a qualsiasi profondità: così restano
    // validi anche se in futuro vengono raggruppati in un contenitore
    private void SetDoorsActive(GameObject room, bool active)
    {
        SetDoorsActive(room.transform, active);
    }

    private void SetDoorsActive(Transform parent, bool active)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Door"))
            {
                child.gameObject.SetActive(active);
                continue; // una porta non contiene altre porte
            }

            SetDoorsActive(child, active);
        }
    }

    private int CountEnemiesInRoom(GameObject room)
    {
        return CountEnemies(room.transform);
    }

    private int CountEnemies(Transform parent)
    {
        int count = 0;

        foreach (Transform child in parent)
        {
            if (child.CompareTag("Enemy")) count++;
            count += CountEnemies(child);
        }

        return count;
    }
}
