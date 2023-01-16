using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColors : MonoBehaviour
{
    private Transform playerSprite;
    private SpriteRenderer rendTorso;
    private SpriteRenderer rendSleeves;
    private SpriteRenderer rendHat;
    private SpriteRenderer rendPants;

    private Color32 _shirtColor;
    private Color32 _hatColor;
    private Color32 _pantsColor;

    public void Awake()
    {
        _shirtColor = PlayerOutfitChanger.Instance.ShirtColors[PlayerOutfitChanger.Instance.CurrentShirt];
        _hatColor = PlayerOutfitChanger.Instance.HatColors[PlayerOutfitChanger.Instance.CurrentHat];
        _pantsColor = PlayerOutfitChanger.Instance.PantsColors[PlayerOutfitChanger.Instance.CurrentPants];
    }
    private void Start()
    {
        playerSprite = gameObject.transform.Find("PlayerSprite");
        rendTorso = playerSprite.Find("Torso").GetComponent<SpriteRenderer>();
        rendSleeves = playerSprite.Find("Sleeves").GetComponent<SpriteRenderer>();
        rendHat = playerSprite.Find("Hat").GetComponent<SpriteRenderer>();
        rendPants = playerSprite.Find("Pants").GetComponent<SpriteRenderer>();

        rendTorso.color = _shirtColor;
        rendSleeves.color = _shirtColor;
        rendHat.color = _hatColor;
        rendPants.color = _pantsColor;
    }
}
