using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler
{
    private Button _button;
    public AudioClipGroup ChooseAudio;
    public AudioClipGroup SelectAudio;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Click);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        ChooseAudio.Play(AudioSourcePool.Instance.transform);
    }

    void Click()
    {
        SelectAudio.Play(AudioSourcePool.Instance.transform);
    }
}
