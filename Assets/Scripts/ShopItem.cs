using TMPro;
using UnityEngine;

// Da mettere sull'oggetto in vendita, insieme al suo PickupController: finché
// non è pagato il pickup è marcato come merce (IsForSale) e camminarci sopra non
// fa niente. Pagando è questo script a chiamare ApplyEffect sul pickup, che sa
// cosa fare: il negozio non deve sapere cosa vende.
[RequireComponent(typeof(PickupController))]
public class ShopItem : MonoBehaviour
{
    [SerializeField] private int price = 5;
    // Il cartellino sopra l'oggetto. TMP_Text è la base comune a
    // TextMeshProUGUI (su Canvas) e TextMeshPro (nel mondo): vanno bene
    // entrambi, il negozio non ha motivo di preferirne uno.
    [SerializeField] private TMP_Text priceLabel;

    private PickupController pickup;
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

        // L'effetto lo applica il pickup, l'unico a sapere cosa fa questo
        // oggetto. Chiamarlo direttamente evita di dover far rigenerare il
        // contatto: il player è già dentro al trigger, e un OnTriggerEnter2D
        // non arriverebbe finché non esce e rientra.
        if (pickup != null) pickup.ApplyEffect(player);

        // Il cartellino è figlio della merce e se ne andrebbe con lei, ma uno
        // ShopItem configurato a mano potrebbe averlo appeso altrove
        if (priceLabel != null) Destroy(priceLabel.gameObject);

        Destroy(gameObject);
    }
}
