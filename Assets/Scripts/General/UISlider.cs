using UnityEngine;
using UnityEngine.UI;

public class UISlider : MonoBehaviour
{
    // Components
    private Slider _slider;

    // Audio
    public SFXClipGroup ChangeValueAudio;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        _slider.onValueChanged.AddListener(ChangeValue); // Can't be done in awake because on Lobby awake values are being changed to match room properties
    }

    private void ChangeValue(float value)
    {
        ChangeValueAudio.Play(SFXSourcePool.Instance.transform);
    }
}
