using UnityEngine.UI;

public class UIButton : UIInteractable
{
    private Button _button;
    public SFXClipGroup SelectAudio;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Click);
    }

    private void Click()
    {
        SelectAudio.Play(SFXSourcePool.Instance.transform);
    }
}
