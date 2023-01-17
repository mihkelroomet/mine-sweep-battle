using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetMusicVolume(float volume)
    {
        _audioSource.volume = volume;
    }

    public void PlayMusic(AudioClip music)
    {
        _audioSource.clip = music;
        _audioSource.Play();
    }
}
