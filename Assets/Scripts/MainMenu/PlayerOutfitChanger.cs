using UnityEngine;
using UnityEngine.UI;

public class PlayerOutfitChanger : MonoBehaviour
{
    public static PlayerOutfitChanger Instance;

    public Transform PlayerPreview;
    private Image _hat;
    private Image _shirt;
    private Image _sleeves;
    private Image _pants;
    private Image _boots;

    private Color32 _sleevesColor;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        _hat = PlayerPreview.Find("Hat").GetComponent<Image>();
        _shirt = PlayerPreview.Find("Shirt").GetComponent<Image>();
        _sleeves = PlayerPreview.Find("Sleeves").GetComponent<Image>();
        _pants = PlayerPreview.Find("Pants").GetComponent<Image>();
        _boots = PlayerPreview.Find("Boots").GetComponent<Image>();

        _hat.color = PlayerCustomizer.Instance.TranslateHatColor(Events.GetHatColor());
        _shirt.color = PlayerCustomizer.Instance.TranslateShirtColor(Events.GetShirtColor());
        _sleeves.color = PlayerCustomizer.Instance.TranslateShirtColorIntoSleeveColor(Events.GetShirtColor());
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

    public void NextHat()
    {
        Events.SetHatColor(PlayerCustomizer.Instance.GetNextHat());
    }

    public void PreviousHat()
    {
        Events.SetHatColor(PlayerCustomizer.Instance.GetPrevHat());
    }

    public void NextShirt()
    {
        Events.SetShirtColor(PlayerCustomizer.Instance.GetNextShirt());
    }

    public void PreviousShirt()
    {
        Events.SetShirtColor(PlayerCustomizer.Instance.GetPrevShirt());
    }

    public void NextPants()
    {
        Events.SetPantsColor(PlayerCustomizer.Instance.GetNextPants());
    }

    public void PreviousPants()
    {
        Events.SetPantsColor(PlayerCustomizer.Instance.GetPrevPants());
    }

    public void NextBoots()
    {
        Events.SetBootsColor(PlayerCustomizer.Instance.GetNextBoots());
    }

    public void PreviousBoots()
    {
        Events.SetBootsColor(PlayerCustomizer.Instance.GetPrevBoots());
    }

    private void OnDestroy()
    {
        Events.OnSetHatColor -= SetHatColor;
        Events.OnSetShirtColor -= SetShirtColor;
        Events.OnSetPantsColor -= SetPantsColor;
        Events.OnSetBootsColor -= SetBootsColor;
    }
}
