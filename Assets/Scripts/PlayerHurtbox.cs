using UnityEngine;

// Va su un figlio del Player con un Collider2D marcato Is Trigger.
// Sposta la rilevazione del contatto con i nemici fuori dal collider della
// radice: l'area che incassa il danno puo' cosi' essere piu' piccola (o di
// forma diversa) rispetto a quella che il player usa per muri e ostacoli.
// Il danno vero e proprio resta a carico del PlayerController, che tiene
// invulnerabilita', lampeggio e bonus dell'armatura.
[RequireComponent(typeof(Collider2D))]
public class PlayerHurtbox : MonoBehaviour
{
    [Header("Danno da contatto")]
    // Danno inflitto da un nemico che tocca la hurtbox. Il valore per singolo
    // nemico vive in EnemyBase.contactDamage, che non e' leggibile da qui:
    // il default resta allineato a quello (1) e si tara nell'Inspector.
    [SerializeField] private int contactDamage = 1;

    private PlayerController player;

    private void Awake()
    {
        // Risale al PlayerController: la hurtbox e' un figlio, non la radice
        player = GetComponentInParent<PlayerController>();

        if (player == null)
        {
            Debug.LogWarning("PlayerHurtbox: nessun PlayerController nei genitori", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    // Serve anche Stay: restando fermi addosso a un nemico non arriva nessun
    // nuovo Enter, e quando l'invulnerabilita' scade il player smetterebbe di
    // prendere danno pur essendo ancora a contatto.
    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    // Stesso criterio di riconoscimento usato dal PlayerController: tag "Enemy"
    private void TryDamagePlayer(Collider2D other)
    {
        if (player == null || contactDamage <= 0) return;
        if (!other.CompareTag("Enemy")) return;

        player.TakeContactDamage(contactDamage);
    }
}
