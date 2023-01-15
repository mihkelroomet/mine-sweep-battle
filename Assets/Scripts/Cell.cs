using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private BoxCollider2D _boxCollider2D;
    public bool IsMine {
        get
        {
            return CurrentSprite == 11;
        }
        set
        {
            if (value) CurrentSprite = 11; // When planting mine
            else Open(); // When removing mine
        }
    }

    // Cell Sprites
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _cellSprites; // 0-8 - Opened, 9 - Unopened Non-Mine, 10 - Border, 11 - Unopened Mine
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
        get
        {
            return _currentSprite;
        }
        set
        {
            // Only valid changes are higher → lower during game and 9 → higher at grid initialization
            if (value < _currentSprite || value > _currentSprite && _currentSprite == 9)
            {
                if (value < 9) _boxCollider2D.isTrigger = true; // Enable stepping on if opened
                else _boxCollider2D.isTrigger = false; // Disable stepping on if unopened
                _spriteRenderer.sprite = _cellSprites[value];
                _currentSprite = value;
            }
        }
    }
    private byte _currentSprite;

    // Bomb Sprite
    public SpriteRenderer MineSpritePrefab;
    private float _mineSpriteTimer;
    private SpriteRenderer _mineSpriteRenderer;

    // Particle Effects
    public ParticleSystem Explosion;
    public ParticleSystem CellOpened;
    public ParticleSystem MineDefused;

    // Scoring
    public int OpenEmptyScore;
    public int OpenMineScore;
    public int FailEmptyPenalty;
    public int FailMinePenalty;

    // Sounds
    public AudioClipGroup CorrectEmptySound;
    public AudioClipGroup CorrectMineSound;
    public AudioClipGroup IncorrectEmptySound;
    public AudioClipGroup IncorrectMineSound;

    // Position in grid
    public int Column {get; set;}
    public int Row {get; set;}

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cellSprites = new Sprite[]{OpenCellSprite0, OpenCellSprite1, OpenCellSprite2, OpenCellSprite3, OpenCellSprite4,
        OpenCellSprite5, OpenCellSprite6, OpenCellSprite7, OpenCellSprite8, UnopenedCellSprite, BorderCellSprite, UnopenedCellSprite};
        _currentSprite = 9; // 9 - Unopened Non-Mine Cell Sprite
    }

    private void Update()
    {
        if (_mineSpriteRenderer || _spriteRenderer.color != Color.white) CountMineSpriteDown();
    }

    public bool IsOpen()
    {
        return CurrentSprite < 9;
    }

    public bool IsBorderCell()
    {
        return CurrentSprite == 10;
    }

    /// <summary>
    /// Changes the color of a cell<br></br>
    /// Used when shooting cells to change to green / red and then back to white later
    /// </summary>
    public void ChangeColor(Color color) {
        _spriteRenderer.color = color;
        _mineSpriteTimer = 0.2f;
    }

    /// <summary>
    /// Removes mine and updates surrounding indicators<br></br>
    /// Meant to be called right before Open only
    /// </summary>
    public void RemoveMine() {
        if (IsMine && !IsBorderCell()) {
            Grid.Instance.SetCurrentSprite(Column, Row, 9);
            List<Cell> surroundingCells = GetSurroundingCells();

            foreach (Cell cell in surroundingCells)
            {
                // Updating indicators around
                if (cell.IsOpen()) {
                    Grid.Instance.SetCurrentSprite(cell.Column, cell.Row, (byte) Mathf.Max(cell.CurrentSprite - 1, 0));
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

    public void Open() {
        if (!IsOpen() && !IsBorderCell())
        {
            byte bombCount = CountMinesAround();
            Grid.Instance.SetCurrentSprite(Column, Row, bombCount);
            if (bombCount == 0) {
                OpenSurroundingCells();
            }
        }
    }

    private void OpenSurroundingCells() {
        foreach (Cell cell in GetSurroundingCells())
        {
            cell.Open();
        }
    }

    /// <summary>
    /// Counts mines in the 8 surrounding cells
    /// </summary>
    private byte CountMinesAround() {
        byte bombCount = 0;

        foreach (Cell cell in GetSurroundingCells())
        {
            if (cell.IsMine) {
                bombCount++;
            }
        }

        return bombCount;
    }

    private List<Cell> GetSurroundingCells() {
        List<Cell> surroundingCells = new List<Cell>();

        for (int col = Column - 1; col <= Column + 1; col++)
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

    public void ShootWith(Color beamColor, PlayerController player) {

        // Animations and sounds
        if (!IsOpen() && !IsBorderCell()) {
            // Stun player if they made the wrong call
            if (IsMine && beamColor == Color.red || !IsMine && beamColor == Color.green) {
                // Play the explosion particle system and when mine explodes
                if (IsMine) {
                    Events.SetScore(Events.GetScore() + FailMinePenalty);
                    Grid.Instance.HandleCellShotEvent(Column, Row, 1);
                }
                else {
                    Events.SetScore(Events.GetScore() + FailEmptyPenalty);
                    Grid.Instance.HandleCellShotEvent(Column, Row, 2);
                }

                player.Stun();
            }

            // Show the mine and turn the cell green
            else if (IsMine && beamColor == Color.green)
            {
                Events.SetScore(Events.GetScore() + OpenMineScore);
                Grid.Instance.HandleCellShotEvent(Column, Row, 3);
            }
            else if (!IsMine && beamColor == Color.red) // Could just be 'else' but this is more elaborate
            {
                Events.SetScore(Events.GetScore() + OpenEmptyScore);
                Grid.Instance.HandleCellShotEvent(Column, Row, 4);
            }
        }
        
        RemoveMine();
        Open();
    }

    /// <summary>
    /// Displays cell effects on getting shot<br></br>
    /// 1 - Incorrect shot on mine cell<br></br>
    /// 2 - Incorrect shot on empty cell<br></br>
    /// 3 - Correct shot on mine cell<br></br>
    /// 4 - Correct shot on empty cell
    /// </summary>
    public void DisplayEffectsOnCellShotEvent(byte eventType)
    {
        switch (eventType)
        {
            case 1:
                Instantiate(Explosion, transform.position, transform.rotation);
                DisplayMineSprite();
                ChangeColor(Color.red);
                IncorrectMineSound.Play(transform);
                break;
            case 2:
                ChangeColor(Color.red);
                IncorrectEmptySound.Play(transform);
                break;
            case 3:
                Instantiate(MineDefused, transform.position, transform.rotation);
                DisplayMineSprite();
                ChangeColor(Color.green);
                CorrectMineSound.Play(transform);
                break;
            case 4:
                Instantiate(CellOpened, transform.position, transform.rotation);
                CorrectEmptySound.Play(transform);
                break;
        }
    }

    /// <summary>
    /// Makes sure the mine sprite disappears
    /// </summary>
    private void CountMineSpriteDown() {
        if (_mineSpriteTimer > 0) {
            _mineSpriteTimer -= Time.deltaTime;
            if (_mineSpriteTimer <= 0) {
                ChangeColor(Color.white);
                HideMineSprite();
            }
        }
    }

    private void DisplayMineSprite() {
        if (!_mineSpriteRenderer)
        {
            _mineSpriteRenderer = Instantiate(MineSpritePrefab, transform);
            _mineSpriteTimer = 0.2f;
        }
    }

    private void HideMineSprite() {
        if (_mineSpriteRenderer) Destroy(_mineSpriteRenderer.gameObject);
    }
}
