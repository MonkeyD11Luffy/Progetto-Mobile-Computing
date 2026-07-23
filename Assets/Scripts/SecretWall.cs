using UnityEngine;

public class SecretWall : MonoBehaviour
{
    public void Destroy()
    {
        // Rende il muro attraversabile per sempre, rivelando il passaggio
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Cambia colore per segnalare visivamente che è stato distrutto
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.3f);
        }
    }
}
