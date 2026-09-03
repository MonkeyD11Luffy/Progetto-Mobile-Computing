using UnityEngine;

// Va sul prefab della porta segreta, con un Collider2D marcato Is Trigger.
// Sposta l'ingresso alla stanza segreta dal muro alla porta: il muro conserva
// il proprio collider solido, che serve a tenere il player dentro alla stanza,
// mentre il passaggio avviene attraversando il varco.
[RequireComponent(typeof(Collider2D))]
public class SecretDoorTrigger : MonoBehaviour
{
    // Padre logico, non gerarchico: la porta è figlia della stanza, non del
    // muro, quindi il riferimento non si può risalire con GetComponentInParent
    // e lo passa il SecretWall stesso via SetWall.
    private SecretWall wall;

    private void Awake()
    {
        // Ripiego per una porta annidata sotto il muro: in quel caso il
        // riferimento si trova da solo e non serve nessuna assegnazione
        if (wall == null) wall = GetComponentInParent<SecretWall>();
    }

    public void SetWall(SecretWall secretWall)
    {
        wall = secretWall;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryEnter(other);
    }

    // Come sul muro: se GoToRoom viene ignorato per doorIgnoreTimer, al frame
    // successivo il player è ancora dentro al varco e il passaggio riparte da
    // solo, invece di essere perso per sempre.
    private void OnTriggerStay2D(Collider2D other)
    {
        TryEnter(other);
    }

    private void TryEnter(Collider2D other)
    {
        if (wall == null) return;

        // Il player si riconosce dal componente e non dal tag, così vale anche
        // quando a entrare nel varco è un suo collider figlio (la hurtbox)
        if (other.GetComponentInParent<PlayerController>() == null) return;

        // Le guardie (isRevealed, stanza liberata) restano dentro al SecretWall
        wall.TryEnterSecretRoom();
    }
}
