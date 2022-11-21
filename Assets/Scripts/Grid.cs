using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Grid : MonoBehaviour
{
    public static Grid Instance;
    [HideInInspector]
    public int CellsOpened {get; set;} // Number of cells that have been opened in total
    public Cell CellPrefab;
    public Cell[][] CellGrid {get; set;}
    public int Columns;
    public int Rows;
    public float BombProbability;
    private byte[][] _gridState; // bombless cells mapped to 0-10 according to their cell sprite number, bomb cells mapped to 11
    [SerializeField] private PhotonView _view;
    private bool _initialized;

    private void Awake() {
        Instance = this;
        CellsOpened = 0;
        _initialized = false;
    }

    IEnumerator Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Columns")); // Wait until the room has been initialized by Host
        }

        InitializeGrid();

        if (PhotonNetwork.IsMasterClient)
        {
            InitializeRoomProperties();
        }
        _initialized = true;
    }

    private void InitializeGrid()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!PhotonNetwork.IsMasterClient)
        {
            Columns = (int) properties["Columns"];
            Rows = (int) properties["Rows"];
            CellsOpened = (int) properties["CellsOpened"];
        }

        CellGrid = new Cell[Columns][];
        _gridState = new byte[Columns][];

        float cellSize = CellPrefab.GetComponent<Renderer>().bounds.size.x;
        float colMidpoint = Columns / 2f - 0.5f;
        float rowMidpoint = Rows / 2f - 0.5f;
        float offsetX = cellSize * colMidpoint;
        float offsetY = cellSize * rowMidpoint;

        for (int col = 0; col < Columns; col++)
        {
            CellGrid[col] = new Cell[Rows];
            _gridState[col] = new byte[Rows];

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

                // Give the cell its location info
                cell.Col = col;
                cell.Row = row;

                if (PhotonNetwork.IsMasterClient)
                {
                    // Make the outer edges into borders
                    if (col == 0 || col == Columns - 1 || row == 0 || row == Rows - 1) {
                        cell.CurrentSprite = 10;
                    }
                    else {
                        // Plant bombs randomly, but not in the middle
                        if (Mathf.Abs(col - colMidpoint) >= 2.1 || Mathf.Abs(row - rowMidpoint) >= 2.1) {
                            if (Random.value < BombProbability) {
                                cell.IsBomb = true;
                            }
                        }
                    }
                }
                else // If not Host
                {
                    byte gridStateIndicator = (byte) ((byte[]) properties["GridState_Column" + col])[row];
                    if (gridStateIndicator == 11)
                    {
                        cell.IsBomb = true;
                    }
                    else
                    {
                        cell.CurrentSprite = gridStateIndicator;
                    }
                }
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Open cells in the middle of the grid
            for (int col = 0; col < Columns-1; col++)
            {
                for (int row = 0; row < Rows-1; row++)
                {
                    Cell cell = CellGrid[col][row];

                    if (Mathf.Abs(col - colMidpoint) < 2.1 && Mathf.Abs(row - rowMidpoint) < 2.1) {
                        cell.Open();
                    }
                }
            }
        }
    }

    private void InitializeRoomProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!properties.TryAdd("Columns", Columns)) properties["Columns"] = Columns;
        if (!properties.TryAdd("Rows", Rows)) properties["Rows"] = Rows;
        if (!properties.TryAdd("CellsOpened", CellsOpened)) properties["CellsOpened"] = CellsOpened;

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Cell cell = CellGrid[col][row];

                if (cell.IsBomb)
                {
                    _gridState[col][row] = 11;
                }
                else
                {
                    _gridState[col][row] = cell.CurrentSprite;
                }
            }
            if (!properties.TryAdd("GridState_Column" + col, _gridState[col])) properties["GridState_Column" + col] = _gridState[col];
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    private void OnDestroy()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        properties.Remove("Columns");
        properties.Remove("Rows");
        properties.Remove("CellsOpened");
        for (int col = 0; col < Columns; col++)
        {
            properties.Remove("GridState_Column" + col);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void SetCellsOpened(int value)
    {
        _view.RPC("SetCellsOpenedRPC", RpcTarget.All, value);
    }

    [PunRPC]
    void SetCellsOpenedRPC(int value)
    {
        CellsOpened = value;
    }

    public void RemoveBomb(int col, int row)
    {
        _view.RPC("RemoveBombRPC", RpcTarget.All, col, row);
    }

    [PunRPC]
    void RemoveBombRPC(int col, int row)
    {
        CellGrid[col][row].IsBomb = false;
        SetGridStateIndicator(col, row, 9); // Updating state for defusal in case someone connects before Open is called
    }

    public void SetCurrentSprite(int col, int row, byte value)
    {
        _view.RPC("SetCurrentSpriteRPC", RpcTarget.All, col, row, value);
    }

    [PunRPC]
    void SetCurrentSpriteRPC(int col, int row, byte value)
    {
        if (PhotonNetwork.IsMasterClient || _initialized)
        {
            CellGrid[col][row].CurrentSprite = value;
            SetGridStateIndicator(col, row, value);
        }
    }

    public void SetGridStateIndicator(int col, int row, byte value)
    {
        _view.RPC("SetGridStateIndicatorRPC", RpcTarget.MasterClient, col, row, value);
    }

    [PunRPC]
    void SetGridStateIndicatorRPC(int col, int row, byte value)
    {
        _gridState[col][row] = value;
    }
}
