using UnityEngine;

public class PickupController : MonoBehaviour
{
    public enum PickupType { Heal, SpeedBoost, Bomb, MaxHealthUp, DamageUp, SpeedUp, FireRateUp, HealthRegenUp, ArmorUp, MeleeArcUp, ProjectileBounceUp, Credit }

    [Header("Tipo di potenziamento")]
    [SerializeField] private PickupType pickupType;

    [Header("Valori Heal")]
    [SerializeField] private int healAmount = 2;

    [Header("Valori Speed Boost")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float boostDuration = 5f;

    [Header("Valori Bomb")]
    [SerializeField] private int bombAmount = 1;

    [Header("Valori Credit")]
    [SerializeField] private int creditAmount = 1;

    [Header("Valori Potenziamenti Permanenti")]
    [SerializeField] private int maxHealthIncrease = 2;
    [SerializeField] private int damageIncrease = 1;
    [SerializeField] private float speedIncrease = 1f;
    [SerializeField] private float fireRateIncrease = 0.05f;
    [SerializeField] private float healthRegenChanceIncrease = 0.15f;
    [SerializeField] private int armorPickupIncrease = 1;
    [SerializeField] private float meleeArcIncrease = 0.15f;
    [SerializeField] private int projectileBounceIncrease = 1;

    // Merce di negozio: si passa dalla cassa e non dal trigger, è ShopItem a
    // chiamare ApplyEffect a pagamento riuscito. Proprietà e non campo
    // serializzato perché è un dato di collegamento scritto a runtime da
    // ShopRoom, come targetRoom in DoorTrigger: nei prefab non deve comparire,
    // e su un pickup normale non deve poter essere spuntata per sbaglio.
    public bool IsForSale { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsForSale) return;

        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;

        ApplyEffect(playerController);

        // In una stanza tesoro si sceglie un potenziamento solo: prendendone uno
        // gli altri spariscono. InParent perché il pickup è figlio della stanza.
        TreasureRoom treasureRoom = GetComponentInParent<TreasureRoom>();
        if (treasureRoom != null) treasureRoom.OnUpgradeTaken(gameObject);

        Destroy(gameObject);
    }

    // Applica l'effetto e basta: non distrugge l'oggetto e non tocca la stanza
    // tesoro, così può chiamarla anche ShopItem dopo il pagamento. È l'unico
    // punto in cui l'effetto viene applicato, da qualunque strada si arrivi.
    public void ApplyEffect(PlayerController player)
    {
        if (player == null) return;

        switch (pickupType)
        {
            case PickupType.Heal:
                player.Heal(healAmount);
                break;

            case PickupType.SpeedBoost:
                player.ApplySpeedBoost(speedMultiplier, boostDuration);
                break;

            case PickupType.Bomb:
                player.AddBomb(bombAmount);
                break;

            case PickupType.MaxHealthUp:
                player.IncreaseMaxHealth(maxHealthIncrease);
                break;

            case PickupType.DamageUp:
                player.IncreaseDamage(damageIncrease);
                break;

            case PickupType.SpeedUp:
                player.IncreaseSpeed(speedIncrease);
                break;


            case PickupType.FireRateUp:
    player.DecreaseFireCooldown(fireRateIncrease);
    break;

            case PickupType.HealthRegenUp:
                player.IncreaseHealthRegenChance(healthRegenChanceIncrease);
                break;

            case PickupType.ArmorUp:
                player.AddArmorPickup(armorPickupIncrease);
                break;

            case PickupType.MeleeArcUp:
                player.WidenMeleeArc(meleeArcIncrease);
                break;

            case PickupType.ProjectileBounceUp:
                player.IncreaseProjectileBounces(projectileBounceIncrease);
                break;

            case PickupType.Credit:
                player.AddCredits(creditAmount);
                break;
        }

        // Niente popup per i crediti: ne cade una manciata a ogni nemico
        // ucciso, il messaggio resterebbe sempre acceso
        if (pickupType != PickupType.Credit)
        {
            player.ShowUpgradePopup("+" + GetDisplayName());
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
    }

    private string GetDisplayName()
    {
        return pickupType switch
        {
            PickupType.Heal => "Vita",
            PickupType.SpeedBoost => "Scatto",
            PickupType.Bomb => "Bomba",
            PickupType.MaxHealthUp => "Vita Massima",
            PickupType.DamageUp => "Danno",
            PickupType.SpeedUp => "Velocità",
            PickupType.FireRateUp => "Cadenza di Fuoco",
            PickupType.HealthRegenUp => "Rigenerazione",
            PickupType.ArmorUp => "Armatura",
            PickupType.MeleeArcUp => "Arco Corpo a Corpo",
            PickupType.ProjectileBounceUp => "Rimbalzo Proiettili",
            _ => pickupType.ToString()
        };
    }
}
