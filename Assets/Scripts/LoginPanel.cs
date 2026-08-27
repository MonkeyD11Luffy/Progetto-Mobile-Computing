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

    [Header("Dopo l'accesso")]
    [SerializeField] private GameObject menuButtons;

    // Sessione già aperta (login ricordato dal file account): il pannello non
    // ha niente da chiedere e lascia subito il posto al menu.
    private void OnEnable()
    {
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
