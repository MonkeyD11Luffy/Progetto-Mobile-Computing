using System.Collections.Generic;
using UnityEngine;

// A* sulla griglia di una stanza (RoomNavGrid), con movimento a 8 direzioni.
// Non è un MonoBehaviour: non va messa su nessun GameObject, come VectorUtils.
public static class Pathfinder
{
    // Tetto all'esplorazione: in casi patologici (destinazione murata, stanza
    // enorme) è preferibile nessun percorso a un frame bloccato.
    private const int MaxExploredNodes = 500;

    private const float DiagonalCost = 1.414f;

    private static readonly Vector2Int[] OrthogonalOffsets =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
    };

    private static readonly Vector2Int[] DiagonalOffsets =
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    // Stato di una singola ricerca, raccolto per non passare sei dizionari
    // a ogni metodo di appoggio.
    private class Search
    {
        public readonly Dictionary<Vector2Int, Vector2Int> CameFrom = new Dictionary<Vector2Int, Vector2Int>();
        public readonly Dictionary<Vector2Int, float> GScore = new Dictionary<Vector2Int, float>();
        public readonly Dictionary<Vector2Int, float> FScore = new Dictionary<Vector2Int, float>();
        public readonly List<Vector2Int> Open = new List<Vector2Int>();
        public readonly HashSet<Vector2Int> Closed = new HashSet<Vector2Int>();
    }

    // Centri delle celle dal primo passo alla destinazione, esclusa la cella di
    // partenza. Lista vuota se si è già arrivati, null se non esiste percorso.
    public static List<Vector2> FindPath(RoomNavGrid grid, Vector2 from, Vector2 to)
    {
        if (grid == null) return null;

        Vector2Int start = grid.WorldToCell(from);
        Vector2Int goal = grid.WorldToCell(to);

        // Partenza dentro un muro: capita a un nemico schiacciato contro un
        // ostacolo, il cui centro cade in una cella occupata. Dentro a un muro
        // spesso anche tutte le celle vicine sono occupate, quindi l'A* non
        // avrebbe da dove espandersi: si riparte dalla cella libera più vicina.
        if (!grid.IsWalkable(start) && !TryFindNearestFreeCell(grid, start, out start)) return null;

        // Destinazione dentro un muro o fuori dalla stanza: si punta alla cella
        // libera più vicina, così l'inseguimento non si interrompe del tutto.
        if (!grid.IsWalkable(goal) && !TryFindNearestFreeCell(grid, goal, out goal)) return null;

        if (start == goal) return new List<Vector2>();

        Search search = new Search();
        search.GScore[start] = 0f;
        search.FScore[start] = Heuristic(start, goal);
        search.Open.Add(start);

        int explored = 0;

        while (search.Open.Count > 0 && explored < MaxExploredNodes)
        {
            Vector2Int current = PopLowest(search);

            if (current == goal) return BuildPath(grid, search, start, goal);

            search.Closed.Add(current);
            explored++;

            foreach (Vector2Int offset in OrthogonalOffsets)
            {
                TryImprove(grid, search, current, current + offset, 1f, goal);
            }

            foreach (Vector2Int offset in DiagonalOffsets)
            {
                // Niente tagli agli spigoli: la diagonale vale solo se sono
                // libere anche le due celle ortogonali che la fiancheggiano.
                if (!CanMoveDiagonally(grid, current, offset)) continue;

                TryImprove(grid, search, current, current + offset, DiagonalCost, goal);
            }
        }

        return null;
    }

    private static void TryImprove(RoomNavGrid grid, Search search, Vector2Int current, Vector2Int neighbour, float stepCost, Vector2Int goal)
    {
        if (search.Closed.Contains(neighbour)) return;
        if (!grid.IsWalkable(neighbour)) return;

        float tentative = search.GScore[current] + stepCost;

        if (search.GScore.TryGetValue(neighbour, out float known) && tentative >= known) return;

        search.CameFrom[neighbour] = current;
        search.GScore[neighbour] = tentative;
        search.FScore[neighbour] = tentative + Heuristic(neighbour, goal);

        if (!search.Open.Contains(neighbour)) search.Open.Add(neighbour);
    }

    private static bool CanMoveDiagonally(RoomNavGrid grid, Vector2Int from, Vector2Int offset)
    {
        return grid.IsWalkable(from + new Vector2Int(offset.x, 0))
            && grid.IsWalkable(from + new Vector2Int(0, offset.y));
    }

    // Distanza ottagonale: si procede in diagonale finché conviene, il resto
    // in orizzontale o verticale.
    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return (dx + dy) + (DiagonalCost - 2f) * Mathf.Min(dx, dy);
    }

    // Con al massimo MaxExploredNodes nodi aperti la scansione lineare costa
    // meno di quanto costerebbe mantenere una coda di priorità.
    private static Vector2Int PopLowest(Search search)
    {
        int bestIndex = 0;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < search.Open.Count; i++)
        {
            float score = search.FScore.TryGetValue(search.Open[i], out float f)
                ? f
                : float.PositiveInfinity;

            if (score >= bestScore) continue;

            bestScore = score;
            bestIndex = i;
        }

        Vector2Int best = search.Open[bestIndex];
        search.Open.RemoveAt(bestIndex);

        return best;
    }

    private static List<Vector2> BuildPath(RoomNavGrid grid, Search search, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        Vector2Int cell = goal;

        while (cell != start)
        {
            cells.Add(cell);

            if (!search.CameFrom.TryGetValue(cell, out cell)) return null; // catena interrotta
        }

        cells.Reverse();

        List<Vector2> path = new List<Vector2>(cells.Count);
        foreach (Vector2Int step in cells) path.Add(grid.CellToWorld(step));

        return path;
    }

    // Visita in ampiezza attorno alla cella occupata: la prima libera che si
    // incontra è anche la più vicina. Si parte da una cella dentro la griglia,
    // altrimenti una destinazione lontana farebbe vagare la ricerca nel vuoto.
    private static bool TryFindNearestFreeCell(RoomNavGrid grid, Vector2Int from, out Vector2Int nearest)
    {
        Vector2Int clamped = new Vector2Int(
            Mathf.Clamp(from.x, 0, grid.Width - 1),
            Mathf.Clamp(from.y, 0, grid.Height - 1));

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();

        frontier.Enqueue(clamped);
        seen.Add(clamped);

        int explored = 0;

        while (frontier.Count > 0 && explored < MaxExploredNodes)
        {
            Vector2Int cell = frontier.Dequeue();
            explored++;

            if (grid.IsWalkable(cell))
            {
                nearest = cell;
                return true;
            }

            foreach (Vector2Int offset in OrthogonalOffsets)
            {
                Vector2Int next = cell + offset;

                if (next.x < 0 || next.x >= grid.Width) continue;
                if (next.y < 0 || next.y >= grid.Height) continue;
                if (!seen.Add(next)) continue;

                frontier.Enqueue(next);
            }
        }

        nearest = from;
        return false;
    }
}
