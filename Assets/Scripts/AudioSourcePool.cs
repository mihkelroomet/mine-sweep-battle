using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    public static AudioSourcePool Instance;

    public AudioSource AudioSourcePrefab;

    private List<AudioSource> _audioSources;

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
}
