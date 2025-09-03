using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public Sound[] sounds;
    private Dictionary<string, float> sfxLastPlayTime = new();
    private Dictionary<string, Sound> soundDict = new();
    private Dictionary<string, AudioSource> loopingSFXSources = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            if (!soundDict.ContainsKey(s.name))
            {
                soundDict.Add(s.name, s);
            }
        }
    }

    public void Play(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s)) return;

        if (s.type == SoundType.Music)
        {
            musicSource.clip = s.clip;
            musicSource.loop = s.loop;
            musicSource.volume = s.volume;
            musicSource.Play();
        }
        else if (s.type == SoundType.SFX)
        {
            float currentTime = Time.time;
            float lastPlayTime = sfxLastPlayTime.ContainsKey(name) ? sfxLastPlayTime[name] : -999f;

            // Gi?i h?n phát n?u ch?a ??n cooldown
            float minInterval = 0.3f; // th?i gian cách nhau t?i thi?u gi?a các l?n phát cùng 1 clip

            if (currentTime - lastPlayTime >= minInterval)
            {
                sfxSource.PlayOneShot(s.clip, s.volume);
                sfxLastPlayTime[name] = currentTime;
            }
        }
    }

    private float lastSequenceTime = -999f;
    [SerializeField] private float minSequenceInterval = 1.5f; // kho?ng cách gi?a 2 l?n g?i

    public void PlaySequenceWithStaggeredDelay(float delayBetweenClips, params string[] names)
    {
        float currentTime = Time.time;

        if (currentTime - lastSequenceTime < minSequenceInterval)
        {
            Debug.Log("Sequence too soon, skipping");
            return;
        }

        lastSequenceTime = currentTime;
        StartCoroutine(PlayClipsWithStaggeredDelayCoroutine(delayBetweenClips, names));
    }

    private IEnumerator PlayClipsWithStaggeredDelayCoroutine(float delay, string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (soundDict.TryGetValue(name, out Sound s) && s.type == SoundType.SFX)
            {
                StartCoroutine(DelayedPlay(s.clip, s.volume, i * delay));
            }
        }

        // Không c?n ??i ?? reset flag, vì dùng Time.time ki?m tra r?i
        yield break;
    }

    private IEnumerator DelayedPlay(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayLoopingSFX(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s)) return;
        if (s.type != SoundType.SFX) return;

        // N?u ?ã có r?i thì không c?n phát l?i
        if (loopingSFXSources.ContainsKey(name)) return;

        // T?o audio source t?m riêng cho SFX này
        AudioSource newSfx = gameObject.AddComponent<AudioSource>();
        newSfx.clip = s.clip;
        newSfx.loop = true;
        newSfx.volume = s.volume;
        newSfx.Play();

        loopingSFXSources.Add(name, newSfx);
    }
    public void StopLoopingSFX(string name)
    {
        if (loopingSFXSources.TryGetValue(name, out AudioSource source))
        {
            source.Stop();
            Destroy(source); // cleanup
            loopingSFXSources.Remove(name);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
    }
}
