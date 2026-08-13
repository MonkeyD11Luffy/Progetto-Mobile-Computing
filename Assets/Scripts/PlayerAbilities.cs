using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Abilità attive del player, da mettere sul GameObject Player accanto a
// PlayerController. Si scorrono ciclicamente come le armi, ma con due
// differenze: si sbloccano una alla volta durante la partita, quindi la
// rotazione salta quelle non ancora possedute, e ognuna ha il proprio cooldown
// indipendente dagli altri.
[RequireComponent(typeof(PlayerController))]
public class PlayerAbilities : MonoBehaviour
{
    public enum AbilityType { Dash, Shockwave, SlowTime, Shield }

    [Header("Comandi")]
    [SerializeField] private KeyCode switchKey = KeyCode.F;
    [SerializeField] private KeyCode useKey = KeyCode.R;

    [Header("Cooldown (secondi)")]
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float shockwaveCooldown = 8f;
    [SerializeField] private float slowTimeCooldown = 15f;
    [SerializeField] private float shieldCooldown = 12f;

    [Header("Scatto")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private int dashDamage = 3;
    [SerializeField] private float dashRadius = 0.7f;
    // Effetto della scia, istanziato una volta sola all'inizio dello scatto e
    // orientato nella sua direzione: si distrugge da sé (SpriteAnimator con
    // destroyOnEnd, o un ParticleSystem con Stop Action su Destroy)
    [SerializeField] private GameObject dashTrailPrefab;

    [Header("Onda d'urto")]
    [SerializeField] private float shockwaveRadius = 2.5f;
    [SerializeField] private int shockwaveDamage = 4;
    [SerializeField] private GameObject shockwaveEffectPrefab;

    [Header("Rallenta Tempo")]
    // Mai zero: la velocità del player viene compensata con 1/slowFactor
    [SerializeField] [Range(0.05f, 1f)] private float slowFactor = 0.35f;
    [SerializeField] private float slowDuration = 4f;
    // Velo a schermo che dice che l'effetto è in corso: va lasciato disattivato
    // nell'Inspector, lo accende e lo spegne questo script
    [SerializeField] private GameObject slowTimeOverlay;

    [Header("Scudo")]
    [SerializeField] private float shieldDuration = 3f;
    [SerializeField] private GameObject shieldVisualPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI abilityText;

    [Header("Test")]
    // TEMPORANEO: finché il negozio non vende le abilità non c'è modo di
    // sbloccarle in partita, quindi il sistema non sarebbe provabile. Va tolto
    // insieme a Start() quando le abilità saranno in vendita.
    [SerializeField] private bool unlockAllForTesting = false;

    // Le abilità nell'ordine in cui sono dichiarate nell'enum: è anche l'ordine
    // in cui switchKey le fa girare. Ricavarla dall'enum invece di scrivere 4
    // tiene allineati gli array se un domani se ne aggiunge una quinta.
    // Pubblica perché la scorre anche ShopRoom, per sapere cosa può vendere.
    public static readonly AbilityType[] AllAbilities =
        (AbilityType[])System.Enum.GetValues(typeof(AbilityType));

    // Indicizzati da (int)AbilityType: niente dizionari, i valori dell'enum
    // sono contigui e partono da zero
    private readonly bool[] unlocked = new bool[AllAbilities.Length];
    private readonly float[] cooldownTimers = new float[AllAbilities.Length];

    private PlayerController player;

    private AbilityType currentAbility;
    // Finché è false non c'è niente da usare e currentAbility non significa
    // nulla: la prima Unlock la fa puntare all'abilità appena ottenuta
    private bool hasSelection;

    // Stato degli effetti in corso. Serve anche a OnDisable, che deve poter
    // rimettere a posto quello che un effetto interrotto lascerebbe alterato.
    private bool dashActive;
    private readonly HashSet<GameObject> dashHitEnemies = new HashSet<GameObject>();
    private bool slowTimeActive;
    private float defaultFixedDeltaTime;
    private GameObject shieldVisual;
    private bool shieldActive;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        // Letto una volta sola: dopo che SlowTime l'ha modificato, il valore di
        // partenza non è più ricavabile da Time.fixedDeltaTime
        defaultFixedDeltaTime = Time.fixedDeltaTime;

        UpdateAbilityUI();
    }

    private void OnEnable()
    {
        RoomManager.RoomEntered += OnRoomEntered;
    }

    // Ricarica scena, morte del player, componente spento a mano: un effetto
    // interrotto a metà lascerebbe il gioco rallentato o il player immune per
    // sempre. fixedDeltaTime in particolare sopravvive al cambio di scena.
    private void OnDisable()
    {
        // Prima di tutto: l'evento è statico e sopravvive alla ricarica della
        // scena, quindi senza questo resterebbe iscritto un player distrutto
        RoomManager.RoomEntered -= OnRoomEntered;

        if (dashActive) EndDash();
        if (slowTimeActive) EndSlowTime();
        if (shieldActive) EndShield();
    }

    // Il rallentamento vale per la stanza in cui è stato usato: portarselo
    // dietro oltre la porta lascerebbe il gioco lento in una stanza appena
    // popolata, con i suoi nemici già in movimento.
    private void OnRoomEntered(GameObject room)
    {
        // EndSlowTime rimette a posto tutto quanto — timeScale, fixedDeltaTime,
        // velocità del player e velo — ed esce da solo se non c'è niente da
        // interrompere, come alla prima stanza
        EndSlowTime();
    }

    // TEMPORANEO, insieme a unlockAllForTesting. In Start e non in Awake per
    // lasciare il tempo a chi sblocca abilità dall'Awake di farlo prima.
    private void Start()
    {
        if (!unlockAllForTesting) return;

        // In ordine di enum, quindi la selezione iniziale finisce sulla prima:
        // è Unlock stessa a puntarci, essendo la prima a essere sbloccata
        foreach (AbilityType ability in AllAbilities)
        {
            Unlock(ability);
        }
    }

    private void Update()
    {
        // Stessa guardia di PlayerController.Update(): Time.timeScale = 0 ferma
        // la fisica ma non Update(), quindi senza questo le abilità
        // risponderebbero dalla pausa e dalle schermate di fine partita
        if (Time.timeScale == 0f) return;
        if (player != null && player.IsDead) return;

        TickCooldowns();
        HandleAbilitySwitch();
        HandleAbilityUse();
    }

    // --- sblocco -------------------------------------------------------------

    public void Unlock(AbilityType ability)
    {
        int index = (int)ability;
        if (unlocked[index]) return;

        unlocked[index] = true;

        // La prima sbloccata diventa anche quella selezionata: altrimenti il
        // player avrebbe un'abilità in tasca e la UI ne mostrerebbe un'altra,
        // e dovrebbe premere switchKey per arrivarci
        if (!hasSelection)
        {
            currentAbility = ability;
            hasSelection = true;
        }

        UpdateAbilityUI();
    }

    public bool IsUnlocked(AbilityType ability)
    {
        return unlocked[(int)ability];
    }

    // --- input ---------------------------------------------------------------

    private void TickCooldowns()
    {
        bool currentChanged = false;

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] <= 0f) continue;

            // Mathf.Max evita che il timer scenda sotto zero: il testo mostra i
            // secondi mancanti e "-0,1s" si leggerebbe per un frame
            cooldownTimers[i] = Mathf.Max(0f, cooldownTimers[i] - Time.deltaTime);

            if (hasSelection && i == (int)currentAbility) currentChanged = true;
        }

        // Solo se scorre il cooldown di quella mostrata: le altre non si vedono
        if (currentChanged) UpdateAbilityUI();
    }

    // Gira in avanti fino alla prossima sbloccata. Il ciclo prova tutte le
    // abilità una volta sola: se nessuna è sbloccata esce senza cambiare niente,
    // e se l'unica posseduta è già quella corrente ci si ferma sopra.
    private void HandleAbilitySwitch()
    {
        if (!Input.GetKeyDown(switchKey)) return;

        int count = AllAbilities.Length;
        int start = (int)currentAbility;

        for (int step = 1; step <= count; step++)
        {
            int index = (start + step) % count;
            if (!unlocked[index]) continue;

            currentAbility = (AbilityType)index;
            hasSelection = true;
            UpdateAbilityUI();
            return;
        }
    }

    private void HandleAbilityUse()
    {
        if (!Input.GetKeyDown(useKey)) return;
        if (!hasSelection) return;

        int index = (int)currentAbility;

        if (!unlocked[index]) return;
        if (cooldownTimers[index] > 0f) return;

        // Il cooldown parte prima dell'effetto: un'abilità che dovesse
        // disattivare questo componente lascerebbe comunque il timer avviato
        cooldownTimers[index] = CooldownOf(currentAbility);

        Execute(currentAbility);
        UpdateAbilityUI();
    }

    // --- effetti -------------------------------------------------------------

    private void Execute(AbilityType ability)
    {
        switch (ability)
        {
            case AbilityType.Dash:
                Dash();
                break;

            case AbilityType.Shockwave:
                Shockwave();
                break;

            case AbilityType.SlowTime:
                SlowTime();
                break;

            case AbilityType.Shield:
                Shield();
                break;
        }
    }

    // --- scatto --------------------------------------------------------------

    // Scatto rapido, invulnerabile, che fa danno ai nemici attraversati.
    private void Dash()
    {
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        // Da fermo si scatta nell'ultima direzione percorsa, non in quella di
        // tiro: sparare e camminare sono due input separati, e prendere la mira
        // manderebbe lo scatto da una parte mentre il player ne guarda un'altra.
        // Nessun ripiego sotto: LastMoveDirection non è mai zero.
        Vector2 direction = player.MoveInput != Vector2.zero
            ? player.MoveInput
            : player.LastMoveDirection;

        dashHitEnemies.Clear();
        dashActive = true;

        player.SetMovementOverride(direction.normalized * dashSpeed);
        player.SetAbilityImmunity(true);

        SpawnDashTrail(direction);

        // Tempo di gioco e non reale: sotto Rallenta Tempo lo scatto dura di più
        // a schermo ma copre la stessa distanza
        yield return new WaitForSeconds(dashDuration);

        EndDash();
    }

    // Effetto unico piantato dove lo scatto comincia, orientato come lui. Resta
    // fermo lì mentre il player si allontana: è la scia che il player si lascia
    // dietro, non un oggetto che lo segue.
    private void SpawnDashTrail(Vector2 direction)
    {
        if (dashTrailPrefab == null) return;

        // Stessa formula di PlayerController.ShowMeleeVisual: lo sprite va
        // disegnato con la punta verso destra, l'angolo lo gira dove serve
        Quaternion rotation = Quaternion.Euler(0f, 0f, VectorUtils.ToAngle(direction));

        // Figlio della stanza corrente e non della radice della scena: così lo
        // spegne ActivateOnly insieme alla stanza, invece di restare appeso a
        // mezz'aria dopo un cambio stanza
        Transform parent = RoomManager.Instance != null && RoomManager.Instance.CurrentRoom != null
            ? RoomManager.Instance.CurrentRoom.transform
            : null;

        Instantiate(dashTrailPrefab, transform.position, rotation, parent);
    }

    // Come EndSlowTime ed EndShield: idempotente, perché la fine può arrivare
    // dalla coroutine o da OnDisable, e togliere due volte l'immunità
    // scompenserebbe il contatore in PlayerController
    private void EndDash()
    {
        if (!dashActive) return;

        dashActive = false;

        if (player == null) return;

        player.ClearMovementOverride();
        player.SetAbilityImmunity(false);
    }

    // Il danno dello scatto si applica qui e non nella coroutine perché va
    // campionato al passo di fisica: fra un frame e l'altro il player percorre
    // parecchia strada, e un controllo per frame si lascerebbe indietro i nemici
    // sfiorati a metà percorso.
    private void FixedUpdate()
    {
        if (!dashActive) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            // Add restituisce false se c'era già: un nemico attraversato per
            // più passi di fisica prende il danno una volta sola per scatto
            if (!dashHitEnemies.Add(hit.gameObject)) continue;

            // Come ProjectileController e BombController: ogni nemico ha il suo
            // TakeDamage privato, SendMessage lo chiama qualunque script sia
            hit.gameObject.SendMessage("TakeDamage", dashDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    // --- onda d'urto ---------------------------------------------------------

    // Colpo ad area istantaneo attorno al player, che spazza via anche i
    // proiettili nemici: è la via d'uscita quando si è circondati.
    private void Shockwave()
    {
        if (shockwaveEffectPrefab != null)
        {
            Instantiate(shockwaveEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.gameObject.SendMessage("TakeDamage", shockwaveDamage, SendMessageOptions.DontRequireReceiver);
                continue;
            }

            if (hit.CompareTag("EnemyProjectile")) Destroy(hit.gameObject);
        }
    }

    // --- rallenta tempo ------------------------------------------------------

    private void SlowTime()
    {
        StartCoroutine(SlowTimeRoutine());
    }

    private IEnumerator SlowTimeRoutine()
    {
        BeginSlowTime();

        float elapsed = 0f;

        // Attesa in tempo reale, ma contata a mano invece che con
        // WaitForSecondsRealtime: il ciclo deve poter uscire nel frame stesso in
        // cui arriva la pausa, e un'attesa unica si accorgerebbe di tutto solo
        // alla scadenza, a pannello già a schermo.
        while (elapsed < slowDuration)
        {
            // slowTimeActive cade quando l'effetto è già stato chiuso da fuori,
            // p.es. da un cambio stanza: da lì in poi il ciclo non ha più niente
            // da sorvegliare e continuare significherebbe solo richiamare
            // EndSlowTime alla scadenza, su uno stato che non è più suo
            if (!slowTimeActive || GameIsHalted()) break;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        EndSlowTime();
    }

    private void BeginSlowTime()
    {
        slowTimeActive = true;

        Time.timeScale = slowFactor;

        // In proporzione, altrimenti la fisica gira con lo stesso passo di prima
        // e il rallentamento risulta a scatti
        Time.fixedDeltaTime = defaultFixedDeltaTime * slowFactor;

        // Il player si muove a velocità normale: è il resto del mondo a
        // rallentare, ed è questo che rende l'abilità utile invece che simmetrica
        player.SetAbilitySpeedMultiplier(1f / slowFactor);

        if (slowTimeOverlay != null) slowTimeOverlay.SetActive(true);
    }

    private void EndSlowTime()
    {
        if (!slowTimeActive) return;

        slowTimeActive = false;

        // Prima di tutto il resto: qualunque sia il motivo della fine — scadenza,
        // pausa, game over, ricarica della scena — il velo non deve restare a
        // schermo su una partita che non è più rallentata
        if (slowTimeOverlay != null) slowTimeOverlay.SetActive(false);

        Time.fixedDeltaTime = defaultFixedDeltaTime;

        if (player != null) player.ClearAbilitySpeedMultiplier();

        // Il timeScale si restituisce solo se il gioco sta ancora scorrendo. Se
        // siamo qui per una pausa o un game over quello zero l'ha scritto
        // GameManager, ed è suo: rimetterlo a 1 farebbe ripartire la partita
        // sotto il pannello, ed è esattamente il caso in cui la coroutine, che
        // conta in tempo reale, continua a girare a gioco fermo.
        if (!GameIsHalted()) Time.timeScale = 1f;
    }

    // Senza GameManager in scena non c'è nessuno a cui restituire il timeScale
    private static bool GameIsHalted()
    {
        if (GameManager.Instance == null) return false;

        return GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver;
    }

    // --- scudo ---------------------------------------------------------------

    private void Shield()
    {
        StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        shieldActive = true;
        player.SetAbilityImmunity(true);

        if (shieldVisualPrefab != null)
        {
            // Figlio del player: segue il movimento da solo, senza doverlo
            // riposizionare a ogni frame
            shieldVisual = Instantiate(shieldVisualPrefab, transform.position, Quaternion.identity, transform);
        }

        yield return new WaitForSeconds(shieldDuration);

        EndShield();
    }

    private void EndShield()
    {
        if (!shieldActive) return;

        shieldActive = false;

        if (player != null) player.SetAbilityImmunity(false);

        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
            shieldVisual = null;
        }
    }

    // --- UI ------------------------------------------------------------------

    private float CooldownOf(AbilityType ability)
    {
        return ability switch
        {
            AbilityType.Dash => dashCooldown,
            AbilityType.Shockwave => shockwaveCooldown,
            AbilityType.SlowTime => slowTimeCooldown,
            _ => shieldCooldown
        };
    }

    // Pubblico: lo usa anche il negozio per il popup dell'acquisto, e i nomi
    // mostrati al player devono stare in un posto solo
    public static string DisplayName(AbilityType ability)
    {
        return ability switch
        {
            AbilityType.Dash => "Scatto",
            AbilityType.Shockwave => "Onda d'urto",
            AbilityType.SlowTime => "Rallenta Tempo",
            AbilityType.Shield => "Scudo",
            _ => ability.ToString()
        };
    }

    private void UpdateAbilityUI()
    {
        if (abilityText == null) return;

        if (!hasSelection)
        {
            abilityText.text = "Abilità: nessuna";
            return;
        }

        float remaining = cooldownTimers[(int)currentAbility];
        string state = remaining > 0f ? $"{remaining:F1}s" : "pronta";

        abilityText.text = $"Abilità: {DisplayName(currentAbility)} ({state})";
    }
}
