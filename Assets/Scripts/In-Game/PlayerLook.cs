using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerLook : MonoBehaviour
{
    // Components
    [SerializeField] private PhotonView _view;
    [SerializeField] private SpriteRenderer _rendNaked;
    [SerializeField] private SpriteRenderer _rendHands;
    [SerializeField] private SpriteRenderer _rendHat;
    [SerializeField] private SpriteRenderer _rendShirt;
    [SerializeField] private SpriteRenderer _rendSleeves;
    [SerializeField] private SpriteRenderer _rendPants;
    [SerializeField] private SpriteRenderer _rendEffects;
    [SerializeField] private Image _nametagPanel;
    [SerializeField] private TMP_Text _nametagText;
    private SpriteRenderer[] _spriteRenderers;

    // Parameters
    public float OtherPlayersOpacity;
    public float OtherPlayersNametagPanelOpacity;

    private void Awake()
    {
        _nametagText.text = _view.Owner.NickName;
        _rendHat.color = PlayerCustomizer.Instance.TranslateHatColor((int) _view.Owner.CustomProperties["HatColor"]);
        int shirtColor = (int) _view.Owner.CustomProperties["ShirtColor"];
        _rendShirt.color = PlayerCustomizer.Instance.TranslateShirtColor(shirtColor);
        _rendSleeves.color = PlayerCustomizer.Instance.TranslateShirtColorIntoSleeveColor(shirtColor);
        _rendPants.color = PlayerCustomizer.Instance.TranslatePantsColor((int) _view.Owner.CustomProperties["PantsColor"]);

        _spriteRenderers = new SpriteRenderer[] { _rendNaked, _rendHands, _rendHat, _rendShirt, _rendSleeves, _rendPants, _rendEffects };

        // Making other players besides the controlled one transparent
        if (!_view.IsMine)
        {
            foreach (SpriteRenderer spriteRenderer in _spriteRenderers) MakeTransparent(spriteRenderer);
            _nametagPanel.color = new Color(_nametagPanel.color.r, _nametagPanel.color.g, _nametagPanel.color.b, _nametagPanel.color.a * OtherPlayersNametagPanelOpacity);
        }
    }

    private void MakeTransparent(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a * OtherPlayersOpacity);
    }
}
