using UnityEngine;

public class GridOld : MonoBehaviour
{
    
    public static GridOld Instance;
    [HideInInspector]
    public int CellsOpened; // Number of cells that have been opened in total
    public CellOld CellPrefab;
    public CellOld[][] CellGrid {get; set;}
    public int Columns;
    public int Rows;
    public float BombProbability;

    private void Awake() {
        Instance = this;
        CellGrid = new CellOld[Columns][];
        for (int col = 0; col < Columns; col++) {
            CellGrid[col] = new CellOld[Rows];
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
                CellOld cell = GameObject.Instantiate(
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

                // Make the outer edges into borders
                if (col == 0 || col == Columns - 1 || row == 0 || row == Rows - 1) {
                    cell.MakeIntoBorderCell();
                }
                else {
                    // Plant bombs randomly, but not in the middle
                    if (Mathf.Abs(col - colMidpoint) >= 2.1 || Mathf.Abs(row - rowMidpoint) >= 2.1) {
                        if (Random.value < BombProbability) {
                            cell.PlantBomb();
                        }
                    }
                }
            }
        }

        for (int col = 0; col < Columns-1; col++)
        {
            for (int row = 0; row < Rows-1; row++)
            {
                CellOld cell = CellGrid[col][row];

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
