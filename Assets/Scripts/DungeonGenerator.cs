using System.Collections.Generic;
using UnityEngine;

// Genera il dungeon a runtime: costruisce un grafo di stanze su griglia,
// istanzia i prefab, collega le porte e popola le stanze di nemici.
// Va messo sullo stesso GameObject del RoomManager.
//
// Gira in Awake() perché RoomManager entra nella stanza iniziale in Start():
// Unity esegue tutti gli Awake prima di qualsiasi Start, quindi le stanze
// esistono già quando il RoomManager parte. Per lo stesso motivo qui il
// RoomManager si prende con GetComponent e non con RoomManager.Instance,
// che potrebbe non essere ancora stato assegnato.
[RequireComponent(typeof(RoomManager))]
public class DungeonGenerator : MonoBehaviour
{
    [Header("Stanze")]
    // Il primo elemento è la stanza iniziale, gli altri sono le stanze comuni
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private GameObject bossRoomPrefab;
    [SerializeField] private GameObject minibossRoomPrefab;
    [SerializeField] private GameObject secretRoomPrefab;
    // Tinta del muro sfondabile: unico indizio che dietro c'è qualcosa
    [SerializeField] private Color secretWallTint = new Color(0.85f, 0.85f, 0.9f, 1f);
    [SerializeField] private int roomCount = 8;
    [SerializeField] private int gridSize = 9;

    [Header("Nemici")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject minibossPrefab;
    [SerializeField] private int minEnemiesPerRoom = 1;
    [SerializeField] private int maxEnemiesPerRoom = 4;

    // Esponente applicato alla distanza dalla partenza quando si sorteggia la
    // stanza del miniboss: 0 = tutte le celle equiprobabili, valori più alti
    // la spingono sempre più verso il fondo del dungeon.
    [SerializeField] private float minibossDistanceBias = 1f;

    [Header("Minimappa")]
    // Facoltativa: se non assegnata il dungeon si genera lo stesso
    [SerializeField] private MinimapController minimap;

    [Header("Seed")]
    // 0 = seed casuale, quindi un dungeon diverso a ogni partita
    [SerializeField] private int randomSeed = 0;

    // I figli di una stanza con questo prefisso nel nome sono i punti
    // in cui possono comparire i nemici.
    private const string SpawnPointPrefix = "Spawn_";

    private const string WallTag = "Wall";

    private static readonly DoorTrigger.Direction[] AllDirections =
    {
        DoorTrigger.Direction.North,
        DoorTrigger.Direction.South,
        DoorTrigger.Direction.East,
        DoorTrigger.Direction.West
    };

    // Si collega una coppia di stanze adiacenti una volta sola, guardando
    // solo verso nord e verso est: sud e ovest sono le stesse coppie viste
    // dall'altra parte.
    private static readonly DoorTrigger.Direction[] PairDirections =
    {
        DoorTrigger.Direction.North,
        DoorTrigger.Direction.East
    };

    private readonly Dictionary<Vector2Int, GameObject> rooms = new Dictionary<Vector2Int, GameObject>();
    private readonly List<GameObject> allRooms = new List<GameObject>();

    // Per ogni cella, le celle raggiungibili da lì attraverso una porta
    private readonly Dictionary<Vector2Int, HashSet<Vector2Int>> connections =
        new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    // Coppie di celle vicine rimaste senza passaggio perché a un prefab mancava
    // la porta nella direzione richiesta: servono solo alla diagnosi finale.
    private readonly List<string> missingPassages = new List<string>();

    private void Awake()
    {
        if (randomSeed != 0) Random.InitState(randomSeed);

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogWarning("Assegna almeno un prefab in roomPrefabs.");
            return;
        }

        ValidateRoomPrefabs();

        List<Vector2Int> cells = GenerateLayout();

        Vector2Int startCell = cells[0];
        Vector2Int bossCell = FarthestCell(cells, startCell);
        Vector2Int? minibossCell = PickMinibossCell(cells, startCell, bossCell);

        // TEMPORANEO: serve a controllare in playtest quanto minibossDistanceBias
        // spinge il miniboss lontano dalla partenza.
        Debug.Log($"Dungeon: distanza boss {Distance(startCell, bossCell)}, " +
                  $"distanza miniboss {(minibossCell.HasValue ? Distance(startCell, minibossCell.Value).ToString() : "nessuna")}");

        InstantiateRooms(cells, startCell, bossCell, minibossCell);

        if (!rooms.ContainsKey(startCell) || !rooms.ContainsKey(bossCell)) return;

        ConnectDoors(cells);
        AttachSecretRoom(bossCell, minibossCell);
        RemoveUnconnectedDoors();
        PopulateRooms(startCell, bossCell, minibossCell);

        RoomManager roomManager = GetComponent<RoomManager>();
        roomManager.SetStartingRoom(rooms[startCell]);
        roomManager.SetFinalRoom(rooms[bossCell]);

        // In coda alla generazione: la minimappa deve vedere le stanze già
        // create. Il RoomManager entrerà nella stanza iniziale in Start(),
        // colorandola come corrente.
        if (minimap != null) minimap.Build(rooms, connections, startCell, bossCell, minibossCell);

        VerifyConnectivity(startCell);
    }

    // --- 0. validazione dei prefab -------------------------------------------

    // Una stanza deve avere tutte e quattro le porte, anche se ne userà solo
    // alcune: quelle che non servono vengono distrutte da RemoveUnconnectedDoors.
    // Se ne manca una, la cella non potrà collegarsi al vicino da quel lato e il
    // dungeon rischia di spezzarsi in componenti irraggiungibili.
    private void ValidateRoomPrefabs()
    {
        if (roomPrefabs != null)
        {
            foreach (GameObject prefab in roomPrefabs) ValidateRoomPrefab(prefab);
        }

        ValidateRoomPrefab(bossRoomPrefab);
        ValidateRoomPrefab(minibossRoomPrefab);
        ValidateRoomPrefab(secretRoomPrefab);
    }

    private static void ValidateRoomPrefab(GameObject prefab)
    {
        if (prefab == null) return; // non configurato: è una scelta legittima

        // true: nei prefab le porte possono essere disattivate
        DoorTrigger[] doors = prefab.GetComponentsInChildren<DoorTrigger>(true);

        int[] countPerDirection = new int[AllDirections.Length];
        foreach (DoorTrigger door in doors) countPerDirection[(int)door.Dir]++;

        List<string> problems = new List<string>();

        foreach (DoorTrigger.Direction direction in AllDirections)
        {
            int count = countPerDirection[(int)direction];

            if (count == 0) problems.Add($"{direction} mancante");
            else if (count > 1) problems.Add($"{direction} ripetuta {count} volte");
        }

        if (problems.Count == 0) return;

        // Il prefab come contesto: cliccando il log si seleziona nel Project
        Debug.LogError($"Prefab stanza '{prefab.name}': servono quattro DoorTrigger, uno per direzione, " +
                       $"ma ne ha {doors.Length}. Problemi: {string.Join(", ", problems)}", prefab);
    }

    // --- 1. grafo su griglia -------------------------------------------------

    // Espansione a coda dalla cella centrale: da ogni cella si provano le 4
    // adiacenti e se ne aggiunge una con probabilità 50%, ma solo se ha una
    // sola cella vicina già occupata (quella da cui si sta espandendo). Così
    // il dungeon resta ramificato invece di riempirsi a blocco.
    private List<Vector2Int> GenerateLayout()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        int center = gridSize / 2;
        Vector2Int start = new Vector2Int(center, center);

        cells.Add(start);
        occupied.Add(start);

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        int target = Mathf.Max(1, roomCount);

        // Con il 50% di probabilità la coda può svuotarsi prima di aver
        // raggiunto roomCount: in quel caso si riparte da tutte le celle già
        // piazzate. Il contatore dei giri evita un ciclo infinito quando la
        // griglia è troppo piccola per contenere roomCount stanze.
        int passes = 0;

        while (cells.Count < target && passes < 100)
        {
            if (frontier.Count == 0)
            {
                foreach (Vector2Int cell in cells) frontier.Enqueue(cell);
                passes++;
            }

            Vector2Int current = frontier.Dequeue();

            foreach (DoorTrigger.Direction direction in AllDirections)
            {
                if (cells.Count >= target) break;

                Vector2Int next = current + OffsetOf(direction);

                if (!InsideGrid(next)) continue;
                if (occupied.Contains(next)) continue;
                if (Random.value < 0.5f) continue;
                if (CountOccupiedNeighbours(occupied, next) > 1) continue;

                cells.Add(next);
                occupied.Add(next);
                frontier.Enqueue(next);
            }
        }

        return cells;
    }

    // --- 2. stanza boss e stanza miniboss ------------------------------------

    private static int Distance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
    }

    private static Vector2Int FarthestCell(List<Vector2Int> cells, Vector2Int from)
    {
        Vector2Int farthest = from;
        int maxDistance = -1;

        foreach (Vector2Int cell in cells)
        {
            int distance = Distance(from, cell);
            if (distance <= maxDistance) continue;

            maxDistance = distance;
            farthest = cell;
        }

        return farthest;
    }

    // Roulette wheel sul peso (distanza dalla partenza)^minibossDistanceBias:
    // ogni cella occupa una fetta proporzionale al peso e se ne estrae una.
    // Restituisce null se non ci sono candidate: in quel caso il dungeon esce
    // semplicemente senza miniboss.
    private Vector2Int? PickMinibossCell(List<Vector2Int> cells, Vector2Int startCell, Vector2Int bossCell)
    {
        if (minibossRoomPrefab == null) return null;

        List<Vector2Int> candidates = new List<Vector2Int>();
        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach (Vector2Int cell in cells)
        {
            if (cell == startCell || cell == bossCell) continue;

            int distance = Distance(startCell, cell);
            if (distance <= 1) continue; // troppo vicina: adiacente alla partenza

            float weight = Mathf.Pow(distance, minibossDistanceBias);
            if (weight <= 0f) continue;

            candidates.Add(cell);
            weights.Add(weight);
            totalWeight += weight;
        }

        if (candidates.Count == 0 || totalWeight <= 0f) return null;

        float pick = Random.value * totalWeight;

        for (int i = 0; i < candidates.Count; i++)
        {
            pick -= weights[i];
            if (pick <= 0f) return candidates[i];
        }

        // Solo se gli errori di arrotondamento fanno avanzare pick oltre la fine
        return candidates[candidates.Count - 1];
    }

    // --- 3. istanziamento ----------------------------------------------------

    private void InstantiateRooms(List<Vector2Int> cells, Vector2Int startCell, Vector2Int bossCell, Vector2Int? minibossCell)
    {
        foreach (Vector2Int cell in cells)
        {
            GameObject prefab = PickRoomPrefab(cell, startCell, bossCell, minibossCell);
            if (prefab == null) continue;

            rooms.Add(cell, CreateRoom(prefab));
        }
    }

    // Le stanze nascono disattivate: è il RoomManager ad accenderne una alla
    // volta. Il false finale mantiene la posizione locale del prefab, così le
    // arrivalPosition tarate nel prefab restano valide.
    private GameObject CreateRoom(GameObject prefab)
    {
        GameObject room = Instantiate(prefab, transform, false);
        room.SetActive(false);

        allRooms.Add(room);
        return room;
    }

    private GameObject PickRoomPrefab(Vector2Int cell, Vector2Int startCell, Vector2Int bossCell, Vector2Int? minibossCell)
    {
        if (cell == bossCell && bossRoomPrefab != null) return bossRoomPrefab;
        if (minibossCell.HasValue && cell == minibossCell.Value) return minibossRoomPrefab;
        if (cell == startCell) return roomPrefabs[0];

        return roomPrefabs[Random.Range(0, roomPrefabs.Length)];
    }

    // --- 4. collegamento delle porte -----------------------------------------

    private void ConnectDoors(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (!rooms.TryGetValue(cell, out GameObject room)) continue;

            foreach (DoorTrigger.Direction direction in PairDirections)
            {
                Vector2Int neighbourCell = cell + OffsetOf(direction);
                if (!rooms.TryGetValue(neighbourCell, out GameObject neighbour)) continue;

                DoorTrigger door = FindDoor(room, direction);
                DoorTrigger neighbourDoor = FindDoor(neighbour, Opposite(direction));

                // Se manca una delle due porte il passaggio non esiste: restano
                // entrambe scollegate e vengono rimosse più avanti. Le celle
                // restano vicine sulla griglia ma non comunicano.
                if (door == null || neighbourDoor == null)
                {
                    string missing = door == null
                        ? $"{cell} non ha la porta {direction}"
                        : $"{neighbourCell} non ha la porta {Opposite(direction)}";

                    missingPassages.Add($"{cell}-{neighbourCell}: {missing}");
                    continue;
                }

                // Ogni porta manda il player sull'arrivalPosition della porta
                // che si trova dall'altra parte del passaggio.
                door.Connect(neighbour, neighbourDoor.ArrivalPosition);
                neighbourDoor.Connect(room, door.ArrivalPosition);

                RegisterConnection(cell, neighbourCell);
            }
        }
    }

    // Due celle vicine sulla griglia non sono per forza collegate: il passaggio
    // esiste solo se entrambe le stanze avevano la porta giusta. La minimappa
    // ha bisogno dei collegamenti veri, non della semplice vicinanza.
    private void RegisterConnection(Vector2Int a, Vector2Int b)
    {
        AddConnection(a, b);
        AddConnection(b, a);
    }

    private void AddConnection(Vector2Int from, Vector2Int to)
    {
        if (!connections.TryGetValue(from, out HashSet<Vector2Int> linked))
        {
            linked = new HashSet<Vector2Int>();
            connections.Add(from, linked);
        }

        linked.Add(to);
    }

    private static DoorTrigger FindDoor(GameObject room, DoorTrigger.Direction direction)
    {
        // true: le stanze sono disattivate, senza questo non troverebbe nulla
        foreach (DoorTrigger door in room.GetComponentsInChildren<DoorTrigger>(true))
        {
            if (door.Dir == direction) return door;
        }

        return null;
    }

    // --- 4b. stanza segreta --------------------------------------------------

    // Sta fra ConnectDoors e RemoveUnconnectedDoors non per comodità: il
    // collegamento legge l'arrivalPosition della porta scollegata dell'ospite,
    // che al passo successivo viene distrutta insieme al suo GameObject.
    private void AttachSecretRoom(Vector2Int bossCell, Vector2Int? minibossCell)
    {
        if (secretRoomPrefab == null) return;

        GameObject secretRoom = CreateRoom(secretRoomPrefab);

        // Non occupa una cella della griglia: è appesa fuori dal grafo, quindi
        // non finisce in rooms e nessun vicino può collegarcisi.
        secretRoom.transform.localPosition = Vector3.zero;

        if (TryAttachSecretRoom(secretRoom, bossCell, minibossCell)) return;

        // Nessun ospite disponibile: il dungeon esce senza stanza segreta
        allRooms.Remove(secretRoom);
        DestroyImmediate(secretRoom);
    }

    private bool TryAttachSecretRoom(GameObject secretRoom, Vector2Int bossCell, Vector2Int? minibossCell)
    {
        List<GameObject> hosts = new List<GameObject>();

        foreach (KeyValuePair<Vector2Int, GameObject> entry in rooms)
        {
            if (entry.Key == bossCell) continue;
            if (minibossCell.HasValue && entry.Key == minibossCell.Value) continue;
            if (FreeDirections(entry.Value).Count == 0) continue;

            hosts.Add(entry.Value);
        }

        if (hosts.Count == 0) return false;

        GameObject hostRoom = hosts[Random.Range(0, hosts.Count)];

        List<DoorTrigger.Direction> freeDirections = FreeDirections(hostRoom);
        DoorTrigger.Direction direction = freeDirections[Random.Range(0, freeDirections.Count)];

        DoorTrigger hostDoor = FindDoor(hostRoom, direction);
        DoorTrigger secretDoor = FindDoor(secretRoom, Opposite(direction));
        if (hostDoor == null || secretDoor == null) return false;

        SecretWall secretWall = CreateSecretWall(hostRoom, direction);
        if (secretWall == null) return false;

        // Va letta adesso: fra un passo la porta dell'ospite non esisterà più
        Vector2 hostArrival = hostDoor.ArrivalPosition;

        // La porta della stanza segreta riporta indietro ed è l'unica delle sue
        // quattro a restare collegata: le altre tre spariscono con le altre
        // porte senza destinazione.
        secretDoor.Connect(hostRoom, hostArrival);
        secretWall.Connect(secretRoom, secretDoor.ArrivalPosition);

        return true;
    }

    // Direzioni in cui l'ospite ha una porta rimasta senza destinazione: da lì
    // non si passa, quindi il muro corrispondente può diventare quello segreto.
    private static List<DoorTrigger.Direction> FreeDirections(GameObject room)
    {
        List<DoorTrigger.Direction> directions = new List<DoorTrigger.Direction>();

        foreach (DoorTrigger.Direction direction in AllDirections)
        {
            DoorTrigger door = FindDoor(room, direction);
            if (door != null && !door.IsConnected) directions.Add(direction);
        }

        return directions;
    }

    private SecretWall CreateSecretWall(GameObject room, DoorTrigger.Direction direction)
    {
        Transform wall = FindOutermostWall(room, direction);
        if (wall == null) return null;

        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = secretWallTint;

        // Se il prefab avesse già dichiarato un muro segreto lì, si riusa quello
        SecretWall secretWall = wall.GetComponent<SecretWall>();
        if (secretWall == null) secretWall = wall.gameObject.AddComponent<SecretWall>();

        return secretWall;
    }

    // Il muro da sfondare è il più esterno nella direzione scelta. Si riconosce
    // dalla posizione e non dal nome, così i prefab restano liberi di chiamare
    // i propri oggetti come vogliono.
    private static Transform FindOutermostWall(GameObject room, DoorTrigger.Direction direction)
    {
        Vector2 axis = OffsetOf(direction);

        Transform outermost = null;
        float bestDistance = float.NegativeInfinity;

        foreach (Transform child in room.GetComponentsInChildren<Transform>(true))
        {
            if (!child.CompareTag(WallTag)) continue;

            // Rispetto alla radice della stanza, così vale anche per i muri
            // annidati dentro a un contenitore.
            Vector3 local = room.transform.InverseTransformPoint(child.position);
            float distance = local.x * axis.x + local.y * axis.y;

            if (distance <= bestDistance) continue;

            bestDistance = distance;
            outermost = child;
        }

        return outermost;
    }

    // --- 5. porte che non portano da nessuna parte ---------------------------

    // Va chiamato quando tutti i collegamenti sono già stati stabiliti: una
    // porta senza targetRoom prima di allora potrebbe essere solo una porta
    // che deve ancora essere collegata dalla stanza vicina.
    //
    // Si distrugge l'intero GameObject e non il solo DoorTrigger: disattivarlo
    // non basterebbe, perché RoomManager.SetDoorsActive cerca per tag "Door" e
    // lo riaccenderebbe alla liberazione della stanza.
    private void RemoveUnconnectedDoors()
    {
        foreach (GameObject room in allRooms)
        {
            foreach (DoorTrigger door in room.GetComponentsInChildren<DoorTrigger>(true))
            {
                // Già distrutta insieme a una porta che la conteneva
                if (door == null) continue;

                if (door.IsConnected) continue;

                DestroyImmediate(door.gameObject);
            }
        }
    }

    // --- 6. nemici -----------------------------------------------------------

    // Né la stanza iniziale né quella del boss vengono popolate: la prima resta
    // vuota, il boss arriva già dentro al suo prefab. Quella del miniboss ha un
    // trattamento a parte, senza segnaposto.
    private void PopulateRooms(Vector2Int startCell, Vector2Int bossCell, Vector2Int? minibossCell)
    {
        foreach (KeyValuePair<Vector2Int, GameObject> entry in rooms)
        {
            if (entry.Key == startCell || entry.Key == bossCell) continue;

            if (minibossCell.HasValue && entry.Key == minibossCell.Value)
            {
                SpawnMiniboss(entry.Value);
                continue;
            }

            if (enemyPrefabs == null || enemyPrefabs.Length == 0) continue;

            PopulateRoom(entry.Value);
        }
    }

    // Niente RegisterEnemySpawn qui e in PopulateRoom: RoomManager conta i
    // nemici della stanza quando ci si entra, registrarli ora li conterebbe
    // due volte e le porte resterebbero bloccate per sempre.
    private void SpawnMiniboss(GameObject room)
    {
        if (minibossPrefab == null) return;

        Instantiate(minibossPrefab, room.transform.position, Quaternion.identity, room.transform);
    }

    private void PopulateRoom(GameObject room)
    {
        List<Transform> spawnPoints = new List<Transform>();
        CollectSpawnPoints(room.transform, spawnPoints);

        if (spawnPoints.Count == 0) return;

        int min = Mathf.Max(0, minEnemiesPerRoom);
        int max = Mathf.Max(min, maxEnemiesPerRoom);
        int amount = Mathf.Min(Random.Range(min, max + 1), spawnPoints.Count);

        // Mescolati una volta sola e poi presi in ordine: ogni segnaposto viene
        // usato al massimo da un nemico, così non si sovrappongono.
        Shuffle(spawnPoints);

        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (prefab == null) continue;

            Instantiate(prefab, spawnPoints[i].position, Quaternion.identity, room.transform);
        }
    }

    // Fisher-Yates
    private static void Shuffle(List<Transform> items)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            Transform swap = items[i];
            items[i] = items[j];
            items[j] = swap;
        }
    }

    private static void CollectSpawnPoints(Transform parent, List<Transform> spawnPoints)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith(SpawnPointPrefix)) spawnPoints.Add(child);

            CollectSpawnPoints(child, spawnPoints);
        }
    }

    // --- controllo finale ----------------------------------------------------

    // La visita in ampiezza gira sui collegamenti VERI (connections), non sulla
    // vicinanza fra celle: il grafo di celle nasce già connesso per costruzione
    // in GenerateLayout, quindi una visita sulla griglia passerebbe sempre e non
    // troverebbe mai il problema. Quello che può spezzarsi è il grafo delle
    // porte, quando a un prefab manca la porta nella direzione richiesta.
    private void VerifyConnectivity(Vector2Int startCell)
    {
        HashSet<Vector2Int> reached = new HashSet<Vector2Int> { startCell };

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(startCell);

        while (frontier.Count > 0)
        {
            Vector2Int cell = frontier.Dequeue();

            if (!connections.TryGetValue(cell, out HashSet<Vector2Int> linked)) continue;

            foreach (Vector2Int next in linked)
            {
                if (!reached.Add(next)) continue;

                frontier.Enqueue(next);
            }
        }

        List<Vector2Int> unreachable = new List<Vector2Int>();

        foreach (Vector2Int cell in rooms.Keys)
        {
            if (!reached.Contains(cell)) unreachable.Add(cell);
        }

        if (unreachable.Count == 0) return;

        Debug.LogError($"Dungeon spezzato: {unreachable.Count} stanze irraggiungibili da {startCell}: " +
                       $"{string.Join(", ", unreachable)}");

        if (missingPassages.Count > 0)
        {
            Debug.LogError($"Porte mancanti nei prefab: {string.Join(" | ", missingPassages)}");
        }
    }

    // --- utilità sulla griglia -----------------------------------------------

    private bool InsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridSize && cell.y >= 0 && cell.y < gridSize;
    }

    private static int CountOccupiedNeighbours(HashSet<Vector2Int> occupied, Vector2Int cell)
    {
        int count = 0;

        foreach (DoorTrigger.Direction direction in AllDirections)
        {
            if (occupied.Contains(cell + OffsetOf(direction))) count++;
        }

        return count;
    }

    private static Vector2Int OffsetOf(DoorTrigger.Direction direction)
    {
        switch (direction)
        {
            case DoorTrigger.Direction.North: return new Vector2Int(0, 1);
            case DoorTrigger.Direction.South: return new Vector2Int(0, -1);
            case DoorTrigger.Direction.East: return new Vector2Int(1, 0);
            default: return new Vector2Int(-1, 0);
        }
    }

    private static DoorTrigger.Direction Opposite(DoorTrigger.Direction direction)
    {
        switch (direction)
        {
            case DoorTrigger.Direction.North: return DoorTrigger.Direction.South;
            case DoorTrigger.Direction.South: return DoorTrigger.Direction.North;
            case DoorTrigger.Direction.East: return DoorTrigger.Direction.West;
            default: return DoorTrigger.Direction.East;
        }
    }
}
