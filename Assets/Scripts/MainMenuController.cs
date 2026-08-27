using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject leaderboardPanel;

    public void PlayGame()
    {
        // Il tempo potrebbe essere fermo se si arriva qui da una schermata di fine partita
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Il pannello si aggiorna da solo quando si accende: LeaderboardPanel
    // ricostruisce le righe in OnEnable.
    public void OpenLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();

        // Nell'editor Application.Quit() non fa nulla: questo serve per testarlo
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
