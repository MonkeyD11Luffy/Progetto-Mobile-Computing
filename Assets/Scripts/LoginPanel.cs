using TMPro;
using UnityEngine;

// Pannello di accesso del menu principale. Sta sul GameObject del pannello
// stesso: a login riuscito lo spegne e accende il gruppo dei bottoni.
public class LoginPanel : MonoBehaviour
{
    [Header("Campi")]
    [SerializeField] private TMP_InputField emailInput;
    // Serve solo alla registrazione: l'accesso resta per email
    [SerializeField] private TMP_InputField displayNameInput;
    // Va messo con Content Type "Password" nell'Inspector: qui non si legge
    // mai il testo se non per passarlo ad AccountManager.
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Messaggi")]
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("Modalità")]
    // L'oggetto che contiene il campo del nome (riga, etichetta e input): si
    // accende e si spegne tutto insieme, mentre displayNameInput qui sopra
    // serve solo a leggerne il testo.
    [SerializeField] private GameObject displayNameRow;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI primaryButtonText;
    [SerializeField] private TextMeshProUGUI switchButtonText;

    [Header("Dopo l'accesso")]
    [SerializeField] private GameObject menuButtons;

    private const string LoginTitle = "ACCEDI";
    private const string LoginPrimary = "ACCEDI";
    private const string LoginSwitch = "NON HAI UN ACCOUNT? REGISTRATI";

    private const string RegisterTitle = "REGISTRATI";
    private const string RegisterPrimary = "REGISTRATI";
    private const string RegisterSwitch = "HAI GIÀ UN ACCOUNT? ACCEDI";

    // false = accesso, true = registrazione. Un bool e non un enum: le modalità
    // sono due e il bottone di scambio non fa altro che invertirlo.
    private bool registerMode;

    // Sessione già aperta (login ricordato dal file account): il pannello non
    // ha niente da chiedere e lascia subito il posto al menu.
    private void OnEnable()
    {
        // Il pannello si riapre sempre sull'accesso, anche se l'ultima volta era
        // rimasto sulla registrazione: è il caso di gran lunga più frequente.
        registerMode = false;
        ApplyMode();

        SkipIfLoggedIn();
    }

    // Anche in Start: all'avvio della scena l'Awake di AccountManager e questo
    // OnEnable non hanno un ordine garantito, mentre Start viene dopo tutti gli
    // Awake. Se OnEnable ha già spento il pannello, Start non parte nemmeno.
    private void Start()
    {
        SkipIfLoggedIn();
    }

    private void SkipIfLoggedIn()
    {
        if (AccountManager.Instance != null && AccountManager.Instance.IsLoggedIn) EnterMenu();
    }

    // Sul bottone di scambio.
    public void ToggleMode()
    {
        registerMode = !registerMode;
        ApplyMode();
    }

    // Sul bottone principale: quale delle due azioni sia dipende dalla modalità,
    // così il pannello ha un solo bottone di conferma invece di due.
    public void OnPrimaryPressed()
    {
        if (registerMode) OnRegisterPressed();
        else OnLoginPressed();
    }

    // Tutto ciò che distingue le due modalità sta qui: i tre testi, il campo del
    // nome che serve solo alla registrazione, e la pulizia di quanto scritto
    // finora — cambiare modalità è un ripartire da capo, e un errore rimasto
    // sullo schermo si riferirebbe a un'azione che non è più quella scelta.
    private void ApplyMode()
    {
        if (titleText != null) titleText.text = registerMode ? RegisterTitle : LoginTitle;
        if (primaryButtonText != null) primaryButtonText.text = registerMode ? RegisterPrimary : LoginPrimary;
        if (switchButtonText != null) switchButtonText.text = registerMode ? RegisterSwitch : LoginSwitch;

        if (displayNameRow != null) displayNameRow.SetActive(registerMode);

        ShowMessage(string.Empty);
        ClearInputs();
    }

    private void ClearInputs()
    {
        if (emailInput != null) emailInput.text = string.Empty;
        if (displayNameInput != null) displayNameInput.text = string.Empty;
        ClearPassword();
    }

    public void OnRegisterPressed()
    {
        if (!HasAccountManager()) return;

        RegisterResult result =
            AccountManager.Instance.Register(ReadEmail(), ReadDisplayName(), ReadPassword());

        ShowMessage(MessageFor(result));
        ClearPassword();

        if (result == RegisterResult.Success) EnterMenu();
    }

    public void OnLoginPressed()
    {
        if (!HasAccountManager()) return;

        LoginResult result = AccountManager.Instance.Login(ReadEmail(), ReadPassword());

        ShowMessage(MessageFor(result));
        ClearPassword();

        if (result == LoginResult.Success) EnterMenu();
    }

    // La password non compare né nei log né nei messaggi: si legge, si passa al
    // manager e si dimentica. Per lo stesso motivo il campo viene svuotato dopo
    // ogni tentativo, riuscito o fallito, così non resta in scena.
    private string ReadPassword()
    {
        return passwordInput != null ? passwordInput.text : string.Empty;
    }

    private string ReadEmail()
    {
        return emailInput != null ? emailInput.text : string.Empty;
    }

    private string ReadDisplayName()
    {
        return displayNameInput != null ? displayNameInput.text : string.Empty;
    }

    private void ClearPassword()
    {
        if (passwordInput != null) passwordInput.text = string.Empty;
    }

    private bool HasAccountManager()
    {
        if (AccountManager.Instance != null) return true;

        ShowMessage("Servizio account non disponibile.");
        ClearPassword();
        return false;
    }

    private void ShowMessage(string message)
    {
        if (errorText != null) errorText.text = message;
    }

    // Il percorso inverso di EnterMenu: chiude la sessione e rimette in scena il
    // pannello. L'ordine conta — prima il Logout, poi la riaccensione: accendere
    // il pannello scatena il suo OnEnable, e con la sessione ancora aperta
    // SkipIfLoggedIn lo rispegnerebbe all'istante.
    public void ShowLogin()
    {
        AccountManager.Instance?.Logout();

        if (menuButtons != null) menuButtons.SetActive(false);
        gameObject.SetActive(true);

        if (emailInput != null) emailInput.text = string.Empty;
        ClearPassword();
    }

    private void EnterMenu()
    {
        if (menuButtons != null) menuButtons.SetActive(true);

        // Per ultimo: spegnere il pannello ferma questo script insieme a lui
        gameObject.SetActive(false);
    }

    private string MessageFor(RegisterResult result)
    {
        switch (result)
        {
            case RegisterResult.Success:
                return "Registrazione completata.";
            case RegisterResult.EmailAlreadyExists:
                return "Esiste già un account con questa email.";
            case RegisterResult.InvalidEmail:
                return "Email non valida.";
            case RegisterResult.InvalidDisplayName:
                return "Scegli un nome da 1 a 20 caratteri.";
            case RegisterResult.DisplayNameAlreadyExists:
                return "Questo nome è già in uso.";
            case RegisterResult.PasswordTooShort:
                return "La password deve avere almeno 8 caratteri.";
            default:
                return "Registrazione non riuscita.";
        }
    }

    private string MessageFor(LoginResult result)
    {
        switch (result)
        {
            case LoginResult.Success:
                return "Accesso effettuato.";
            case LoginResult.EmailNotFound:
                return "Nessun account con questa email.";
            case LoginResult.WrongPassword:
                return "Password errata.";
            case LoginResult.InvalidEmail:
                return "Email non valida.";
            default:
                return "Accesso non riuscito.";
        }
    }
}
