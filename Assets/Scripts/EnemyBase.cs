using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Classe base comune a tutti i nemici: vita, morte, danno da contatto,
// riferimento al player e utilità per sparare.
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Vita")]
    [SerializeField] protected int maxHealth = 3;

    [Header("Danno da contatto")]
    [SerializeField] protected int contactDamage = 1; // 0 = non fa danno toccando il player

    [Header("Crediti")]
    [SerializeField] protected int creditDrop = 1; // 0 = non lascia crediti
    // Probabilità che il drop avvenga: 1 = sempre, 0 = mai
    [SerializeField] [Range(0f, 1f)] protected float creditDropChance = 0.5f;

    [Header("Feedback")]
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private float deathEffectDuration = 0.2f;

    [Header("Aggiramento ostacoli")]
    // Quanto avanti guarda il nemico prima di decidere se scartare
    [SerializeField] private float obstacleCheckDistance = 1f;
    // Quanto pesa lo scarto laterale rispetto alla direzione verso il player
    [SerializeField] private float avoidanceStrength = 1f;

    [Header("Navigazione")]
    [SerializeField] private float pathRecalculateInterval = 0.3f;
    // Quanto vicino deve arrivare al waypoint per considerarlo raggiunto
    [SerializeField] private float waypointReachedDistance = 0.15f;
    // Da quanto tempo deve essere praticamente fermo prima di reagire
    [SerializeField] private float stuckTimeout = 0.5f;
    // Frazione della distanza attesa (velocità × stuckTimeout) sotto la quale
    // il nemico è considerato bloccato
    [SerializeField] [Range(0f, 1f)] private float stuckProgressFraction = 0.25f;

    private const string WallTag = "Wall";

    // Dopo tanti ricalcoli falliti di fila si smette di insistere sul
    // pathfinding e si torna al comportamento normale: senza questo il nemico
    // resterebbe per sempre a ignorare la linea di vista senza un percorso.
    private const int MaxPathRetriesWhenStuck = 3;

    private RoomNavGrid navGrid;
    private List<Vector2> currentPath;
    private int waypointIndex;
    private float pathTimer;

    // Raggio del corpo: la linea di vista va verificata con lo spessore vero
    // del nemico, non con un raggio sottile.
    private float bodyRadius;

    // Posizione all'inizio della finestra di osservazione dello stallo
    private Vector2 stuckWindowStart;
    private float stuckTimer;
    private bool ignoreLineOfSight;
    private int pathRetriesWhenStuck;

    private Vector2 moveDirection;
    private float lastMoveTime = float.NegativeInfinity;

    // Solo per il gizmo: quale dei due rami ha comandato l'ultimo movimento
    private bool usedDirectMove;

    protected Rigidbody2D rb;
    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    protected int currentHealth;
    protected bool isDead;
    protected bool isFlashing;

    protected Color baseSpriteColor;

    private Coroutine flashCoroutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) baseSpriteColor = spriteRenderer.color;

        // Letto ora, finché il collider è attivo: il Teleporter lo disabilita
        // mentre è sparito.
        //
        // Semi-diagonale e non metà del lato corto: il corpo è un box, e sulla
        // diagonale gli spigoli sporgono oltre il cerchio inscritto. Con il
        // raggio inscritto il cast passa accanto a uno spigolo, dichiara la
        // strada libera e il nemico ci si incastra contro.
        Collider2D bodyCollider = GetComponent<Collider2D>();
        if (bodyCollider != null)
        {
            bodyRadius = bodyCollider.bounds.extents.magnitude;
        }
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Senza griglia si insegue in linea retta, come prima
        navGrid = GetComponentInParent<RoomNavGrid>();

        // Primo ricalcolo sfalsato: senza, tutti i nemici della stanza
        // cercherebbero il percorso nello stesso frame.
        pathTimer = Random.Range(0f, pathRecalculateInterval);

        stuckWindowStart = rb != null ? rb.position : (Vector2)transform.position;
    }

    // Pubblico: viene chiamato via SendMessage da ProjectileController e BombController
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        PlayHitFlash();

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    private void PlayHitFlash()
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        isFlashing = true;
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(hitFlashDuration);

        isFlashing = false;
        RestoreBaseColor();
        flashCoroutine = null;
    }

    // Sovrascritto da nemici che gestiscono il proprio colore (fasi, rabbia, ecc.)
    // per evitare che il flash bianco venga ripristinato con il colore sbagliato.
    protected virtual void RestoreBaseColor()
    {
        if (spriteRenderer != null) spriteRenderer.color = baseSpriteColor;
    }

    protected virtual void Die()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemyDeath();

            // I crediti cadono dove il nemico è morto: li fa nascere il
            // RoomManager, che sa in quale stanza appenderli
            if (creditDrop > 0 && Random.value <= creditDropChance)
            {
                RoomManager.Instance.SpawnCredits(transform.position, creditDrop);
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathPopRoutine());
        Destroy(gameObject, deathEffectDuration);
    }

    private IEnumerator DeathPopRoutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 popScale = startScale * 1.3f;
        float half = deathEffectDuration * 0.5f;

        float t = 0f;
        while (t < half)
        {
            transform.localScale = Vector3.Lerp(startScale, popScale, t / half);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            transform.localScale = Vector3.Lerp(popScale, Vector3.zero, t / half);
            t += Time.deltaTime;
            yield return null;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected void TryDamagePlayer(GameObject other)
    {
        if (isDead || contactDamage <= 0) return;
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeContactDamage(contactDamage);
        }
    }

    // Direzione unitaria verso il player, Vector2.zero se il player non c'è
    protected Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;

        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    // Inseguimento: usato da Walker, Splitter, Shielded, Miniboss e Boss.
    // Segue un percorso calcolato sulla griglia della stanza; quando il player
    // è già in vista, o la stanza non ha una griglia, va dritto verso di lui.
    protected void MoveTowardsPlayer(float speed)
    {
        if (player == null || rb == null) return;

        Vector2 direction = DirectionToPlayer();
        if (direction == Vector2.zero) return;

        UpdateStuckState(speed);

        // Strada libera: il percorso farebbe fare un giro largo inutile. Se però
        // il nemico è appena rimasto incastrato, la vista libera è proprio ciò
        // che lo ha ingannato: la si ignora fino al ricalcolo del percorso.
        if (navGrid == null || (!ignoreLineOfSight && HasLineOfSightToPlayer()))
        {
            currentPath = null;
            MoveDirectly(direction, speed);
            return;
        }

        UpdatePath();

        if (!TryGetWaypoint(out Vector2 waypoint))
        {
            MoveDirectly(direction, speed);
            return;
        }

        usedDirectMove = false;

        Vector2 position = rb.position;
        Vector2 step = (waypoint - position).normalized;

        SetMoveDirection(speed > 0f ? step : Vector2.zero);

        // MovePosition invece della velocità: spingendo con la velocità contro
        // lo spigolo di un ostacolo il solver azzera la spinta e il nemico resta
        // incastrato, mentre MovePosition risolve la collisione scivolando lungo
        // il collider. La velocità va azzerata, altrimenti i due si sommano.
        // Il percorso aggira già i muri: nessuno scarto laterale da applicare.
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(position + step * (speed * Time.fixedDeltaTime));
    }

    // Direzione comandata nell'ultimo passo di fisica, o Vector2.zero se il
    // nemico non si sta muovendo. Con MovePosition il Rigidbody2D resta a
    // velocità zero, quindi rb.linearVelocity non è più una fonte attendibile
    // per EnemyVisuals.
    //
    // La scadenza evita di restituire una direzione vecchia quando il movimento
    // smette di essere comandato: il Boss in telegrafo e il Dasher in carica
    // azzerano la velocità da soli, senza passare da qui.
    public Vector2 MoveDirection =>
        Time.fixedTime - lastMoveTime <= Time.fixedDeltaTime * 2f ? moveDirection : Vector2.zero;

    private void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction;
        lastMoveTime = Time.fixedTime;
    }

    // Si misura lo spostamento NETTO su una finestra lunga stuckTimeout, non
    // quello di un singolo passo: un nemico che vibra contro uno spigolo si
    // muove a ogni passo di frazioni di unità, ma dopo mezzo secondo è ancora
    // dov'era. Confrontare passo per passo lo dichiarerebbe sempre in movimento.
    //
    // Il riferimento è rb.position e non transform.position: con
    // l'interpolazione attiva il transform si muove fra un passo di fisica e
    // l'altro anche quando il corpo è fermo.
    private void UpdateStuckState(float speed)
    {
        Vector2 position = rb.position;

        stuckTimer += Time.deltaTime;
        if (stuckTimer < stuckTimeout) return;

        float travelled = Vector2.Distance(position, stuckWindowStart);
        float expected = speed * stuckTimeout;

        // Finestra chiusa: si riparte da qui in ogni caso
        stuckTimer = 0f;
        stuckWindowStart = position;

        // Con speed 0 expected è 0 e non scatta mai: un nemico fermo per scelta
        // (Boss in telegrafo) non è un nemico bloccato.
        if (travelled >= expected * stuckProgressFraction) return;

        pathTimer = 0f;            // ricalcolo al prossimo UpdatePath
        ignoreLineOfSight = true;  // consumato da UpdatePath, se trova un percorso
        pathRetriesWhenStuck = 0;
    }

    // Movimento diretto, con lo scarto laterale se davanti c'è un muro: è il
    // comportamento delle stanze senza griglia e il ripiego quando il percorso
    // non esiste.
    private void MoveDirectly(Vector2 direction, float speed)
    {
        usedDirectMove = true;

        if (IsBlocked(direction))
        {
            Vector2 detour = ChooseFreeSide(direction);
            direction = (direction + detour * avoidanceStrength).normalized;
        }

        SetMoveDirection(speed > 0f ? direction : Vector2.zero);

        rb.linearVelocity = direction * speed;
    }

    private void UpdatePath()
    {
        pathTimer -= Time.deltaTime;
        if (pathTimer > 0f) return;

        pathTimer = pathRecalculateInterval;

        currentPath = Pathfinder.FindPath(navGrid, rb.position, player.position);
        waypointIndex = 0;

        if (currentPath != null && currentPath.Count > 0)
        {
            // Percorso utilizzabile: la deroga sulla linea di vista ha fatto il
            // suo lavoro e si può consumare.
            ignoreLineOfSight = false;
            pathRetriesWhenStuck = 0;
            return;
        }

        if (!ignoreLineOfSight) return;

        // Nessun percorso: si insiste per qualche tentativo, poi si molla. Il
        // nemico torna a usare la linea di vista e il movimento diretto, e sarà
        // di nuovo UpdateStuckState a riaccorgersi dell'eventuale blocco.
        pathRetriesWhenStuck++;
        if (pathRetriesWhenStuck < MaxPathRetriesWhenStuck) return;

        ignoreLineOfSight = false;
        pathRetriesWhenStuck = 0;
    }

    private bool TryGetWaypoint(out Vector2 waypoint)
    {
        waypoint = Vector2.zero;

        if (currentPath == null) return false;

        Vector2 position = transform.position;

        // Più di un waypoint alla volta può risultare già raggiunto, per esempio
        // subito dopo un ricalcolo che parte da dove il nemico si trova ora.
        while (waypointIndex < currentPath.Count &&
               Vector2.Distance(position, currentPath[waypointIndex]) <= waypointReachedDistance)
        {
            waypointIndex++;
        }

        if (waypointIndex >= currentPath.Count) return false;

        waypoint = currentPath[waypointIndex];
        return true;
    }

    // Solo per la Scene view: mostra se il percorso viene calcolato e dove
    // punta. In play mode il percorso esiste solo quando il pathfinding è
    // attivo, cioè con un RoomNavGrid e il player fuori dalla linea di vista.
    // Virtual perché una sottoclasse possa aggiungere i propri gizmo senza
    // nascondere questo.
    protected virtual void OnDrawGizmos()
    {
        // Ramo diretto: nessun percorso da mostrare, ma serve vedere che il
        // nemico sta puntando il player invece di non vedere niente.
        if (usedDirectMove)
        {
            if (player == null) return;

            Gizmos.color = new Color(1f, 0.3f, 0.5f, 0.9f);
            Gizmos.DrawLine(transform.position, player.position);
            return;
        }

        if (currentPath == null || waypointIndex >= currentPath.Count) return;

        // Dal nemico al waypoint attivo, poi di waypoint in waypoint
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);

        Vector3 previous = transform.position;

        for (int i = waypointIndex; i < currentPath.Count; i++)
        {
            Vector3 point = currentPath[i];

            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        // Il raggio è waypointReachedDistance: si vede quanto deve avvicinarsi
        // il nemico prima di passare al waypoint successivo.
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(currentPath[waypointIndex], waypointReachedDistance);
    }

    // Nessun muro fra il nemico e il player. Il controllo usa il raggio del
    // corpo e non un raggio sottile: passando di fianco a un pilastro un raycast
    // direbbe "strada libera" e il nemico ci si incastrerebbe contro.
    private bool HasLineOfSightToPlayer()
    {
        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)player.position - origin;

        float distance = toPlayer.magnitude;
        if (distance <= Mathf.Epsilon) return true;

        return FreeSpace(toPlayer / distance, distance, bodyRadius) >= distance;
    }

    // Fra le due perpendicolari vince quella libera; se lo sono entrambe (o
    // nessuna delle due) si guarda l'allineamento con la direzione originale e,
    // a parità, chi ha più spazio davanti. Con perpendicolari esatte
    // l'allineamento è sempre nullo, quindi decide lo spazio.
    private Vector2 ChooseFreeSide(Vector2 direction)
    {
        Vector2 left = VectorUtils.Rotate(direction, 90f);
        Vector2 right = VectorUtils.Rotate(direction, -90f);

        float leftSpace = FreeSpace(left, obstacleCheckDistance, 0f);
        float rightSpace = FreeSpace(right, obstacleCheckDistance, 0f);

        bool leftIsFree = leftSpace >= obstacleCheckDistance;
        bool rightIsFree = rightSpace >= obstacleCheckDistance;

        if (leftIsFree != rightIsFree) return leftIsFree ? left : right;

        float leftAlignment = Vector2.Dot(left, direction);
        float rightAlignment = Vector2.Dot(right, direction);

        if (!Mathf.Approximately(leftAlignment, rightAlignment))
        {
            return leftAlignment > rightAlignment ? left : right;
        }

        return leftSpace >= rightSpace ? left : right;
    }

    private bool IsBlocked(Vector2 direction)
    {
        return FreeSpace(direction, obstacleCheckDistance, 0f) < obstacleCheckDistance;
    }

    // Distanza dal muro più vicino in quella direzione, o maxDistance se la
    // strada è libera. Con radius > 0 si sonda uno spessore invece di una linea.
    // Solo i collider taggati "Wall" contano: gli altri nemici non vanno
    // aggirati, altrimenti un gruppo che insegue il player si sparpaglierebbe.
    private float FreeSpace(Vector2 direction, float maxDistance, float radius)
    {
        float nearest = maxDistance;

        RaycastHit2D[] hits = radius > 0f
            ? Physics2D.CircleCastAll(transform.position, radius, direction, maxDistance)
            : Physics2D.RaycastAll(transform.position, direction, maxDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.gameObject == gameObject) continue; // il proprio collider
            if (!hit.collider.CompareTag(WallTag)) continue;

            if (hit.distance < nearest) nearest = hit.distance;
        }

        return nearest;
    }

    // Utilità condivisa: usata da Turret, Teleporter, Miniboss e Boss
    protected void SpawnProjectile(GameObject prefab, Vector2 origin, Vector2 direction, float speed)
    {
        if (prefab == null) return;

        GameObject projectile = Instantiate(prefab, origin, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * speed;
        }

        projectile.transform.rotation = Quaternion.Euler(0, 0, VectorUtils.ToAngle(direction));
    }

    // Raffica a ventaglio completo attorno al nemico
    protected void FireRadialBurst(GameObject prefab, int count, float speed, float angleOffset = 0f)
    {
        if (prefab == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i + angleOffset;
            SpawnProjectile(prefab, transform.position, VectorUtils.FromAngle(angle), speed);
        }
    }

    // Genera nemici in cerchio attorno a sé (Splitter che si divide, Boss che evoca).
    // Registra da sola lo spawn nel RoomManager: senza, le porte si sbloccherebbero in anticipo.
    protected void SpawnEnemiesAroundSelf(GameObject prefab, int count, float radius)
    {
        if (prefab == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = VectorUtils.FromAngle((360f / count) * i) * radius;
            Instantiate(prefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterEnemySpawn(count);
        }
    }
}
