using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private PhotonView _view;
    [SerializeField] private TMP_Text _nametagText;
    [SerializeField] private SpriteRenderer _rendHat;
    [SerializeField] private SpriteRenderer _rendShirt;
    [SerializeField] private SpriteRenderer _rendSleeves;
    [SerializeField] private SpriteRenderer _rendPants;

    private void Awake()
    {
        _nametagText.text = _view.Owner.NickName;
        _rendHat.color = PlayerCustomizer.Instance.TranslateHatColor((int) _view.Owner.CustomProperties["HatColor"]);
        int shirtColor = (int) _view.Owner.CustomProperties["ShirtColor"];
        _rendShirt.color = PlayerCustomizer.Instance.TranslateShirtColor(shirtColor);
        _rendSleeves.color = PlayerCustomizer.Instance.TranslateShirtColorIntoSleeveColor(shirtColor);
        _rendPants.color = PlayerCustomizer.Instance.TranslatePantsColor((int) _view.Owner.CustomProperties["PantsColor"]);
    }
}
