using UnityEngine;

public class Grid : MonoBehaviour
{
    public Cell CellPrefab;
    public Grid Instance;
    public Cell[][] CellGrid {get; set;}
    
    public int Columns;
    public int Rows;
    public float BombProbability;

    private void Awake() {
        Instance = this;
        CellGrid = new Cell[Rows][];
        for (int row = 0; row < Rows; row++) {
            CellGrid[row] = new Cell[Columns];
        }
    }

    void Start()
    {
        float cellSize = CellPrefab.GetComponent<Renderer>().bounds.size.x;
        float offsetX = cellSize * (Columns / 2f - 0.5f);
        float offsetY = cellSize * (Rows / 2f - 0.5f);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Cell cell = GameObject.Instantiate(
                    CellPrefab,
                    new Vector3(cellSize * col - offsetX, cellSize * row - offsetY),
                    Quaternion.identity
                    );

                CellGrid[row][col] = cell;

                // Make an opening in the middle for the player
                float rowMidpoint = Rows / 2f - 0.5f;
                float colMidpoint = Columns / 2f - 0.5f;
                if (Mathf.Abs(row - rowMidpoint) < 1.6 && Mathf.Abs(col - colMidpoint) < 1.6) {
                    cell.showNumber(0);
                }

                if (Random.value < BombProbability) {
                    cell.PlantBomb();
                }
            }
        }
    }

    void Update()
    {
        
    }
}
