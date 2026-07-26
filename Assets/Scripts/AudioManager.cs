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

    private void Awake()
    {
        Instance = this;
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

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}