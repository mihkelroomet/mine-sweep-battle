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
        _slider.onValueChanged.AddListener(ChangeValue);
    }

    private void ChangeValue(float value)
    {
        ChangeValueAudio.Play(SFXSourcePool.Instance.transform);
    }
}
