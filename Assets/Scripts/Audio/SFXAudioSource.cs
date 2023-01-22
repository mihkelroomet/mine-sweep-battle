using UnityEngine;

public class SFXAudioSource : MonoBehaviour
{
    private void OnDestroy()
    {
        SFXSourcePool.Instance.RemoveAudioSource(this.GetComponent<AudioSource>());
    }
}
