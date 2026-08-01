using UnityEngine;

// Utilità geometriche condivise da player e nemici.
// Non è un MonoBehaviour: non va messa su nessun GameObject.
public static class VectorUtils
{
    // Ruota una direzione di un certo numero di gradi (antiorario)
    public static Vector2 Rotate(Vector2 direction, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }

    // Direzione unitaria corrispondente a un angolo in gradi (0 = destra)
    public static Vector2 FromAngle(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    // Angolo in gradi di una direzione, pronto per Quaternion.Euler(0, 0, angle)
    public static float ToAngle(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}
