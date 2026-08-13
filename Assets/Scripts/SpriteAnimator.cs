using UnityEngine;

// Animazione a sprite minimale: scorre un array di fotogrammi su un
// SpriteRenderer. Serve agli effetti creati a runtime (onda d'urto, scudo,
// esplosioni), che nascono e muoiono nel giro di un secondo e non giustificano
// un Animator con la sua macchina a stati.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 10f;
    [SerializeField] private bool loop = true;
    // Ha effetto solo con loop spento: un'animazione ciclica non finisce mai
    [SerializeField] private bool destroyOnEnd = false;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float frameTimer;
    private bool finished;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Il primo fotogramma subito: senza, per un frame si vedrebbe lo sprite
        // lasciato nel prefab, che può essere qualsiasi cosa
        ShowFrame(0);
    }

    private void Update()
    {
        if (finished) return;
        if (frames == null || frames.Length == 0) return;

        // Un frameRate a zero o negativo fermerebbe l'animazione dividendo per
        // zero: si resta sul primo fotogramma
        if (frameRate <= 0f) return;

        frameTimer += Time.deltaTime;

        float frameDuration = 1f / frameRate;

        // while e non if: con frameRate alti, o con un frame lungo dopo un calo
        // di prestazioni, ne scade più d'uno nello stesso Update e con l'if
        // l'animazione andrebbe a rilento invece di recuperare
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            Advance();

            // Advance può aver distrutto l'oggetto: continuare il ciclo
            // lavorerebbe su un componente già rimosso
            if (finished) return;
        }
    }

    private void Advance()
    {
        int next = currentFrame + 1;

        if (next < frames.Length)
        {
            ShowFrame(next);
            return;
        }

        if (loop)
        {
            ShowFrame(0);
            return;
        }

        // Finita e non ciclica: currentFrame resta dov'è, quindi l'ultimo
        // fotogramma è quello che rimane a schermo
        finished = true;

        if (destroyOnEnd) Destroy(gameObject);
    }

    private void ShowFrame(int index)
    {
        if (frames == null || index < 0 || index >= frames.Length) return;

        currentFrame = index;

        if (spriteRenderer != null) spriteRenderer.sprite = frames[index];
    }
}
