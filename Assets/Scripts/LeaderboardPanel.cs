using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Classifica del menu principale. Non tiene stato: ricostruisce le righe da
// zero a ogni Refresh, perché il database degli account cambia fra una partita
// e l'altra e il pannello resta in scena, solo spento.
public class LeaderboardPanel : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

    [Header("Righe")]
    [SerializeField] private int maxRows = 10;
    [SerializeField] private string emptyMessage = "Nessun punteggio salvato";

    // OnEnable e non Start: il pannello si accende e si spegne più volte nella
    // stessa scena, e ogni volta deve mostrare i punteggi aggiornati.
    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (rowContainer == null || rowPrefab == null) return;

        ClearRows();

        List<LeaderboardEntry> leaderboard = AccountManager.Instance != null
            ? AccountManager.Instance.GetLeaderboard()
            : null;

        // Senza AccountManager (scena aperta da sola) o senza account salvati
        // la classifica non è un errore: è solo vuota.
        if (leaderboard == null || leaderboard.Count == 0)
        {
            FillRow(CreateRow(), emptyMessage, string.Empty, string.Empty);
            return;
        }

        int count = Mathf.Min(leaderboard.Count, maxRows);

        for (int i = 0; i < count; i++)
        {
            LeaderboardEntry entry = leaderboard[i];
            FillRow(CreateRow(), $"{i + 1}.", entry.displayName, entry.bestScore.ToString());
        }
    }

    private void ClearRows()
    {
        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private GameObject CreateRow()
    {
        return Instantiate(rowPrefab, rowContainer);
    }

    // Il prefab può avere tre testi (posizione, nome, punteggio) oppure uno
    // solo: nel secondo caso la riga viene composta in un'unica stringa. Le
    // etichette si cercano per componente e non per nome, così rinominare un
    // figlio nel prefab non rompe la classifica.
    private void FillRow(GameObject row, string position, string playerName, string score)
    {
        TextMeshProUGUI[] labels = row.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (labels.Length == 0) return;

        if (labels.Length >= 3)
        {
            labels[0].text = position;
            labels[1].text = playerName;
            labels[2].text = score;
            return;
        }

        labels[0].text = string.IsNullOrEmpty(playerName)
            ? position
            : $"{position} {playerName} — {score}";
    }
}
