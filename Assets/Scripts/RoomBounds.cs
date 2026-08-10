using UnityEngine;

// Da mettere sulla radice di una stanza per dichiarare le sue semi-dimensioni
// interne, cioè lo spazio calpestabile misurato dal centro della stanza.
// Chi deve restare dentro ai muri (per ora il Teleporter) le legge da qui
// invece di avere i confini fissati nel proprio Inspector.
public class RoomBounds : MonoBehaviour
{
    [SerializeField] private float halfWidth = 4f;
    [SerializeField] private float halfHeight = 4f;

    public float HalfWidth => halfWidth;
    public float HalfHeight => halfHeight;
}
