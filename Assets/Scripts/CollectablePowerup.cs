using UnityEngine;
using Photon.Pun;

public class CollectablePowerup : MonoBehaviour
{
    public PowerupData Data
    {
        get
        {
            return _data;
        }
        set
        {
            _data = value;
            _spriteRenderer.sprite = _data.CollectablePic;
        }
    }

    private PowerupData _data;
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;

    // Position in grid
    public int Column {get; set;}
    public int Row {get; set;}

    public float ExpiresAt {get; set;}

    private void Awake() {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if (ExpiresAt != 0 && GameController.Instance.TimeLeft != 0 && ExpiresAt > GameController.Instance.TimeLeft)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                if (Events.GetPowerupInFirstSlot() == null) Events.SetPowerupInFirstSlot(Data);
                else if (Events.GetPowerupInSecondSlot() == null) Events.SetPowerupInSecondSlot(Data);
                else return; // Don't destroy if slots full
                Grid.Instance.DestroyCollectablePowerup(Column, Row, (byte) Data.Type);
            }
        }
    }
}
