using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f; // ferma il gioco (nemici, movimento, ecc.)
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // riattiva il tempo prima di ricaricare
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
