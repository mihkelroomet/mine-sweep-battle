using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AudioClipGroup")]
public class AudioClipGroup : ScriptableObject
{
    [Range(0, 2)]
    public float VolumeMin = 1;
    [Range(0, 2)]
    public float VolumeMax = 1;
    [Range(0, 2)]
    public float PitchMin = 1;
    [Range(0, 2)]
    public float PitchMax = 1;
    public float Cooldown = 0.1f;
    public float MaxAudibleDistance = 7f;

    public List<AudioClip> Clips;

    private float timestamp;

    private void OnEnable()
    {
        timestamp = 0;
    }

    public void Play()
    {
        Play(PlayerController.Instance.transform);
    }

    public void Play(Transform parent)
    {
        if (AudioSourcePool.Instance == null) return;
        
        // Don't play sounds if their source is too far
        if (Vector3.Distance(PlayerController.Instance.transform.position, parent.transform.position) < MaxAudibleDistance) Play(AudioSourcePool.Instance.GetSource(), parent);
    }

    public void Play(AudioSource source, Transform parent)
    {
        if (timestamp > Time.time) return;
        if (Clips.Count <= 0) return;

        timestamp = Time.time + Cooldown;

        source.volume = Random.Range(VolumeMin, VolumeMax);
        source.pitch = Random.Range(PitchMin, PitchMax);
        source.clip = Clips[Random.Range(0, Clips.Count)];
        source.transform.parent = parent; // Necessary for moving objects
        source.transform.position = parent.transform.position;

        source.Play();
    }

}
