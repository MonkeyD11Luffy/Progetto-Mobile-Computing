using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

// Un account locale. I campi sono pubblici e non-proprietà perché JsonUtility
// serializza solo i campi pubblici (o marcati [SerializeField]).
[Serializable]
public class Account
{
    public string email;
    public string salt;          // base64, 16 byte casuali
    public string passwordHash;  // base64, PBKDF2-SHA256 del salt + password
    public int bestScore;
    public string lastPlayed;    // ISO 8601 UTC, stringa vuota se mai giocato
}

// JsonUtility non serializza né i dizionari né una List<T> passata come radice:
// serve un oggetto contenitore con la lista come campo.
[Serializable]
public class AccountDatabase
{
    public List<Account> accounts = new List<Account>();
}

public enum RegisterResult
{
    Success,
    EmailAlreadyExists,
    InvalidEmail,
    PasswordTooShort
}

public enum LoginResult
{
    Success,
    EmailNotFound,
    WrongPassword,
    InvalidEmail
}

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance;

    // Costo del PBKDF2. Alzare le iterazioni invalida gli hash già salvati solo
    // se si tocca anche il formato: qui il conteggio è fisso e uguale per tutti.
    private const int HashIterations = 100000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;
    private const int MinPasswordLength = 8;
    private const string FileName = "accounts.json";

    // Controllo minimo: qualcosa, @, qualcosa, punto, qualcosa, senza spazi.
    // Non è una validazione RFC 5322 e non deve esserlo.
    private static readonly Regex EmailPattern =
        new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private AccountDatabase database = new AccountDatabase();
    private string currentEmail;
    private string filePath;

    public string CurrentEmail => currentEmail;
    public bool IsLoggedIn => !string.IsNullOrEmpty(currentEmail);

    private void Awake()
    {
        // Il manager sopravvive ai cambi di scena: senza questa guardia, tornare
        // al menu principale ne creerebbe un secondo e la sessione di login
        // finirebbe su un'istanza diversa da quella che gli altri script leggono.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, FileName);
        Load();
    }

    // ---------------------------------------------------------------- account

    public RegisterResult Register(string email, string password)
    {
        string normalized = Normalize(email);

        if (!IsValidEmail(normalized)) return RegisterResult.InvalidEmail;
        if (password == null || password.Length < MinPasswordLength) return RegisterResult.PasswordTooShort;
        if (FindAccount(normalized) != null) return RegisterResult.EmailAlreadyExists;

        byte[] salt = new byte[SaltBytes];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        Account account = new Account
        {
            email = normalized,
            salt = Convert.ToBase64String(salt),
            passwordHash = Convert.ToBase64String(ComputeHash(password, salt)),
            bestScore = 0,
            lastPlayed = string.Empty
        };

        database.accounts.Add(account);
        Save();

        currentEmail = normalized;
        return RegisterResult.Success;
    }

    public LoginResult Login(string email, string password)
    {
        string normalized = Normalize(email);

        if (!IsValidEmail(normalized)) return LoginResult.InvalidEmail;

        Account account = FindAccount(normalized);
        if (account == null) return LoginResult.EmailNotFound;

        if (!VerifyPassword(account, password)) return LoginResult.WrongPassword;

        currentEmail = normalized;
        return LoginResult.Success;
    }

    public void Logout()
    {
        currentEmail = null;
    }

    // ----------------------------------------------------------------- record

    // Aggiorna sempre la data dell'ultima partita, il punteggio solo se migliore.
    // Restituisce true se il record è stato battuto.
    public bool SaveScore(int punteggio)
    {
        if (!IsLoggedIn) return false;

        Account account = FindAccount(currentEmail);
        if (account == null) return false;

        bool isNewRecord = punteggio > account.bestScore;
        if (isNewRecord) account.bestScore = punteggio;

        account.lastPlayed = DateTime.UtcNow.ToString("o");
        Save();

        return isNewRecord;
    }

    // Copia della lista, non quella interna: chi la ordina o la filtra per
    // mostrarla in UI non deve poter riordinare il database.
    public List<Account> GetLeaderboard()
    {
        List<Account> sorted = new List<Account>(database.accounts);
        sorted.Sort((a, b) => b.bestScore.CompareTo(a.bestScore));
        return sorted;
    }

    // ------------------------------------------------------------ persistenza

    private void Load()
    {
        if (!File.Exists(filePath))
        {
            database = new AccountDatabase();
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            database = JsonUtility.FromJson<AccountDatabase>(json) ?? new AccountDatabase();

            // FromJson su un JSON valido ma di forma diversa lascia il campo a null
            if (database.accounts == null) database.accounts = new List<Account>();
        }
        catch (Exception e)
        {
            // Nessun dato dell'account nel log: solo il motivo dell'errore di I/O
            Debug.LogWarning($"AccountManager: file account illeggibile, si riparte da vuoto ({e.GetType().Name}).");
            database = new AccountDatabase();
        }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(database, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"AccountManager: salvataggio account fallito ({e.GetType().Name}).");
        }
    }

    // ---------------------------------------------------------------- utilità

    private Account FindAccount(string normalizedEmail)
    {
        foreach (Account account in database.accounts)
        {
            if (account != null && Normalize(account.email) == normalizedEmail) return account;
        }

        return null;
    }

    private static string Normalize(string email)
    {
        return email == null ? string.Empty : email.Trim().ToLowerInvariant();
    }

    private static bool IsValidEmail(string normalizedEmail)
    {
        return !string.IsNullOrEmpty(normalizedEmail) && EmailPattern.IsMatch(normalizedEmail);
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        using (Rfc2898DeriveBytes derive =
               new Rfc2898DeriveBytes(password, salt, HashIterations, HashAlgorithmName.SHA256))
        {
            return derive.GetBytes(HashBytes);
        }
    }

    private static bool VerifyPassword(Account account, string password)
    {
        if (password == null || string.IsNullOrEmpty(account.salt) || string.IsNullOrEmpty(account.passwordHash))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(account.salt);
            expected = Convert.FromBase64String(account.passwordHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return FixedTimeEquals(ComputeHash(password, salt), expected);
    }

    // Confronto a tempo costante: uscire al primo byte diverso renderebbe la
    // durata della verifica dipendente da quanti byte dell'hash sono giusti.
    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }

        return diff == 0;
    }
}
