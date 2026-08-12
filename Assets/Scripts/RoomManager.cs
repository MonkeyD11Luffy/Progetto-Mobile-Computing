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

    [Header("Posizionamento dei drop")]
    // Spazio che il pickup deve avere libero attorno a sé
    [SerializeField] private float pickupRadius = 0.3f;
    // Di quanto ci si allontana dal centro a ogni tentativo fallito
    [SerializeField] private float placementStep = 0.5f;
    [SerializeField] private int maxPlacementAttempts = 12;

    [Header("Crediti")]
    [SerializeField] private GameObject creditPrefab;
    // Raggio entro cui i crediti si sparpagliano attorno al punto di morte:
    // cadendo tutti nello stesso punto sembrerebbero uno solo
    [SerializeField] private float creditScatter = 0.35f;

    [Header("Potenziamenti Permanenti")]
    [SerializeField] private GameObject[] permanentUpgradePrefabs;

    // Li legge il DungeonGenerator per riempire i piedistalli della stanza
    // tesoro: la lista è una sola e sta qui, dove la usano già i drop dei boss
    public GameObject[] PermanentUpgradePrefabs => permanentUpgradePrefabs;

    [Header("Minimappa")]
    // Facoltativa: senza, il gioco funziona esattamente come prima
    [SerializeField] private MinimapController minimap;

    [Header("Camera")]
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;
    [SerializeField] private Vector2Int defaultReferenceResolution = new Vector2Int(240, 135);

    [Header("Transizioni")]
    // Le porte vengono riattivate quando la stanza si libera: se il player è
    // fermo sopra una porta, il suo trigger scatterebbe subito. Per questo
    // ogni cambio stanza e ogni sblocco ignorano le porte per qualche frame.
    [SerializeField] private float doorIgnoreDelay = 0.25f;

    private const string WallTag = "Wall";

    private GameObject currentRoom;
    private int enemiesRemaining;
    private bool roomCleared;
    private bool victoryShown;
    private float doorIgnoreTimer;

    // La stanza attiva: serve a chi crea oggetti a runtime per appenderli lì
    // invece che alla radice della scena, dove sopravvivrebbero al cambio stanza
    public GameObject CurrentRoom => currentRoom;

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

    // Stanze assegnate da codice quando il dungeon è generato a runtime:
    // vanno impostate prima di Start(), che entra nella stanza iniziale.
    public void SetStartingRoom(GameObject room)
    {
        startingRoom = room;
    }

    public void SetFinalRoom(GameObject room)
    {
        finalRoom = room;
    }

    public void EnterRoom(GameObject room)
    {
        ActivateOnly(room);
        ApplyCameraFor(room);
        currentRoom = room;
        doorIgnoreTimer = doorIgnoreDelay;

        enemiesRemaining = CountEnemiesInRoom(room);
        roomCleared = enemiesRemaining <= 0;

        ApplyRoomMusic(room, roomCleared);

        SetDoorsActive(room, roomCleared);

        // Una stanza finale già vuota (o ripulita in una visita precedente)
        // deve comunque far vincere la partita
        if (roomCleared) CheckVictory();

        if (minimap != null) minimap.SetCurrentRoom(room);
    }

    public void RegisterEnemyDeath()
    {
        if (enemiesRemaining > 0) enemiesRemaining--;

        // roomCleared evita che un conteggio sfasato faccia cadere più pickup
        if (enemiesRemaining > 0 || roomCleared || currentRoom == null) return;

        roomCleared = true;
        ApplyRoomMusic(currentRoom, true);
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
            ApplyRoomMusic(currentRoom, false);
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

    // I crediti nascono figli della stanza corrente e non della radice della
    // scena: ActivateOnly spegne le stanze che il player lascia, e appesi alla
    // radice resterebbero visibili e raccoglibili da qualsiasi altra stanza.
    public void SpawnCredits(Vector3 position, int amount)
    {
        if (creditPrefab == null || amount <= 0) return;

        Transform parent = currentRoom != null ? currentRoom.transform : null;

        for (int i = 0; i < amount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * creditScatter;
            Instantiate(creditPrefab, position + (Vector3)offset, Quaternion.identity, parent);
        }
    }

    public void SpawnRandomPermanentUpgrade(Vector3 position, GameObject parentRoom)
    {
        if (permanentUpgradePrefabs == null || permanentUpgradePrefabs.Length == 0) return;

        int index = Random.Range(0, permanentUpgradePrefabs.Length);
        GameObject chosenUpgrade = permanentUpgradePrefabs[index];

        Transform parent = parentRoom != null ? parentRoom.transform : null;
        Instantiate(chosenUpgrade, FindFreePosition(position), Quaternion.identity, parent);
    }

    // Il centro della stanza può essere occupato da un ostacolo: in quel caso
    // si prova sempre più lontano, in direzioni casuali. Se non si trova nulla
    // il drop resta al centro: meglio un pickup scomodo che nessun pickup.
    private Vector3 FindFreePosition(Vector3 center)
    {
        if (IsFree(center)) return center;

        for (int i = 1; i <= maxPlacementAttempts; i++)
        {
            Vector2 offset = VectorUtils.FromAngle(Random.Range(0f, 360f)) * (placementStep * i);
            Vector3 candidate = center + (Vector3)offset;

            if (IsFree(candidate)) return candidate;
        }

        return center;
    }

    private bool IsFree(Vector3 position)
    {
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(position, pickupRadius))
        {
            if (hit.CompareTag(WallTag)) return false;
        }

        return true;
    }

    // La musica dipende dallo stato della stanza: una traccia con i nemici
    // vivi, un'altra quando è liberata. Una stanza senza RoomMusic chiede
    // silenzio, così non si trascina dietro la musica della stanza precedente.
    private void ApplyRoomMusic(GameObject room, bool cleared)
    {
        if (AudioManager.Instance == null || room == null) return;

        RoomMusic music = room.GetComponent<RoomMusic>();
        AudioManager.Instance.PlayMusic(music != null ? music.ClipFor(cleared) : null);
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

        Vector3 spawnPosition = FindFreePosition(room.transform.position);
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
