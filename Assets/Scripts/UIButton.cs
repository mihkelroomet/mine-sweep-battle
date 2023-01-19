using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler
{
    private Button _button;
    public SFXClipGroup ChooseAudio;
    public SFXClipGroup SelectAudio;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Click);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        ChooseAudio.Play(SFXSourcePool.Instance.transform);
    }

    private void Click()
    {
        SelectAudio.Play(SFXSourcePool.Instance.transform);
    }
}
