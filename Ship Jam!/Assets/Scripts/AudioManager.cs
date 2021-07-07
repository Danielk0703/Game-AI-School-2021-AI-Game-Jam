using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("Development Only Settings")]
    public bool doNotStartAudio;

    [Header("UI References")]
    public Slider volumeSlider;

    [Header("Audio Clips")]
    public AudioClip[] backgroundMusicClips;

    public AudioClip[] backgroundSfxClips;

    public AudioClip shootClip;
    public AudioClip defeatClip;

    [Header("Audio Source References")]
    public AudioSource backgroundSfxAudioSource;

    private AudioSource backgroundMusicAudioSource;

    // Singleton of AudioManager
    private static AudioManager instance = null;

    // Singleton of AudioManager
    public static AudioManager Get()
    {
        if (instance == null)
            instance = (AudioManager) FindObjectOfType(typeof(AudioManager));

        return instance;
    }

    private void Awake()
    {
        backgroundMusicAudioSource = GetComponent<AudioSource>();
        volumeSlider.value = AudioListener.volume;
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (doNotStartAudio)
            return;
#endif

        if (backgroundMusicAudioSource != null && backgroundMusicClips.Length > 0)
            StartCoroutine(PlayRandomBackgroundMusic());

        if (backgroundSfxAudioSource != null && backgroundSfxClips.Length > 0)
            StartCoroutine(PlayRandomBackgroundSfxClip());
    }

    public void ChangeGlobalVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public void PlayShootClip()
    {
        backgroundSfxAudioSource.PlayOneShot(shootClip);
    }

    public void PlayDefeatClip()
    {
        backgroundSfxAudioSource.PlayOneShot(defeatClip);
    }

    private IEnumerator PlayRandomBackgroundMusic()
    {
        while (true)
        {
            int clipPos = Random.Range(0, backgroundMusicClips.Length);
            backgroundMusicAudioSource.clip = backgroundMusicClips[clipPos];
            backgroundMusicAudioSource.Play();

            yield return new WaitForSeconds(backgroundMusicAudioSource.clip.length);

            // If short loop music -> Replay it 3 times
            if (backgroundMusicAudioSource.clip.length <= 45)
            {
                int i = 3;
                while (i > 0)
                {
                    backgroundMusicAudioSource.clip = backgroundMusicClips[clipPos];
                    backgroundMusicAudioSource.Play();

                    yield return new WaitForSecondsRealtime(backgroundMusicAudioSource.clip.length);
                    i--;
                }
            }
        }
    }

    private IEnumerator PlayRandomBackgroundSfxClip()
    {
        while (true)
        {
            int clipPos = Random.Range(0, backgroundSfxClips.Length);
            backgroundSfxAudioSource.clip = backgroundSfxClips[clipPos];
            backgroundSfxAudioSource.Play();

            yield return new WaitForSecondsRealtime(backgroundSfxAudioSource.clip.length);
        }
    }
}