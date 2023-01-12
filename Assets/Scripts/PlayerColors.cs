using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColors : MonoBehaviour
{
    private Transform playerSprite;
    private SpriteRenderer rendTorso;
    private SpriteRenderer rendSleeves;

    public Color32 background;
    public void Awake()
    {
        background = new Color32(
            (byte)Random.Range(74, 200),
            (byte)Random.Range(80, 240),
            (byte)Random.Range(45, 255),
            255
        );
    }
    private void Start()
    {
        playerSprite = gameObject.transform.Find("PlayerSprite");
        rendTorso = playerSprite.Find("Torso").GetComponent<SpriteRenderer>();
        rendSleeves = playerSprite.Find("Sleeves").GetComponent<SpriteRenderer>();

        rendTorso.color = background;
        rendSleeves.color = background;
    }
}
