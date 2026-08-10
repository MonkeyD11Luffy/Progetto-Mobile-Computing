using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Minimappa in stile Isaac: una cella per stanza, disegnata sulla griglia del
// dungeon. Va messa sul Canvas. Le celle vengono create dal DungeonGenerator
// alla fine della generazione, i colori aggiornati dal RoomManager a ogni
// cambio stanza.
public class MinimapController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private RectTransform minimapPanel;
    [SerializeField] private GameObject cellPrefab;

    [Header("Disposizione")]
    [SerializeField] private float cellSize = 16f;
    [SerializeField] private float cellSpacing = 3f;

    [Header("Comandi")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("Colori")]
    [SerializeField] private Color visitedColor = new Color(0.8f, 0.8f, 0.85f, 1f);
    [SerializeField] private Color currentColor = Color.white;
    // Stanze intraviste ma non ancora visitate: grigio smorzato
    [SerializeField] private Color adjacentColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
    [SerializeField] private Color bossColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color minibossColor = new Color(0.9f, 0.5f, 0.15f, 1f);

    private readonly Dictionary<Vector2Int, Image> cells = new Dictionary<Vector2Int, Image>();
    private readonly Dictionary<GameObject, Vector2Int> cellByRoom = new Dictionary<GameObject, Vector2Int>();
    private readonly HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();

    // Celle raggiungibili da ciascuna cella attraverso una porta: due stanze
    // vicine sulla griglia possono non essere collegate.
    private Dictionary<Vector2Int, HashSet<Vector2Int>> connections;

    private Vector2Int bossCell;
    private Vector2Int? minibossCell;

    private Vector2Int currentCell;
    private bool hasCurrentCell;

    private void Update()
    {
        // Come in PlayerController: Time.timeScale = 0 ferma la fisica ma non
        // Update(), quindi la minimappa risponderebbe dalle schermate di fine
        // partita.
        if (Time.timeScale == 0f) return;

        if (!Input.GetKeyDown(toggleKey) || minimapPanel == null) return;

        minimapPanel.gameObject.SetActive(!minimapPanel.gameObject.activeSelf);
    }

    // La stanza segreta non ha una cella nella griglia, quindi non arriva qui
    // e non compare sulla minimappa.
    public void Build(Dictionary<Vector2Int, GameObject> rooms, Dictionary<Vector2Int, HashSet<Vector2Int>> roomConnections, Vector2Int startCell, Vector2Int bossRoomCell, Vector2Int? minibossRoomCell)
    {
        if (minimapPanel == null || cellPrefab == null || rooms == null) return;

        connections = roomConnections;
        bossCell = bossRoomCell;
        minibossCell = minibossRoomCell;

        cells.Clear();
        cellByRoom.Clear();
        visitedCells.Clear();

        Vector2 gridCenter = GridCenter(rooms.Keys);

        foreach (KeyValuePair<Vector2Int, GameObject> entry in rooms)
        {
            Image cell = CreateCell(entry.Key, gridCenter);
            if (cell == null) continue;

            cells.Add(entry.Key, cell);
            cellByRoom[entry.Value] = entry.Key;
        }

        // La partenza è già visitata: il RoomManager ci entra subito dopo, ma
        // così la mappa è coerente anche se qualcosa va storto.
        currentCell = startCell;
        hasCurrentCell = cells.ContainsKey(startCell);
        if (hasCurrentCell) visitedCells.Add(startCell);

        RefreshColors();
    }

    public void SetCurrentRoom(GameObject room)
    {
        if (room == null) return;

        // Stanza fuori dalla griglia (la segreta): la mappa resta com'era
        if (!cellByRoom.TryGetValue(room, out Vector2Int cell)) return;

        currentCell = cell;
        hasCurrentCell = true;
        visitedCells.Add(cell);

        RefreshColors();
    }

    private Image CreateCell(Vector2Int cell, Vector2 gridCenter)
    {
        GameObject instance = Instantiate(cellPrefab, minimapPanel);

        Image image = instance.GetComponent<Image>();
        if (image == null)
        {
            Destroy(instance);
            return null;
        }

        float step = cellSize + cellSpacing;

        RectTransform rect = (RectTransform)instance.transform;
        rect.sizeDelta = new Vector2(cellSize, cellSize);
        rect.anchoredPosition = new Vector2(
            (cell.x - gridCenter.x) * step,
            (cell.y - gridCenter.y) * step);

        return image;
    }

    // Punto medio fra le celle estreme: la mappa risulta centrata nel pannello
    // anche quando il dungeon cresce tutto da una parte.
    private static Vector2 GridCenter(IEnumerable<Vector2Int> occupiedCells)
    {
        bool any = false;
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        foreach (Vector2Int cell in occupiedCells)
        {
            if (!any)
            {
                min = cell;
                max = cell;
                any = true;
                continue;
            }

            min = Vector2Int.Min(min, cell);
            max = Vector2Int.Max(max, cell);
        }

        return new Vector2(min.x + max.x, min.y + max.y) * 0.5f;
    }

    private void RefreshColors()
    {
        foreach (KeyValuePair<Vector2Int, Image> entry in cells)
        {
            Vector2Int cell = entry.Key;
            Image image = entry.Value;

            if (hasCurrentCell && cell == currentCell)
            {
                Paint(image, currentColor);
                continue;
            }

            if (visitedCells.Contains(cell))
            {
                Paint(image, ColorOfVisited(cell));
                continue;
            }

            if (IsConnectedToVisited(cell))
            {
                Paint(image, adjacentColor);
                continue;
            }

            // Stanza ancora ignota: la cella non si vede proprio
            image.enabled = false;
        }
    }

    private Color ColorOfVisited(Vector2Int cell)
    {
        if (cell == bossCell) return bossColor;
        if (minibossCell.HasValue && cell == minibossCell.Value) return minibossColor;

        return visitedColor;
    }

    // Si intravede una stanza solo se una porta la collega a una già visitata:
    // due celle affiancate sulla griglia ma senza passaggio restano ignote.
    private bool IsConnectedToVisited(Vector2Int cell)
    {
        if (connections == null) return false;

        if (!connections.TryGetValue(cell, out HashSet<Vector2Int> linked)) return false;

        foreach (Vector2Int neighbour in linked)
        {
            if (visitedCells.Contains(neighbour)) return true;
        }

        return false;
    }

    private static void Paint(Image image, Color color)
    {
        image.enabled = true;
        image.color = color;
    }
}
