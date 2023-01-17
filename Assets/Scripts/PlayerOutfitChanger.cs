using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOutfitChanger : MonoBehaviour
{
    public static PlayerOutfitChanger Instance;

    public List<Color32> HatColors;
    public List<Color32> ShirtColors;
    public List<Color32> PantsColors;

    public int CurrentHat;
    public int CurrentShirt;
    public int CurrentPants;

    public Transform PlayerPreview;
    private Image _hat;
    private Image _torso;
    private Image _sleeves;
    private Image _pants;

    private int _darkener = -10;
    private Color32 _sleevesColor;

    private void Awake()
    {
        _hat = PlayerPreview.Find("Hat").GetComponent<Image>();
        _torso = PlayerPreview.Find("Torso").GetComponent<Image>();
        _sleeves = PlayerPreview.Find("Sleeves").GetComponent<Image>();
        _pants = PlayerPreview.Find("Pants").GetComponent<Image>();

        CurrentHat = 0;
        CurrentShirt = 0;
        CurrentPants = 0;

        _hat.color = HatColors[CurrentHat];
        _torso.color = ShirtColors[CurrentShirt];
        _sleeves.color = ShirtColors[CurrentShirt];
        _pants.color = PantsColors[CurrentPants];

        Instance = this;
    }

    public void NextHat()
    {
        CurrentHat++;
        if (CurrentHat >= HatColors.Count)
            CurrentHat = 0;
        _hat.color = HatColors[CurrentHat];
    }
    public void PreviousHat()
    {
        CurrentHat--;
        if (CurrentHat < 0)
            CurrentHat = HatColors.Count-1;
        _hat.color = HatColors[CurrentHat];
    }


    public void NextShirt()
    {
        CurrentShirt++;
        if (CurrentShirt >= ShirtColors.Count)
            CurrentShirt = 0;
        _torso.color = ShirtColors[CurrentShirt];
        _sleeves.color = Darken(_torso.color);
    }
    public void PreviousShirt()
    {
        CurrentShirt--;
        if (CurrentShirt < 0)
            CurrentShirt = ShirtColors.Count - 1;
        _torso.color = ShirtColors[CurrentShirt];
        _sleeves.color = Darken(_torso.color);
    }

    public void NextPants()
    {
        CurrentPants++;
        if (CurrentPants >= PantsColors.Count)
            CurrentPants = 0;
        _pants.color = PantsColors[CurrentPants];
    }
    public void PreviousPants()
    {
        CurrentPants--;
        if (CurrentPants < 0)
            CurrentPants = PantsColors.Count - 1;
        _pants.color = PantsColors[CurrentPants];
    }

    public void NextBoots()
    {

    }

    public Color32 Darken(Color32 color)
    {
        return new Color32((byte)(color.r + _darkener), (byte)(color.g + _darkener), (byte)(color.b + _darkener), (byte)255);
    }
}
