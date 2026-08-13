using TMPro;
using UnityEngine;

// Da mettere sull'oggetto in vendita, insieme a chi sa cosa quell'oggetto
// consegna: un PickupController (consumabili e potenziamenti) oppure uno
// ShopAbility (abilità). Finché non è pagato il pickup è marcato come merce
// (IsForSale) e camminarci sopra non fa niente; pagando è questo script a
// chiedere la consegna. Il negozio non deve sapere cosa vende.
//
// Niente RequireComponent sul PickupController: la merce può essere un'abilità,
// e in quel caso Unity ne aggiungerebbe uno di troppo, con un effetto suo.
public class ShopItem : MonoBehaviour
{
    [SerializeField] private int price = 5;
    // Il cartellino sopra l'oggetto. TMP_Text è la base comune a
    // TextMeshProUGUI (su Canvas) e TextMeshPro (nel mondo): vanno bene
    // entrambi, il negozio non ha motivo di preferirne uno.
    [SerializeField] private TMP_Text priceLabel;

    private PickupController pickup;
    private ShopAbility ability;
    private bool purchased;

    public int Price => price;

    // Chiamato da ShopRoom sulla merce creata a runtime. Scrive solo campi:
    // gira prima di Awake, perché la stanza è ancora disattivata.
    public void Setup(int itemPrice, TMP_Text label)
    {
        price = itemPrice;
        priceLabel = label;

        if (priceLabel != null) priceLabel.text = price.ToString();
    }

    private void Awake()
    {
        pickup = GetComponent<PickupController>();
        ability = GetComponent<ShopAbility>();

        // Ridondante per la merce creata da ShopRoom, che la marca già alla
        // nascita: serve a chi mette questo script a mano su un pickup dentro
        // un template. Marcare non è spegnere, quindi l'ordine dei componenti
        // non conta: qualunque cosa giri prima, l'effetto passa comunque da qui.
        if (pickup != null) pickup.IsForSale = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // purchased copre il caso di due contatti nello stesso frame: Destroy
        // agisce a fine frame, quindi l'oggetto è ancora lì e pagabile due volte
        if (purchased || !other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (!player.SpendCredits(price))
        {
            // Niente di distruttivo: la merce resta sul bancone e si può
            // tornare più tardi con i crediti in tasca
            player.ShowUpgradePopup("Crediti insufficienti");
            return;
        }

        purchased = true;

        Deliver(player);

        // Il cartellino è figlio della merce e se ne andrebbe con lei, ma uno
        // ShopItem configurato a mano potrebbe averlo appeso altrove
        if (priceLabel != null) Destroy(priceLabel.gameObject);

        Destroy(gameObject);
    }

    // La consegna la fa chi sa cosa c'è dentro l'oggetto. Chiamarlo direttamente
    // evita di dover far rigenerare il contatto: il player è già dentro al
    // trigger, e un OnTriggerEnter2D non arriverebbe finché non esce e rientra.
    private void Deliver(PlayerController player)
    {
        if (pickup != null)
        {
            pickup.ApplyEffect(player);
            return;
        }

        if (ability != null)
        {
            ability.Grant(player);
            return;
        }

        // I crediti sono già stati spesi: senza questo il player pagherebbe e
        // non riceverebbe niente, in silenzio
        Debug.LogWarning($"'{name}' è in vendita ma non ha né PickupController né ShopAbility: " +
                         "il player ha pagato e non ha ricevuto niente.", this);
    }
}
