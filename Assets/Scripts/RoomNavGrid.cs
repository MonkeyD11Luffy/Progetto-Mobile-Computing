using UnityEngine;

// Griglia di celle percorribili di una stanza, ricavata dai muri presenti in
// scena. Va messa sulla radice della stanza, accanto al RoomBounds che dichiara
// l'area da coprire.
//
// Si costruisce una volta sola: i muri non si muovono, quindi rifarla a ogni
// frame sarebbe solo lavoro sprecato.
[RequireComponent(typeof(RoomBounds))]
public class RoomNavGrid : MonoBehaviour
{
    [SerializeField] private float cellSize = 0.5f;

    // Ingombro del nemico più grande che deve poter percorrere la stanza. La
    // cella è libera solo se resta libero anche questo margine attorno: così i
    // waypoint stanno lontani dagli spigoli e il corpo non ci striscia contro.
    [SerializeField] private float agentRadius = 0.7f;

    private const string WallTag = "Wall";

    private bool[,] walkable;
    private Vector2 origin; // angolo in basso a sinistra dell'area coperta

    public int Width { get; private set; }
    public int Height { get; private set; }

    private void Start()
    {
        Build();
    }

    public void Build()
    {
        if (walkable != null) return; // già costruita

        if (cellSize <= 0f)
        {
            Debug.LogError("cellSize deve essere maggiore di zero.", this);
            return;
        }

        RoomBounds bounds = GetComponent<RoomBounds>();

        Vector2 center = transform.position;
        Vector2 size = new Vector2(bounds.HalfWidth * 2f, bounds.HalfHeight * 2f);

        origin = center - size * 0.5f;

        // Arrotondato per eccesso: meglio una fascia di celle in più ai bordi
        // che un pezzo di stanza scoperto.
        Width = Mathf.Max(1, Mathf.CeilToInt(size.x / cellSize));
        Height = Mathf.Max(1, Mathf.CeilToInt(size.y / cellSize));

        walkable = new bool[Width, Height];

        // La cella allargata del margine d'agente su ogni lato
        Vector2 probeSize = Vector2.one * (cellSize + agentRadius * 2f);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                walkable[x, y] = IsCellFree(CellToWorld(new Vector2Int(x, y)), probeSize);
            }
        }
    }

    public bool IsWalkable(Vector2Int cell)
    {
        if (walkable == null) return false;
        if (!IsInside(cell)) return false;

        return walkable[cell.x, cell.y];
    }

    public Vector2Int WorldToCell(Vector2 worldPosition)
    {
        Vector2 local = worldPosition - origin;

        // Può cadere fuori dalla griglia: se ne accorge IsWalkable, che per le
        // celle fuori area restituisce false.
        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellSize));
    }

    // Centro della cella, non il suo angolo: è il punto verso cui muoversi
    public Vector2 CellToWorld(Vector2Int cell)
    {
        return origin + new Vector2(
            (cell.x + 0.5f) * cellSize,
            (cell.y + 0.5f) * cellSize);
    }

    private bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
    }

    private static bool IsCellFree(Vector2 cellCenter, Vector2 probeSize)
    {
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(cellCenter, probeSize, 0f))
        {
            if (hit.CompareTag(WallTag)) return false;
        }

        return true;
    }
}
