using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _openCellSprites;
    private bool _isBomb;
    private int _currentNumber;
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

    public ParticleSystem explosion;
    public ParticleSystem cellOpened;
    public ParticleSystem bombDefused;
    public SpriteRenderer bombsprite;

    private float _bombSpriteTimer;
    private SpriteRenderer _bombSprite;

    public int OpenEmptyScore;
    public int OpenBombScore;
    public int FailEmptyPenalty;
    public int FailBombPenalty;

    // Sounds
    public AudioSource CorrectEmptySound;
    public AudioSource CorrectBombSound;
    public AudioSource IncorrectEmptySound;
    public AudioSource IncorrectBombSound;

    
    // Position in grid
    public int X {get; set;}
    public int Y {get; set;}

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _openCellSprites = new Sprite[]{OpenCellSprite0, OpenCellSprite1, OpenCellSprite2, OpenCellSprite3, OpenCellSprite4,
        OpenCellSprite5, OpenCellSprite6, OpenCellSprite7, OpenCellSprite8};
        _isBomb = false;
        _currentNumber = -1;
        OpenEmptyScore = 10;
        OpenBombScore = 100;
        FailEmptyPenalty = 10;
        FailBombPenalty = 100;
    }

    private void Update() {
        CountBombSpriteDown();
    }

    public bool IsOpen() {
        return _spriteRenderer.sprite != UnopenedCellSprite;
    }

    public bool IsBomb() {
        return _isBomb;
    }

    // Puts a bomb down under the cell
    // Used in initializing cell
    public void PlantBomb() {
        _isBomb = true;
    }

    // Changes the color of a cell
    // Used when shooting cells to change to green / red and then back to white later
    public void ChangeColor(Color color) {
        _spriteRenderer.color = color;
    }

    // If the cell had a bomb, remove it and update indicators around it
    public void DefuseBomb() {
        if (IsBomb()) {
            _isBomb = false;
            List<Cell> surroundingCells = GetSurroundingCells();

            foreach (Cell cell in surroundingCells)
            {
                // Updating indicators around
                if (cell.IsOpen()) {
                    cell.ShowNumber(Mathf.Max(cell.GetNumber() - 1, 0));
                }
            }

            // Explicitly opening surrounding cells that are now showing zero because the current cell might not show
            // zero on open and therefore not trigger these opens.
            // This cannot be done in the previous loop as then some cells will first be opened, then their number reduced,
            // which will result in incorrect info shown
            foreach (Cell cell in surroundingCells)
            {
                if (cell.GetNumber() == 0) {
                    cell.OpenSurroundingCells();
                }
            }
        }
    }

    public int GetNumber() {
        return _currentNumber;
    }

    // Changes the sprite to show the given number
    public void ShowNumber(int number) {
        _boxCollider2D.isTrigger = true;
        _spriteRenderer.sprite = _openCellSprites[number];
        _currentNumber = number;
    }

    // Opens cell
    public void Open() {
        if (!IsOpen())
        {
            int bombCount = CountBombsAround();
            ShowNumber(bombCount);
            Grid.Instance.CellsOpened += 1; // Counting the opened cell
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
    private int CountBombsAround() {
        int bombCount = 0;

        foreach (Cell cell in GetSurroundingCells())
        {
            if (cell.IsBomb()) {
                bombCount++;
            }
        }

        return bombCount;
    }

    // Returns surrounding cells
    private List<Cell> GetSurroundingCells() {
        List<Cell> surroundingCells = new List<Cell>();

        for (int col = X - 1; col <= X + 1; col++)
        {
            if (col >= 0 && col < Grid.Instance.Columns)
            {
                for (int row = Y - 1; row <= Y + 1; row++)
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
                Destroy(_bombSprite);
            }
        }
    }

    public void ShootWith(Color beamColor) {
        // Stun player if they made the wrong call
        if (IsBomb() && beamColor == Color.red || !IsBomb() && beamColor == Color.green) {
            // Play the explosion particle system and when bomb explodes
            if (IsBomb()) {
                Instantiate(explosion, transform.position, transform.rotation);
                _bombSprite = Instantiate(bombsprite, transform);
                Events.SetScore(Events.GetScore() - FailBombPenalty);
                IncorrectBombSound.Play();
            }
            else {
                Events.SetScore(Events.GetScore() - FailEmptyPenalty);
                IncorrectEmptySound.Play();
            }

            _bombSpriteTimer = 0.2f;
            ChangeColor(Color.red);
            PlayerController.Instance.Stun();
        }

        // Show the bomb and turn the cell green
        else if (IsBomb() && beamColor == Color.green)
        {
            Instantiate(bombDefused, transform.position, transform.rotation);
            ChangeColor(Color.green);
            _bombSprite = Instantiate(bombsprite, transform);
            _bombSpriteTimer = 0.2f;
            Events.SetScore(Events.GetScore() + OpenBombScore);
            CorrectBombSound.Play();
        }
        else if (!IsBomb() && beamColor == Color.red) // Could just be 'else' but this is more elaborate
        {
            Instantiate(cellOpened, transform.position, transform.rotation);
            Events.SetScore(Events.GetScore() + OpenEmptyScore);
            CorrectEmptySound.Play();
        }

        DefuseBomb();
        Open();
    }
}
