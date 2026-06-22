using UnityEngine;
using System.Collections;

// Singleton audio manager. Attach to a persistent empty GameObject named "AudioManager".
// Assign all three clips in the Inspector.
// Uses two AudioSources: one for SFX (one-shot), one for BGM (looping).

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip gunshotClip;      // played on player shoot + enemy fire
    [SerializeField] private AudioClip revolverCockClip; // played when Draw button pressed
    [SerializeField] private AudioClip bgmClip;          // looping BGM, starts after cock sound

    [Header("Volume")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float bgmVolume = 0.4f;     // lower than SFX so it doesn't overpower

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // persist through scene reloads so BGM doesn't cut out on retry

        // SFX source - for one-shot sounds
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;

        // BGM source - for looping background music
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.clip = bgmClip;
    }

    // Called when Draw button is pressed - plays cock then chains into BGM
    public void PlayDrawSequence()
    {
        if (revolverCockClip != null)
        {
            sfxSource.PlayOneShot(revolverCockClip, sfxVolume);
            StartCoroutine(StartBGMAfterDelay(revolverCockClip.length));
        }
        else
        {
            StartBGM();
        }
    }

    // Called on successful player shot and when enemy fires back on a loss
    public void PlayGunshot()
    {
        if (gunshotClip != null)
            sfxSource.PlayOneShot(gunshotClip, sfxVolume);
    }

    public void StartBGM()
    {
        if (bgmClip == null || bgmSource.isPlaying) return;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    private IEnumerator StartBGMAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StartBGM();
    }
}