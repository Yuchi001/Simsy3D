using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private float soundFadeOutTime = 0.15f;
    [SerializeField] private float emptySoundDetectionRate = 1f;
    [SerializeField] private List<SoundData> sounds = new();
    [SerializeField] private List<ThemeData> themes = new();

    private AudioSource _mainAudio = null;
    
    private readonly Dictionary<ESoundType, List<AudioSource>> _activeSounds = new();

    private float _timer = 0;
    

    #region Singleton

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != this && Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    #endregion

    public void PlaySoundOneShoot(ESoundType soundType)
    {
        var clip = sounds.FirstOrDefault(s => s.SoundType == soundType)?.AudioClip;
        if (clip == null) return;

        var audioSource = new GameObject($"Audio: {soundType}", typeof(AudioSource));
        var audioSourceScript = audioSource.GetComponent<AudioSource>();
        audioSourceScript.volume = 0.15f;
        audioSourceScript.loop = false;
        audioSourceScript.pitch -= Random.Range(-0.1f, 0.1f);
        audioSourceScript.PlayOneShot(clip);
            
        Destroy(audioSource, 0.5f);
    }
    
    public void PlaySound(ESoundType soundType, bool loop)
    {
        var clip = sounds.FirstOrDefault(s => s.SoundType == soundType)?.AudioClip;
        if (clip == null) return;

        var audioSource = new GameObject($"Audio: {soundType}", typeof(AudioSource));
        var audioSourceScript = audioSource.GetComponent<AudioSource>();
        audioSourceScript.volume = 0.3f;
        audioSourceScript.loop = loop;
        audioSourceScript.pitch -= Random.Range(-0.1f, 0.1f);
        audioSourceScript.clip = clip;
        audioSourceScript.Play();

        var exists = _activeSounds.ContainsKey(soundType);
        
        // Jezeli klucz istnieje w slowniku to dodajemy do listy jego dzwiekow nowo zrobiony dzwiek
        if (exists) _activeSounds[soundType].Add(audioSourceScript);
        // Jezeli nie to tworzymy nowa pare i wpisujemy od razu do listy dziwkow nowo pojawiony dzwiek
        else _activeSounds.Add(soundType, new List<AudioSource> { audioSourceScript });
    }

    public void StopPlayingSound(ESoundType soundType)
    {
        var exists = _activeSounds.ContainsKey(soundType);
        if (!exists) return;

        var soundsToDestroy = _activeSounds[soundType];
        _activeSounds.Remove(soundType);
        // Dla kazdego dzwieku przypisanego do klucza uzywamy funkcji co wyciszy i usunie dzwiek
        foreach (var sound in soundsToDestroy)
            StartCoroutine(sound.FadeOutAndDestroy(soundFadeOutTime));
    }

    public void SetTheme(EThemeType themeType)
    {
        var audioSource = _mainAudio;
        // Jezeli jeszcze theme nie byl ustawiony to tworzymy nowy theme i wpisujemy go do _mainAudio
        if (audioSource == null)
        {
            var audioSourceObj = new GameObject($"Audio: {themeType}", typeof(AudioSource));
            audioSource = audioSourceObj.GetComponent<AudioSource>();
            _mainAudio = audioSource;
        }
            
        var clip = themes.FirstOrDefault(s => s.ThemeType == themeType)?.AudioClip;
        if (clip == null) return;
        
        // Jezeli theme byl juz ustawiony to po prostu zmieniamy jego zawartosc
        audioSource.clip = clip;
        audioSource.volume = 0.3f;
        audioSource.loop = true;
        audioSource.Play();
    }

    public bool IsPlaying(ESoundType soundType)
    {
        // Jezeli klucza "soundType" nie ma w slowniku no to na pewno nie gramy teraz tego dzwieku
        if (!_activeSounds.ContainsKey(soundType)) return false;

        // Jesli klucz jest no to sprawdzamyn czy jakikolwiek komponent "AudioSource" gra jeszcze jakas muzyke
        return _activeSounds[soundType].FirstOrDefault(sound => sound.isPlaying) != default;
    }
}

[System.Serializable]
public class SoundData
{
    public AudioClip AudioClip;
    public ESoundType SoundType;
}
    
[System.Serializable]
public class ThemeData
{
    public AudioClip AudioClip;
    public EThemeType ThemeType;
}