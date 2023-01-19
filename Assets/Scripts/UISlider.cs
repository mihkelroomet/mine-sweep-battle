using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISlider : MonoBehaviour
{
    // Components
    private Slider _slider;

    // Audio
    public SFXClipGroup ChooseAudio;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(ChangeValue);
    }

    private void ChangeValue(float value)
    {
        ChooseAudio.Play(SFXSourcePool.Instance.transform);
    }
}
