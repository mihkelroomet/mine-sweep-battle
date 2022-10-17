using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cell : MonoBehaviour
{
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _openCellSprites;
    private bool _isBomb;
    private int _currentNumber;
    public static int openNo=0; // Number of cells opened?
    public static int openNoforScore = 0;
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
            openNo += 1; // Counting the opened cell
            openNoforScore += 1; // for score
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

}
