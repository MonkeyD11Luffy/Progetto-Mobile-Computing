using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float maxScale = 3f;

    private SpriteRenderer sr;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) startColor = sr.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Si espande e svanisce insieme
        transform.localScale = Vector3.one * Mathf.Lerp(0.5f, maxScale, t);

        if (sr != null)
        {
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
        }
    }
}
