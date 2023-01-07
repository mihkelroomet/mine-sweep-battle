using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColors : MonoBehaviour
{
    private SpriteRenderer rend;
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
        rend = GetComponent<SpriteRenderer>();
        rend.color = background;
    }
}
