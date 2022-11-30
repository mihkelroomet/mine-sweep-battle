using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Cell : MonoBehaviour
{
    private BoxCollider2D _boxCollider2D;
    public bool IsBomb {get; set;}

    // Cell Sprites
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _cellSprites;
    public Sprite OpenCellSprite0;
    public Sprite OpenCellSprite1;
    public Sprite OpenCellSprite2;
    public Sprite OpenCellSprite3;
    public Sprite OpenCellSprite4;
    public Sprite OpenCellSprite5;
    public Sprite OpenCellSprite6;
    public Sprite OpenCellSprite7;
    public Sprite OpenCellSprite8;
    public Sprite UnopenedCellSprite;
    public Sprite BorderCellSprite;
    public byte CurrentSprite {
        get {
            return _currentSprite;
        }
        set {
            if (value < 9) _boxCollider2D.isTrigger = true;
            _spriteRenderer.sprite = _cellSprites[value];
            _currentSprite = value;
        }
    }
    private byte _currentSprite;

    // Bomb Sprite
    public SpriteRenderer BombSpritePrefab;
    private float _bombSpriteTimer;
    private SpriteRenderer _bombSpriteRenderer;

    // Particle Effects
    public ParticleSystem Explosion;
    public ParticleSystem CellOpened;
    public ParticleSystem BombDefused;

    // Scoring
    public int OpenEmptyScore = 10;
    public int OpenBombScore = 100;
    public int FailEmptyPenalty = -10;
    public int FailBombPenalty = -100;

    // Sounds
    //public AudioSource CorrectEmptySound;
    //public AudioSource CorrectBombSound;
    //public AudioSource IncorrectEmptySound;
    //public AudioSource IncorrectBombSound;

    public AudioClipGroup CorrectEmptySound;
    public AudioClipGroup CorrectBombSound;
    public AudioClipGroup IncorrectEmptySound;
    public AudioClipGroup IncorrectBombSound;

    // Position in grid
    public int Col {get; set;}
    public int Row {get; set;}

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cellSprites = new Sprite[]{OpenCellSprite0, OpenCellSprite1, OpenCellSprite2, OpenCellSprite3, OpenCellSprite4,
        OpenCellSprite5, OpenCellSprite6, OpenCellSprite7, OpenCellSprite8, UnopenedCellSprite, BorderCellSprite};
        IsBomb = false;
        CurrentSprite = 9; // 9 - Unopened Cell Sprite
    }

    private void Update()
    {
        CountBombSpriteDown();
    }

    public bool IsOpen()
    {
        return CurrentSprite < 9;
    }

    public bool IsBorderCell()
    {
        return CurrentSprite == 10;
    }

    // Changes the color of a cell
    // Used when shooting cells to change to green / red and then back to white later
    public void ChangeColor(Color color) {
        _spriteRenderer.color = color;
    }

    // If the cell had a bomb, remove it and update indicators around it
    public void DefuseBomb() {
        if (IsBomb && !IsBorderCell()) {
            Grid.Instance.RemoveBomb(Col, Row);
            List<Cell> surroundingCells = GetSurroundingCells();

            foreach (Cell cell in surroundingCells)
            {
                // Updating indicators around
                if (cell.IsOpen()) {
                    Grid.Instance.SetCurrentSprite(cell.Col, cell.Row, (byte) Mathf.Max(cell.CurrentSprite - 1, 0));
                }
            }

            // Explicitly opening surrounding cells that are now showing zero because the current cell might not show
            // zero on open and therefore not trigger these opens.
            // This cannot be done in the previous loop as then some cells will first be opened, then their number reduced,
            // which will result in incorrect info shown
            foreach (Cell cell in surroundingCells)
            {
                if (cell.CurrentSprite == 0) {
                    cell.OpenSurroundingCells();
                }
            }
        }
    }

    // Opens cell
    public void Open() {
        if (!IsOpen() && !IsBorderCell())
        {
            byte bombCount = CountBombsAround();
            Grid.Instance.SetCurrentSprite(Col, Row, bombCount);
            Grid.Instance.SetCellsOpened(Grid.Instance.CellsOpened + 1);
            if (bombCount == 0) {
                OpenSurroundingCells();
            }
        }
    }

    // Opens surrounding cells
    private void OpenSurroundingCells() {
        foreach (Cell cell in GetSurroundingCells())
        {
            cell.Open();
        }
    }

    // Counts bombs around itself
    private byte CountBombsAround() {
        byte bombCount = 0;

        foreach (Cell cell in GetSurroundingCells())
        {
            if (cell.IsBomb) {
                bombCount++;
            }
        }

        return bombCount;
    }

    // Returns surrounding cells
    private List<Cell> GetSurroundingCells() {
        List<Cell> surroundingCells = new List<Cell>();

        for (int col = Col - 1; col <= Col + 1; col++)
        {
            if (col >= 0 && col < Grid.Instance.Columns)
            {
                for (int row = Row - 1; row <= Row + 1; row++)
                {
                    if (row >= 0 && row < Grid.Instance.Rows)
                    {
                        surroundingCells.Add(Grid.Instance.CellGrid[col][row]);
                    }
                }
            }
        }
        
        return surroundingCells;
    }

    // Makes sure the bomb sprite disappears
    private void CountBombSpriteDown() {
        if (_bombSpriteTimer > 0) {
            _bombSpriteTimer -= Time.deltaTime;
            if (_bombSpriteTimer <= 0) {
                ChangeColor(Color.white);
                HideBombSprite();
            }
        }
    }

    public void ShootWith(Color beamColor, PlayerController player) {

        // Animations and sounds
        if (!IsOpen() && !IsBorderCell()) {
            // Stun player if they made the wrong call
            if (IsBomb && beamColor == Color.red || !IsBomb && beamColor == Color.green) {
                // Play the explosion particle system and when bomb explodes
                if (IsBomb) {
                    Instantiate(Explosion, transform.position, transform.rotation);
                    DisplayBombSprite();
                    Events.SetScore(Events.GetScore() + FailBombPenalty);
                    IncorrectBombSound.Play();
                }
                else {
                    Events.SetScore(Events.GetScore() + FailEmptyPenalty);
                    IncorrectEmptySound.Play();
                }

                _bombSpriteTimer = 0.2f;
                ChangeColor(Color.red);
                player.Stun();
            }

            // Show the bomb and turn the cell green
            else if (IsBomb && beamColor == Color.green)
            {
                Instantiate(BombDefused, transform.position, transform.rotation);
                ChangeColor(Color.green);
                DisplayBombSprite();
                _bombSpriteTimer = 0.2f;
                Events.SetScore(Events.GetScore() + OpenBombScore);
                CorrectBombSound.Play();
            }
            else if (!IsBomb && beamColor == Color.red) // Could just be 'else' but this is more elaborate
            {
                Instantiate(CellOpened, transform.position, transform.rotation);
                Events.SetScore(Events.GetScore() + OpenEmptyScore);
                CorrectEmptySound.Play();
            }
        }
        
        DefuseBomb();
        Open();
    }

    private void DisplayBombSprite() {
        _bombSpriteRenderer = Instantiate(BombSpritePrefab, transform);
    }

    private void HideBombSprite() {
        Destroy(_bombSpriteRenderer);
    }
}
