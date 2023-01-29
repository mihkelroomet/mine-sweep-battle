using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SFXClipGroup")]
public class SFXClipGroup : ScriptableObject
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

    // For moving objects
    public void Play(Transform parent)
    {
        Play(parent, parent.position);
    }

    // For static objects
    public void Play(Vector3 position)
    {
        if (SFXSourcePool.Instance == null) return;
        
        Play(SFXSourcePool.Instance.transform, position);
    }

    public void Play(Transform parent, Vector3 position)
    {
        if (SFXSourcePool.Instance == null) return;
        
        // Don't play sounds if their source is too far
        if (!PlayerController.Instance || Vector3.Distance(PlayerController.Instance.transform.position, position) < MaxAudibleDistance)
        {
            Play(SFXSourcePool.Instance.GetSource(), parent, position);
        }
    }

    public void Play(AudioSource source, Transform parent, Vector3 position)
    {
        if (timestamp > Time.time) return;
        if (Clips.Count <= 0) return;

        timestamp = Time.time + Cooldown;

        source.volume = Random.Range(VolumeMin, VolumeMax) * Events.GetSFXVolume() * 0.1f;
        source.pitch = Random.Range(PitchMin, PitchMax);
        source.clip = Clips[Random.Range(0, Clips.Count)];
        source.transform.parent = parent; // Necessary for moving objects
        source.transform.position = position;

        source.enabled = true;
        source.Play();
    }
}
