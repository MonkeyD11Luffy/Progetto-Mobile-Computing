using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sorgenti")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Spari")]
    [SerializeField] private AudioClip shootSingleClip;
    [SerializeField] private AudioClip shootSpreadClip;
    [SerializeField] private AudioClip shootPierceClip;
    [SerializeField] private AudioClip meleeClip;

    [Header("Effetti")]
    [SerializeField] private AudioClip playerHurtClip;
    [SerializeField] private AudioClip enemyDeathClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip pickupClip;

    [Header("Variazione")]
    [SerializeField] private float pitchVariation = 0.1f;

    // Volume di regime della musica: è quello impostato sulla sorgente
    // nell'Inspector, letto una volta sola perché le dissolvenze lo alterano
    private float musicVolume;
    // Traccia richiesta per ultima, non necessariamente già in riproduzione:
    // durante una dissolvenza musicSource.clip è ancora quella precedente
    private AudioClip currentMusic;
    private Coroutine musicFade;
    // Punto in cui ogni traccia è stata interrotta: tornando in una stanza già
    // visitata la musica riprende da lì invece di ricominciare da capo
    private readonly Dictionary<AudioClip, float> musicPositions = new Dictionary<AudioClip, float>();

    private void Awake()
    {
        Instance = this;

        if (musicSource != null)
        {
            musicVolume = musicSource.volume;
            currentMusic = musicSource.isPlaying ? musicSource.clip : null;
        }
    }

    private void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;

        // Piccola variazione di pitch: evita che il suono ripetuto diventi fastidioso
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayShootSingle() => PlaySfx(shootSingleClip, 0.5f);
    public void PlayShootSpread() => PlaySfx(shootSpreadClip, 0.5f);
    public void PlayShootPierce() => PlaySfx(shootPierceClip, 0.5f);
    public void PlayMelee() => PlaySfx(meleeClip, 0.6f);

    public void PlayPlayerHurt() => PlaySfx(playerHurtClip);
    public void PlayEnemyDeath() => PlaySfx(enemyDeathClip, 0.7f);
    public void PlayExplosion() => PlaySfx(explosionClip);
    public void PlayPickup() => PlaySfx(pickupClip);

    // Cambia traccia con una dissolvenza. Con clip a null la musica sfuma
    // fino a fermarsi. Chiedere la traccia già in riproduzione non fa nulla:
    // passando fra due stanze con la stessa musica non deve ripartire da capo.
    public void PlayMusic(AudioClip clip, float fadeDuration = 0.5f)
    {
        if (musicSource == null || clip == currentMusic) return;

        SaveMusicPosition();
        currentMusic = clip;

        if (musicFade != null) StopCoroutine(musicFade);
        musicFade = StartCoroutine(FadeToMusic(clip, fadeDuration));
    }

    private IEnumerator FadeToMusic(AudioClip clip, float duration)
    {
        // Si parte dal volume attuale e non da quello di regime: una dissolvenza
        // interrotta a metà da un nuovo cambio stanza deve continuare da lì
        if (musicSource.isPlaying) yield return FadeMusicVolume(musicSource.volume, 0f, duration);

        if (clip == null)
        {
            musicSource.Stop();
            musicSource.volume = musicVolume;
            musicFade = null;
            yield break;
        }

        musicSource.clip = clip;
        // time va impostato a clip già assegnata e sorgente ancora ferma:
        // prima dell'assegnazione varrebbe per la traccia sbagliata, dopo
        // Play() si sentirebbe comunque un istante dall'inizio
        musicSource.time = ResumePositionFor(clip);
        musicSource.volume = 0f;
        musicSource.Play();

        yield return FadeMusicVolume(0f, musicVolume, duration);

        musicFade = null;
    }

    // La posizione va letta subito e non alla fine della dissolvenza in uscita:
    // musicSource.time vale solo finché la sorgente sta ancora suonando la
    // traccia uscente, e dopo il cambio di clip sarebbe già quella nuova
    private void SaveMusicPosition()
    {
        if (musicSource.clip == null || !musicSource.isPlaying) return;

        musicPositions[musicSource.clip] = musicSource.time;
    }

    private float ResumePositionFor(AudioClip clip)
    {
        if (!musicPositions.TryGetValue(clip, out float position)) return 0f;

        // Se la clip è stata sostituita con una più corta la posizione salvata
        // può cadere oltre la fine: impostarla lì farebbe eccezione
        return position < clip.length ? position : 0f;
    }

    // Tempo non scalato: la musica sfuma anche in pausa e a fine partita,
    // quando Time.timeScale è zero
    private IEnumerator FadeMusicVolume(float from, float to, float duration)
    {
        if (duration > 0f)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
        }

        musicSource.volume = to;
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        if (musicFade != null)
        {
            StopCoroutine(musicFade);
            musicFade = null;
        }

        currentMusic = null;
        musicSource.Stop();
        musicSource.volume = musicVolume;
    }
}