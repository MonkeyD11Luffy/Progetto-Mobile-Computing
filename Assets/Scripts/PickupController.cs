using UnityEngine;

public class PickupController : MonoBehaviour
{
    public enum PickupType { Heal, SpeedBoost, Bomb }

    [Header("Tipo di potenziamento")]
    [SerializeField] private PickupType pickupType;

    [Header("Valori Heal")]
    [SerializeField] private int healAmount = 2;

    [Header("Valori Speed Boost")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float boostDuration = 5f;

    [Header("Valori Bomb")]
    [SerializeField] private int bombAmount = 1;

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
        }

        Destroy(gameObject);
    }
}
