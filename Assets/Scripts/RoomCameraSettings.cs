using UnityEngine;

// Da mettere sul GameObject di una stanza che vuole una risoluzione di
// riferimento diversa da quella di default della PixelPerfectCamera.
public class RoomCameraSettings : MonoBehaviour
{
    [SerializeField] private Vector2Int referenceResolution = new Vector2Int(240, 135);

    public Vector2Int ReferenceResolution => referenceResolution;
}
