using UnityEngine;

public class Cell : MonoBehaviour
{
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _openCellSprites;
    private bool _isBomb;

    public Sprite UnopenedCellSprite;
    public Sprite OpenCellSprite0;
    public Sprite OpenCellSprite1;
    public Sprite OpenCellSprite2;
    public Sprite OpenCellSprite3;
    public Sprite OpenCellSprite4;
    public Sprite OpenCellSprite5;
    public Sprite OpenCellSprite6;
    public Sprite OpenCellSprite7;
    public Sprite OpenCellSprite8;

    void Start()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _openCellSprites = new Sprite[]{OpenCellSprite0, OpenCellSprite1, OpenCellSprite2, OpenCellSprite3, OpenCellSprite4,
        OpenCellSprite5, OpenCellSprite6, OpenCellSprite7, OpenCellSprite8};
        _isBomb = false;
    }

    void Update()
    {
        
    }

    // puts a bomb down under the cell if it's not yet been opened
    // used in initializing cell
    public void plantBomb() {
        if (_spriteRenderer.sprite == UnopenedCellSprite) {
            _isBomb = true;
        }
    }

    // changes the sprite to show the given number
    public void showNumber(int number) {
        _spriteRenderer.sprite = _openCellSprites[number];
    }

    private void OnTriggerEnter2D(Collider2D other) {
        
    }
}
