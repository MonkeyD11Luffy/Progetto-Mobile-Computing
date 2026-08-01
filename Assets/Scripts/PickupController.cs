using UnityEngine;

public class PickupController : MonoBehaviour
{
    public enum PickupType { Heal, SpeedBoost, Bomb, MaxHealthUp, DamageUp, SpeedUp, FireRateUp, HealthRegenUp, ArmorUp, MeleeArcUp, ProjectileBounceUp }

    [Header("Tipo di potenziamento")]
    [SerializeField] private PickupType pickupType;

    [Header("Valori Heal")]
    [SerializeField] private int healAmount = 2;

    [Header("Valori Speed Boost")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float boostDuration = 5f;

    [Header("Valori Bomb")]
    [SerializeField] private int bombAmount = 1;

    [Header("Valori Potenziamenti Permanenti")]
    [SerializeField] private int maxHealthIncrease = 2;
    [SerializeField] private int damageIncrease = 1;
    [SerializeField] private float speedIncrease = 1f;
    [SerializeField] private float fireRateIncrease = 0.05f;
    [SerializeField] private float healthRegenChanceIncrease = 0.15f;
    [SerializeField] private int contactDamageReductionIncrease = 1;
    [SerializeField] private float meleeArcIncrease = 0.15f;
    [SerializeField] private int projectileBounceIncrease = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;

        switch (pickupType)
        {
            case PickupType.Heal:
                playerController.Heal(healAmount);
                break;

            case PickupType.SpeedBoost:
                playerController.ApplySpeedBoost(speedMultiplier, boostDuration);
                break;

            case PickupType.Bomb:
                playerController.AddBomb(bombAmount);
                break;

            case PickupType.MaxHealthUp:
                playerController.IncreaseMaxHealth(maxHealthIncrease);
                break;

            case PickupType.DamageUp:
                playerController.IncreaseDamage(damageIncrease);
                break;

            case PickupType.SpeedUp:
                playerController.IncreaseSpeed(speedIncrease);
                break;


            case PickupType.FireRateUp:
    playerController.DecreaseFireCooldown(fireRateIncrease);
    break;

            case PickupType.HealthRegenUp:
                playerController.IncreaseHealthRegenChance(healthRegenChanceIncrease);
                break;

            case PickupType.ArmorUp:
                playerController.IncreaseContactDamageReduction(contactDamageReductionIncrease);
                break;

            case PickupType.MeleeArcUp:
                playerController.WidenMeleeArc(meleeArcIncrease);
                break;

            case PickupType.ProjectileBounceUp:
                playerController.IncreaseProjectileBounces(projectileBounceIncrease);
                break;
        }

        playerController.ShowUpgradePopup("+" + GetDisplayName());

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();

        Destroy(gameObject);
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
