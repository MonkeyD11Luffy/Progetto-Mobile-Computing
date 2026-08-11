using UnityEngine;

// Da mettere sulla radice di una stanza che vuole una propria musica.
// Le due tracce corrispondono ai due stati della stanza: con nemici vivi
// e liberata. Lasciare una clip vuota significa "silenzio in quello stato".
public class RoomMusic : MonoBehaviour
{
    [SerializeField] private AudioClip combatClip;
    [SerializeField] private AudioClip clearedClip;

    public AudioClip CombatClip => combatClip;
    public AudioClip ClearedClip => clearedClip;

    public AudioClip ClipFor(bool cleared) => cleared ? clearedClip : combatClip;
}
