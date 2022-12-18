using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    public static AudioSourcePool Instance;

    public AudioSource AudioSourcePrefab;

    private List<AudioSource> audioSources;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        audioSources = new List<AudioSource>();
        DontDestroyOnLoad(this.gameObject);
    }

    public AudioSource GetSource()
    {
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        AudioSource newSource = GameObject.Instantiate(AudioSourcePrefab, transform);
        audioSources.Add(newSource);

        return newSource;
    }

    public void ClearAudioSources()
    {
        audioSources = new List<AudioSource>();
    }
}
