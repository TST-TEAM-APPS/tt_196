using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;

        [HideInInspector] public AudioSource source;
    }

    [Header("Audio Settings")]
    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();
    [SerializeField] private List<AudioClip> backgroundMusic = new List<AudioClip>();
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float pitchVariation = 0.1f;

    private AudioSource musicSource;
    private int currentMusicIndex = 0;

    private void Awake()
    {
        SetupAudioSources();
        LoadAudioSettings();
    }

    private void SetupAudioSources()
    {
        // Create music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        // Create sources for each sound effect
        foreach (SoundEffect sound in soundEffects)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * sfxVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }

        Debug.Log("Audio sources set up");
    }

    public void PlaySound(string name)
    {
        SoundEffect sound = soundEffects.Find(s => s.name == name);

        if (sound != null)
        {
            // Add slight pitch variation for more natural sound
            sound.source.pitch = sound.pitch * Random.Range(1 - pitchVariation, 1 + pitchVariation);
            sound.source.Play();

            Debug.Log($"Playing sound: {name}");
        }
        else
        {
            Debug.LogWarning($"Sound effect not found: {name}");
        }
    }

    public void StopSound(string name)
    {
        SoundEffect sound = soundEffects.Find(s => s.name == name);

        if (sound != null)
        {
            sound.source.Stop();
        }
    }

    public void PlayRandomSound(string namePrefix)
    {
        List<SoundEffect> matchingSounds = soundEffects.FindAll(s => s.name.StartsWith(namePrefix));

        if (matchingSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, matchingSounds.Count);

            // Add slight pitch variation
            matchingSounds[randomIndex].source.pitch = matchingSounds[randomIndex].pitch *
                Random.Range(1 - pitchVariation, 1 + pitchVariation);

            matchingSounds[randomIndex].source.Play();
        }
    }

    public void PlayMenuMusic()
    {
        if (backgroundMusic.Count > 0)
        {
            PlayMusic(0);
        }
    }

    public void PlayGameMusic()
    {
        if (backgroundMusic.Count > 1)
        {
            PlayMusic(1);
        }
        else if (backgroundMusic.Count > 0)
        {
            PlayMusic(0);
        }
    }

    private void PlayMusic(int index)
    {
        if (index < 0 || index >= backgroundMusic.Count)
        {
            Debug.LogWarning($"Music index {index} out of range");
            return;
        }

        if (currentMusicIndex == index && musicSource.isPlaying)
        {
            return;
        }

        currentMusicIndex = index;

        if (musicSource.isPlaying)
        {
            // Crossfade music
            float originalVolume = musicSource.volume;

            DOTween.To(() => musicSource.volume, x => musicSource.volume = x, 0, fadeTime)
                .OnComplete(() => {
                    musicSource.Stop();
                    musicSource.clip = backgroundMusic[index];
                    musicSource.pitch = 1f;
                    musicSource.Play();
                    DOTween.To(() => musicSource.volume, x => musicSource.volume = x, originalVolume, fadeTime);
                });
        }
        else
        {
            // Start playing music
            musicSource.clip = backgroundMusic[index];
            musicSource.volume = 0;
            musicSource.pitch = 1f;
            musicSource.Play();
            DOTween.To(() => musicSource.volume, x => musicSource.volume = x, musicVolume, fadeTime);
        }

        Debug.Log($"Playing music track {index}");
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        foreach (SoundEffect sound in soundEffects)
        {
            sound.source.volume = sound.volume * sfxVolume;
        }

        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
        }
    }

    public void MuteAllAudio(bool mute)
    {
        AudioListener.volume = mute ? 0 : 1;
    }

    private void OnDestroy()
    {
        DOTween.Kill(musicSource);
    }
}