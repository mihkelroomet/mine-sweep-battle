using UnityEngine;
using UnityEngine.UI;

public class DummyPlayer : MonoBehaviour
{
    [SerializeField] private Image _hat;
    [SerializeField] private Image _shirt;
    [SerializeField] private Image _sleeves;
    [SerializeField] private Image _pants;
    [SerializeField] private Image _boots;

    private void Awake()
    {
        _hat.color = PlayerCustomizer.Instance.TranslateHatColor(Events.GetHatColor());
        int shirtColor = Events.GetShirtColor();
        _shirt.color = PlayerCustomizer.Instance.TranslateShirtColor(shirtColor);
        _sleeves.color = PlayerCustomizer.Instance.TranslateShirtColorIntoSleeveColor(shirtColor);
        _pants.color = PlayerCustomizer.Instance.TranslatePantsColor(Events.GetPantsColor());
        _boots.color = PlayerCustomizer.Instance.TranslateBootsColor(Events.GetBootsColor());

        Events.OnSetHatColor += SetHatColor;
        Events.OnSetShirtColor += SetShirtColor;
        Events.OnSetPantsColor += SetPantsColor;
        Events.OnSetBootsColor += SetBootsColor;
    }

    private void SetHatColor(int value)
    {
        _hat.color = PlayerCustomizer.Instance.TranslateHatColor(value);
    }

    private void SetShirtColor(int value)
    {
        _shirt.color = PlayerCustomizer.Instance.TranslateShirtColor(value);
        _sleeves.color = PlayerCustomizer.Instance.TranslateShirtColorIntoSleeveColor(value);
    }

    private void SetPantsColor(int value)
    {
        _pants.color = PlayerCustomizer.Instance.TranslatePantsColor(value);
    }

    private void SetBootsColor(int value)
    {
        _boots.color = PlayerCustomizer.Instance.TranslateBootsColor(value);
    }

    private void OnDestroy()
    {
        Events.OnSetHatColor -= SetHatColor;
        Events.OnSetShirtColor -= SetShirtColor;
        Events.OnSetPantsColor -= SetPantsColor;
        Events.OnSetBootsColor -= SetBootsColor;
    }
}
