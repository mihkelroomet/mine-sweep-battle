using System.Collections.Generic;
using UnityEngine;

public class SFXSourcePool : MonoBehaviour
{
    public static SFXSourcePool Instance;

    public AudioSource AudioSourcePrefab;

    private List<AudioSource> _audioSources;

    // Volume
    private int _SFXVolume;
    public int DefaultSFXVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        _audioSources = new List<AudioSource>();
        DontDestroyOnLoad(this.gameObject);

        Events.OnGetSFXVolume += GetSFXVolume;
        Events.OnSetSFXVolume += SetSFXVolume;
    }

    private void Start()
    {
        Events.SetSFXVolume(DefaultSFXVolume);
    }

    private int GetSFXVolume()
    {
        return _SFXVolume;
    }

    private void SetSFXVolume(int value)
    {
        _SFXVolume = value;
    }

    public AudioSource GetSource()
    {
        foreach (AudioSource source in _audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        AudioSource newSource = GameObject.Instantiate(AudioSourcePrefab, transform);
        _audioSources.Add(newSource);

        return newSource;
    }

    public void ClearAudioSources()
    {
        _audioSources = new List<AudioSource>();
    }

    public void RemoveAudioSource(AudioSource audioSource)
    {
        if (_audioSources.Contains(audioSource)) _audioSources.Remove(audioSource);
    }

    private void OnDestroy()
    {
        Events.OnGetSFXVolume -= GetSFXVolume;
        Events.OnSetSFXVolume -= SetSFXVolume;
    }
}
