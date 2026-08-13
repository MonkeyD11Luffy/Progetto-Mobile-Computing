using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Da mettere sulla radice del template della stanza negozio. Gli slot sono i
// posti sul bancone: il primo tiene la merce di consumo, gli ultimi due le
// abilità, quelli in mezzo i potenziamenti permanenti, sempre diversi fra loro.
// Con quattro slot il bancone è quindi un consumabile, un potenziamento e due
// abilità. A differenza della stanza tesoro qui si può comprare tutto, basta
// avere i crediti: è ShopItem, aggiunto a ogni pezzo di merce, a incassare il
// prezzo e a farsi consegnare quello che l'oggetto contiene.
public class ShopRoom : MonoBehaviour
{
    [SerializeField] private Transform[] slots;
    [SerializeField] private GameObject[] consumablePrefabs;
    [SerializeField] private int consumablePrice = 3;
    [SerializeField] private int upgradePrice = 8;

    [Header("Abilità in vendita")]
    [SerializeField] private int abilityPrice = 15;
    // Prefab con il solo SpriteRenderer e un collider trigger: è merce senza
    // effetto proprio, lo sprite e l'abilità li scrive questo script
    [SerializeField] private GameObject abilityPickupPrefab;
    // Indicizzato da (int)PlayerAbilities.AbilityType, cioè nell'ordine
    // dell'enum: Scatto, Onda d'urto, Rallenta Tempo, Scudo
    [SerializeField] private Sprite[] abilitySprites;

    [Header("Cartellino del prezzo")]
    // Il cartellino con dentro un TextMeshProUGUI: l'aspetto è tutto suo, qui
    // ci si limita a scriverci il numero
    [SerializeField] private GameObject priceLabelPrefab;

    // Il bancone si legge da fuori verso dentro: i primi slot alla merce di
    // consumo, gli ultimi alle abilità, quelli che restano in mezzo ai
    // potenziamenti permanenti
    private const int ConsumableSlots = 1;
    private const int AbilitySlots = 2;

    // Il cartellino sta sopra la merce, in coordinate locali dell'oggetto
    private static readonly Vector3 PriceLabelPosition = new Vector3(0f, 0.6f, 0f);

    // Chiamato dal DungeonGenerator subito dopo aver istanziato la stanza, come
    // per TreasureRoom: la stanza è ancora disattivata, quindi la merce nasce
    // spenta insieme a lei e i suoi Awake girano solo quando il player entra.
    public void Populate(GameObject[] upgradePrefabs)
    {
        if (slots == null || slots.Length == 0) return;

        // Copia mescolata dei potenziamenti: si pescano in ordine, così due
        // slot non offrono mai lo stesso potenziamento
        List<GameObject> upgradePool = Shuffled(upgradePrefabs);

        // Le abilità che il player non ha ancora, lette una volta sola: gli slot
        // ci pescano dentro togliendo quello che vendono, così i due posti del
        // bancone non offrono mai la stessa abilità
        List<PlayerAbilities.AbilityType> abilityPool = AvailableAbilities();

        int nextUpgrade = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            Transform slot = slots[i];
            if (slot == null) continue;

            if (i < ConsumableSlots)
            {
                // La merce di consumo può ripetersi: due cure sul bancone sono
                // un'offerta legittima, e i prefab configurati sono pochi
                GameObject consumable = RandomPrefab(consumablePrefabs);
                if (consumable == null) continue;

                PlaceItem(consumable, slot, consumablePrice);
                continue;
            }

            // Gli ultimi posti del bancone sono quelli delle abilità. Se non
            // c'è niente da vendere — prefab non configurato, o il player ne
            // possiede già abbastanza — tornano slot come gli altri.
            if (IsAbilitySlot(i) && TryPlaceAbility(slot, abilityPool)) continue;

            // Finiti i potenziamenti disponibili lo slot resta vuoto: meglio un
            // bancone spoglio che vendere due volte la stessa cosa
            if (nextUpgrade >= upgradePool.Count) continue;

            PlaceItem(upgradePool[nextUpgrade], slot, upgradePrice);
            nextUpgrade++;
        }
    }

    // --- abilità -------------------------------------------------------------

    // I posti in fondo al bancone, ma senza mai rubare quelli della merce di
    // consumo: con un bancone corto le abilità sono le prime a cedere il posto.
    private bool IsAbilitySlot(int index)
    {
        return index >= ConsumableSlots && index >= slots.Length - AbilitySlots;
    }

    // Vera se lo slot è stato riempito con un'abilità. Falsa quando non c'è
    // niente da vendere, e allora il chiamante ci rimette un potenziamento.
    private bool TryPlaceAbility(Transform slot, List<PlayerAbilities.AbilityType> pool)
    {
        if (abilityPickupPrefab == null) return false;

        if (pool == null || pool.Count == 0) return false;

        // Tolta dalla lista: l'altro slot delle abilità pesca da quel che resta
        int pick = Random.Range(0, pool.Count);
        PlayerAbilities.AbilityType ability = pool[pick];
        pool.RemoveAt(pick);

        GameObject item = Instantiate(abilityPickupPrefab, slot.position, Quaternion.identity, slot);

        // Prima lo ShopAbility e poi lo ShopItem: quest'ultimo cerca in Awake
        // chi consegna la merce, e a quel punto deve trovarlo già lì
        ShopAbility shopAbility = item.GetComponent<ShopAbility>();
        if (shopAbility == null) shopAbility = item.AddComponent<ShopAbility>();

        shopAbility.Setup(ability);

        ApplyAbilitySprite(item, ability);

        TMP_Text label = CreatePriceLabel(item.transform, abilityPrice);

        ShopItem shopItem = item.GetComponent<ShopItem>();
        if (shopItem == null) shopItem = item.AddComponent<ShopItem>();

        shopItem.Setup(abilityPrice, label);

        return true;
    }

    // Le abilità che il player non ha ancora: il negozio non deve vendere due
    // volte la stessa cosa, e un'abilità già posseduta costerebbe cara per non
    // fare niente. Con tre già sbloccate ne resta una sola in vendita, con tutte
    // e quattro la lista è vuota e i due slot passano ai potenziamenti.
    private static List<PlayerAbilities.AbilityType> AvailableAbilities()
    {
        PlayerAbilities abilities = FindPlayerAbilities();

        List<PlayerAbilities.AbilityType> available = new List<PlayerAbilities.AbilityType>();

        foreach (PlayerAbilities.AbilityType type in PlayerAbilities.AllAbilities)
        {
            // Senza PlayerAbilities in scena non si sa cosa il player possieda:
            // si mettono in vendita tutte, e sarà l'acquisto ad accorgersene
            if (abilities != null && abilities.IsUnlocked(type)) continue;

            available.Add(type);
        }

        return available;
    }

    // Per tag, come fanno EnemyBase e RoomManager: il negozio nasce a runtime da
    // un prefab, e un prefab non può tenere un riferimento a un oggetto di scena.
    private static PlayerAbilities FindPlayerAbilities()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        return playerObject != null ? playerObject.GetComponent<PlayerAbilities>() : null;
    }

    private void ApplyAbilitySprite(GameObject item, PlayerAbilities.AbilityType ability)
    {
        int index = (int)ability;

        if (abilitySprites == null || index >= abilitySprites.Length || abilitySprites[index] == null)
        {
            Debug.LogWarning($"Nessuno sprite configurato per l'abilità {ability}: " +
                             "l'oggetto in vendita resterebbe invisibile sul bancone.", this);
            return;
        }

        // true: la merce nasce dentro alla stanza disattivata, quindi come per
        // il cartellino senza questo il renderer non si troverebbe
        SpriteRenderer renderer = item.GetComponentInChildren<SpriteRenderer>(true);

        if (renderer == null)
        {
            Debug.LogWarning($"Il prefab '{abilityPickupPrefab.name}' non ha nessuno SpriteRenderer: " +
                             "l'abilità in vendita non si vedrebbe.", abilityPickupPrefab);
            return;
        }

        renderer.sprite = abilitySprites[index];
    }

    // Crea la merce sullo slot con il suo cartellino e la rende acquistabile.
    // L'oggetto venduto è un pickup qualsiasi: il negozio non sa cosa fa, gli
    // aggiunge solo il prezzo davanti.
    private void PlaceItem(GameObject prefab, Transform slot, int price)
    {
        GameObject item = Instantiate(prefab, slot.position, Quaternion.identity, slot);

        // Marcato come merce prima ancora che lo ShopItem esista: da qui in poi
        // il suo trigger non raccoglie più niente, e l'effetto può partire solo
        // da ShopItem a pagamento riuscito.
        PickupController pickup = item.GetComponent<PickupController>();

        if (pickup == null)
        {
            Debug.LogWarning($"Il prefab '{prefab.name}' in vendita nel negozio non ha un PickupController: " +
                             "pagandolo non succederebbe niente.", prefab);
        }
        else
        {
            pickup.IsForSale = true;
        }

        TMP_Text label = CreatePriceLabel(item.transform, price);

        // AddComponent su un oggetto disattivato non fa partire Awake: Setup
        // riesce a scrivere prezzo ed etichetta prima che ShopItem spenga il
        // PickupController, cosa che farà quando la stanza si accende.
        ShopItem shopItem = item.GetComponent<ShopItem>();
        if (shopItem == null) shopItem = item.AddComponent<ShopItem>();

        shopItem.Setup(price, label);
    }

    // Il cartellino è figlio della merce: così la segue ovunque il template la
    // metta, e sparisce con lei se il pickup si distrugge.
    private TMP_Text CreatePriceLabel(Transform item, int price)
    {
        if (priceLabelPrefab == null) return null;

        // false: senza, Unity conserva la posizione nel mondo del prefab e
        // rimaneggia scala e posizione locali per ottenerla, proprio quelle
        // due che qui vogliamo scrivere noi
        GameObject labelObject = Instantiate(priceLabelPrefab, item, false);
        labelObject.transform.localPosition = PriceLabelPosition;
        labelObject.transform.localScale = CompensateScale(priceLabelPrefab.transform.localScale, item.localScale);

        // true: la merce nasce dentro alla stanza disattivata, quindi anche il
        // cartellino è spento e senza questo non lo troveremmo
        TMP_Text label = labelObject.GetComponentInChildren<TMP_Text>(true);

        if (label == null)
        {
            Debug.LogWarning($"Il cartellino '{priceLabelPrefab.name}' non ha nessun TextMeshProUGUI: " +
                             "il prezzo non verrà mostrato.", priceLabelPrefab);
            return null;
        }

        label.text = price.ToString();
        return label;
    }

    // I prefab dei pickup hanno scale diverse fra loro (da 0.3 a 0.6) e un
    // figlio se le eredita: lo stesso cartellino apparirebbe grande il doppio
    // sopra un oggetto e la metà sopra un altro. Dividendo per la scala del
    // genitore il prodotto delle due torna a essere quella voluta dal prefab,
    // e tutti i prezzi si leggono della stessa dimensione.
    private static Vector3 CompensateScale(Vector3 labelScale, Vector3 parentScale)
    {
        return new Vector3(
            SafeDivide(labelScale.x, parentScale.x),
            SafeDivide(labelScale.y, parentScale.y),
            SafeDivide(labelScale.z, parentScale.z));
    }

    // Una scala a zero è un errore di configurazione del pickup, ma dividerci
    // darebbe infinito e il cartellino sparirebbe portandosi dietro un warning
    // di Unity: meglio lasciarlo com'è.
    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
    }

    private static GameObject RandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0) return null;

        // Un buco nell'array configurato non deve lasciare lo slot vuoto per
        // caso: si scarta e si ripesca fra quelli buoni
        List<GameObject> valid = new List<GameObject>();

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null) valid.Add(prefab);
        }

        if (valid.Count == 0) return null;

        return valid[Random.Range(0, valid.Count)];
    }

    // Fisher-Yates su una copia: l'array configurato nell'Inspector non si tocca
    private static List<GameObject> Shuffled(GameObject[] prefabs)
    {
        List<GameObject> items = new List<GameObject>();

        if (prefabs == null) return items;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null) items.Add(prefab);
        }

        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            GameObject swap = items[i];
            items[i] = items[j];
            items[j] = swap;
        }

        return items;
    }
}
