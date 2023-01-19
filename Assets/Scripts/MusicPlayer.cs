using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    [SerializeField] private AudioSource _audioSource;
    public int DefaultMusicVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Events.OnGetMusicVolume += GetMusicVolume;
        Events.OnSetMusicVolume += SetMusicVolume;
    }

    private void Start()
    {
        Events.SetMusicVolume(DefaultMusicVolume);
    }

    private int GetMusicVolume()
    {
        return (int) Mathf.Round(_audioSource.volume * 10);
    }

    private void SetMusicVolume(int volume)
    {
        _audioSource.volume = volume * 0.1f;
    }

    public void PlayMusic(AudioClip music)
    {
        _audioSource.clip = music;
        _audioSource.Play();
    }

    private void OnDestroy()
    {
        Events.OnGetMusicVolume -= GetMusicVolume;
        Events.OnSetMusicVolume -= SetMusicVolume;
    }
}
