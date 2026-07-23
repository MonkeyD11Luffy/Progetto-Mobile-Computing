using UnityEngine;

public class PickupController : MonoBehaviour
{
    public enum PickupType { Heal, SpeedBoost, Bomb, MaxHealthUp, DamageUp, SpeedUp, FireRateUp }

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
        }

        Destroy(gameObject);
    }
}
