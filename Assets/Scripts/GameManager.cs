using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Punteggio")]
    [SerializeField] private TMPro.TextMeshProUGUI victoryScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverScoreText;

    private bool isPaused;

    // A partita finita la pausa va disattivata: ESC rimetterebbe timeScale a 1
    // sopra la schermata di game over o di vittoria, facendo ripartire il gioco
    // sotto il pannello.
    private bool isGameOver;

    // Le legge chi altera Time.timeScale per conto suo (PlayerAbilities con
    // Rallenta Tempo): quando il gioco è fermo il timeScale appartiene a questo
    // script, e nessun altro deve rimetterlo a 1.
    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        SetPaused(!isPaused);
    }

    // Sempre una ripresa, mai un toggle: è il bottone "Riprendi" del pannello
    public void ResumeGame()
    {
        if (isGameOver) return;

        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pausePanel != null) pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        HidePausePanel();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        FinalizeRun(false);

        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        isGameOver = true;
        HidePausePanel();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        FinalizeRun(true);

        Time.timeScale = 0f;
    }

    // Chiude la partita: calcola il punteggio, lo mostra sul pannello giusto e
    // lo consegna all'account. Ogni pezzo è facoltativo — senza RunStats o
    // senza AccountManager la partita finisce comunque, solo senza punteggio.
    private void FinalizeRun(bool victory)
    {
        if (RunStats.Instance == null) return;

        int healthLeft = 0;
        int credits = 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                // Un colpo più forte della vita rimasta la porta sotto zero:
                // senza il clamp la morte sottrarrebbe punti al game over
                healthLeft = Mathf.Max(0, controller.CurrentHealth);
                credits = controller.Credits;
            }
        }

        int score = RunStats.Instance.ComputeScore(victory, credits, healthLeft);

        TMPro.TextMeshProUGUI scoreText = victory ? victoryScoreText : gameOverScoreText;
        if (scoreText != null) scoreText.text = $"Punteggio: {score}";

        AccountManager.Instance?.SaveScore(score);
    }

    // Si può morire da gioco in pausa? No, ma si può vincere: RoomManager
    // controlla la vittoria all'ingresso in stanza. Meglio non lasciare i due
    // pannelli sovrapposti.
    private void HidePausePanel()
    {
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
