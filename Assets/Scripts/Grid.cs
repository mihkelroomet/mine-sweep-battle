using UnityEngine;

public class Grid : MonoBehaviour
{
    
    public static Grid Instance;
    [HideInInspector]
    public int CellsOpened; // Number of cells that have been opened in total

    public Cell CellPrefab;
    public Cell[][] CellGrid {get; set;}
    public int Columns;
    public int Rows;
    public float BombProbability;

    private void Awake() {
        Instance = this;
        CellGrid = new Cell[Columns][];
        for (int col = 0; col < Columns; col++) {
            CellGrid[col] = new Cell[Rows];
        }
        CellsOpened = 0;
    }

    void Start()
    {
        float cellSize = CellPrefab.GetComponent<Renderer>().bounds.size.x;
        float colMidpoint = Columns / 2f - 0.5f;
        float rowMidpoint = Rows / 2f - 0.5f;
        float offsetX = cellSize * colMidpoint;
        float offsetY = cellSize * rowMidpoint;

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                // Create cells
                Cell cell = GameObject.Instantiate(
                    CellPrefab,
                    new Vector3(cellSize * col - offsetX, cellSize * row - offsetY),
                    Quaternion.identity
                    );
                cell.transform.parent = transform; // make cells children of Grid

                // Save cells to grid for easy access
                CellGrid[col][row] = cell;

                // Also give the cell its location info
                cell.X = col;
                cell.Y = row;

                // Plant bombs randomly, but not in the middle
                if (Mathf.Abs(col - colMidpoint) >= 2.1 || Mathf.Abs(row - rowMidpoint) >= 2.1) {
                    if (Random.value < BombProbability) {
                        cell.PlantBomb();
                    }
                }
            }
        }

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Cell cell = CellGrid[col][row];

                // Open the cells in the middle
                if (Mathf.Abs(col - colMidpoint) < 2.1 && Mathf.Abs(row - rowMidpoint) < 2.1) {
                    cell.Open();
                }
            }
        }
    }

    void Update()
    {
        
    }
}
