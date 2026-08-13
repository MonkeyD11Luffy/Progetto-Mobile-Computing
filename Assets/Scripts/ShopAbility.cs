using UnityEngine;

// Da mettere sull'oggetto in vendita al posto del PickupController quando il
// negozio vende un'abilità invece di un potenziamento. Fa lo stesso mestiere:
// sa cosa consegnare al player, e ShopItem glielo chiede a pagamento riuscito
// senza sapere di cosa si tratti.
public class ShopAbility : MonoBehaviour
{
    [SerializeField] private PlayerAbilities.AbilityType ability;

    public PlayerAbilities.AbilityType Ability => ability;

    // Chiamato da ShopRoom sulla merce creata a runtime, come Setup in
    // ShopItem: scrive solo un campo, e gira prima di Awake perché la stanza
    // è ancora disattivata.
    public void Setup(PlayerAbilities.AbilityType type)
    {
        ability = type;
    }

    public void Grant(PlayerController player)
    {
        if (player == null) return;

        // PlayerAbilities sta sullo stesso GameObject di PlayerController, ed è
        // da lì che ShopItem è arrivato fin qui
        PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();

        if (abilities == null)
        {
            Debug.LogWarning("Il player non ha un PlayerAbilities: l'abilità comprata andrebbe persa.", this);
            return;
        }

        abilities.Unlock(ability);

        // Stesso feedback dei potenziamenti: senza, l'acquisto più caro del
        // negozio sarebbe anche l'unico a non dire niente
        player.ShowUpgradePopup("+" + PlayerAbilities.DisplayName(ability));
    }
}
