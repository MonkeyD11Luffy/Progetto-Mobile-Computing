using System.Collections.Generic;
using UnityEngine;

public class TeleporterController : EnemyBase
{
    [Header("Tempi")]
    [SerializeField] private float idleDuration = 1.8f;
    [SerializeField] private float vanishDuration = 0.4f;

    [Header("Teletrasporto")]
    [SerializeField] private float minDistanceFromPlayer = 2.5f;
    [SerializeField] private float maxDistanceFromPlayer = 4f;

    [Header("Confini stanza")]
    // Usati solo se la stanza non dichiara i propri confini con un RoomBounds
    [SerializeField] private float roomHalfWidth = 3f;
    [SerializeField] private float roomHalfHeight = 3f;

    [Header("Riposizionamento")]
    // Margine attorno all'area occupata dal teleporter: evita di riapparire
    // appiccicato a un ostacolo anche quando tecnicamente non lo tocca.
    [SerializeField] private float obstacleMargin = 0.25f;
    [SerializeField] private int maxPlacementAttempts = 12;

    [Header("Preavviso")]
    // Segnala al player dove ricomparirà il teleporter, mentre è invisibile
    [SerializeField] private GameObject arrivalIndicatorPrefab;

    [Header("Sparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 7f;

    private enum TeleporterState { Idle, Vanishing }

    // Tentativi del ripiego verso il centro, quando tutti quelli vicini al
    // player sono finiti dentro a un muro.
    private const int CenterFallbackAttempts = 6;

    private const string ObstacleTag = "Wall";
    private const string EnemyTag = "Enemy";

    // Durante la sparizione il collider è disattivato, quindi Physics2D non
    // vede un teleporter che sta per ricomparire: due di loro sceglierebbero
    // lo stesso punto. Le destinazioni già scelte restano annotate qui finché
    // non vengono raggiunte.
    private static readonly Dictionary<TeleporterController, Vector2> reservedArrivals =
        new Dictionary<TeleporterController, Vector2>();

    private Collider2D col;
    private Vector2 colliderSize;
    private TeleporterState currentState;
    private float stateTimer;

    // Scelta all'inizio della sparizione, applicata alla ricomparsa: l'indicatore
    // può così mostrarla al player per tutta la durata del vanish.
    private Vector2 arrivalPosition;
    private GameObject arrivalIndicator;

    protected override void Awake()
    {
        base.Awake(); // qui viene già preso lo SpriteRenderer

        col = GetComponent<Collider2D>();

        // Le dimensioni si leggono ora, finché il collider è attivo: durante
        // Vanishing viene disabilitato e i suoi bounds non sarebbero più validi.
        if (col != null) colliderSize = col.bounds.size;
    }

    protected override void Start()
    {
        base.Start();

        // Se la stanza dichiara i propri confini, valgono i suoi: così lo
        // stesso prefab di Teleporter funziona in stanze di misure diverse.
        RoomBounds bounds = GetComponentInParent<RoomBounds>();
        if (bounds != null)
        {
            roomHalfWidth = bounds.HalfWidth;
            roomHalfHeight = bounds.HalfHeight;
        }

        currentState = TeleporterState.Idle;
        stateTimer = idleDuration;
    }

    private void Update()
    {
        if (isDead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        if (currentState == TeleporterState.Idle)
        {
            StartVanish();
        }
        else
        {
            Reappear();
        }
    }

    private void StartVanish()
    {
        currentState = TeleporterState.Vanishing;
        stateTimer = vanishDuration;
        SetVisible(false);

        // Dopo SetVisible: il controllo ostacoli gira con il collider proprio
        // già disattivato, così il teleporter non si rileva da solo.
        arrivalPosition = FindArrivalPosition();
        reservedArrivals[this] = arrivalPosition;

        ShowArrivalIndicator();
    }

    private void Reappear()
    {
        currentState = TeleporterState.Idle;
        stateTimer = idleDuration;

        transform.position = arrivalPosition;
        reservedArrivals.Remove(this);

        ClearArrivalIndicator();

        SetVisible(true);
        Fire();
    }

    // Un teleporter ucciso durante il vanish lascerebbe l'indicatore in scena
    // e la sua destinazione prenotata per sempre
    protected override void Die()
    {
        reservedArrivals.Remove(this);
        ClearArrivalIndicator();

        base.Die();
    }

    // La lista è statica e sopravvive al ricaricamento della scena: senza
    // questo, i teleporter della partita precedente continuerebbero a
    // occupare le loro destinazioni.
    private void OnDestroy()
    {
        reservedArrivals.Remove(this);
    }

    private void ShowArrivalIndicator()
    {
        if (arrivalIndicatorPrefab == null) return;

        arrivalIndicator = Instantiate(arrivalIndicatorPrefab, arrivalPosition, Quaternion.identity, transform.parent);
    }

    private void ClearArrivalIndicator()
    {
        if (arrivalIndicator == null) return;

        Destroy(arrivalIndicator);
        arrivalIndicator = null;
    }

    private void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (col != null) col.enabled = visible;
    }

    // Restituisce dove il teleporter ricomparirà, senza spostarlo: il
    // movimento vero avviene alla fine della sparizione.
    private Vector2 FindArrivalPosition()
    {
        if (player == null) return transform.position;

        Vector2 roomCenter = transform.parent != null
            ? (Vector2)transform.parent.position
            : Vector2.zero;

        // Prima scelta: vicino al player, come prima, ma solo se l'area è libera
        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 candidate = RandomPositionNearPlayer(roomCenter);
            if (IsFree(candidate)) return candidate;
        }

        // Ripiego: verso il centro della stanza, dove è più difficile trovare
        // ostacoli. Costa la distanza dal player, ma è comunque una posizione
        // valida.
        for (int i = 0; i < CenterFallbackAttempts; i++)
        {
            Vector2 candidate = RandomPositionNearCenter(roomCenter);
            if (IsFree(candidate)) return candidate;
        }

        // Nessuna posizione libera: meglio un teleporter fermo che uno
        // incastrato dentro un muro.
        return transform.position;
    }

    private Vector2 RandomPositionNearPlayer(Vector2 roomCenter)
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

        Vector2 offset = VectorUtils.FromAngle(angle) * distance;

        return ClampToRoom((Vector2)player.position + offset, roomCenter, 1f);
    }

    private Vector2 RandomPositionNearCenter(Vector2 roomCenter)
    {
        Vector2 offset = new Vector2(
            Random.Range(-roomHalfWidth, roomHalfWidth),
            Random.Range(-roomHalfHeight, roomHalfHeight));

        return ClampToRoom(roomCenter + offset, roomCenter, 0.5f);
    }

    private Vector2 ClampToRoom(Vector2 position, Vector2 roomCenter, float boundsScale)
    {
        float halfWidth = roomHalfWidth * boundsScale;
        float halfHeight = roomHalfHeight * boundsScale;

        position.x = Mathf.Clamp(position.x, roomCenter.x - halfWidth, roomCenter.x + halfWidth);
        position.y = Mathf.Clamp(position.y, roomCenter.y - halfHeight, roomCenter.y + halfHeight);

        return position;
    }

    // L'area che il teleporter occuperebbe, allargata del margine, non deve
    // contenere né muri né altri nemici.
    private bool IsFree(Vector2 position)
    {
        Vector2 size = colliderSize + Vector2.one * (obstacleMargin * 2f);

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(position, size, 0f))
        {
            if (hit == col) continue; // il proprio collider non è un ostacolo

            if (hit.CompareTag(ObstacleTag) || hit.CompareTag(EnemyTag)) return false;
        }

        return IsFarFromReservedArrivals(position, size);
    }

    // Copre i teleporter che stanno per ricomparire: non hanno un collider
    // attivo da intercettare, solo una destinazione prenotata.
    private bool IsFarFromReservedArrivals(Vector2 position, Vector2 size)
    {
        Vector2 halfSize = size * 0.5f;

        foreach (KeyValuePair<TeleporterController, Vector2> reserved in reservedArrivals)
        {
            // La propria prenotazione va ignorata: si sta scegliendo proprio
            // quella. Le chiavi nulle sono teleporter già distrutti.
            if (reserved.Key == this || reserved.Key == null) continue;

            Vector2 delta = reserved.Value - position;
            if (Mathf.Abs(delta.x) < halfSize.x && Mathf.Abs(delta.y) < halfSize.y) return false;
        }

        return true;
    }

    private void Fire()
    {
        if (player == null) return;

        SpawnProjectile(projectilePrefab, transform.position, DirectionToPlayer(), projectileSpeed);
    }
}