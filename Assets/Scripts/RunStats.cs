using System.Collections.Generic;
using UnityEngine;

// Statistiche della partita in corso: nemici uccisi, stanze viste e durata.
// Vive sullo stesso GameObject del RoomManager, che è quello che le alimenta.
public class RunStats : MonoBehaviour
{
    public static RunStats Instance;

    [Header("Punteggio")]
    [SerializeField] private int pointsPerKill = 10;
    [SerializeField] private int pointsPerRoom = 50;
    [SerializeField] private int pointsPerCredit = 5;
    [SerializeField] private int pointsPerHealth = 100;
    [SerializeField] private int victoryBonus = 1000;
    // Quanto costa ogni secondo di partita: premia chi finisce in fretta
    [SerializeField] private int penaltyPerSecond = 1;

    // Le stanze già viste: un HashSet perché il player torna sui suoi passi di
    // continuo e una stanza rivisitata non deve valere due volte.
    private readonly HashSet<GameObject> visitedRooms = new HashSet<GameObject>();

    private int enemiesKilled;
    private float startTime;

    public int EnemiesKilled => enemiesKilled;
    public int RoomsVisited => visitedRooms.Count;

    // unscaled: a fine partita GameManager mette Time.timeScale a 0, e il tempo
    // scalato si fermerebbe insieme al gioco proprio mentre si legge la durata.
    public float RunSeconds => Time.unscaledTime - startTime;

    private void Awake()
    {
        Instance = this;
        startTime = Time.unscaledTime;
    }

    public void RegisterKill()
    {
        enemiesKilled++;
    }

    public void RegisterRoom(GameObject room)
    {
        if (room == null) return;

        visitedRooms.Add(room);
    }

    // Il punteggio non scende mai sotto zero: una partita lunghissima
    // azzererebbe il bottino invece di trasformarlo in un numero negativo.
    public int ComputeScore(bool victory, int credits, int healthLeft)
    {
        int score = enemiesKilled * pointsPerKill
                  + RoomsVisited * pointsPerRoom
                  + credits * pointsPerCredit
                  + healthLeft * pointsPerHealth;

        if (victory) score += victoryBonus;

        score -= Mathf.RoundToInt(RunSeconds) * penaltyPerSecond;

        return Mathf.Max(0, score);
    }
}
