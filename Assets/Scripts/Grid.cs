using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grid : MonoBehaviour
{
    public static Grid Instance;
    public Cell CellPrefab;
    public Cell[][] CellGrid {get; set;}
    [HideInInspector]
    public int Columns;
    [HideInInspector]
    public int Rows;
    [HideInInspector]
    public float MineProbability;
    public float PowerupProbability;
    public Transform PowerupBombPrefab;
    public Bomb BombPrefab;
    public float GridCheckInterval;
    private byte[][] _gridState; // 0-8 - Opened, 9 - Unopened Non-Mine, 10 - Border, 11 - Unopened Mine
    [SerializeField] private PhotonView _view;
    public bool Initialized {
        get {
            return _initialized;
        }
    }
    private bool _initialized;
    private Queue<GridUpdateEvent> _gridUpdateEventQueue;

    private void Awake() {
        Instance = this;
        _initialized = false;
        _gridUpdateEventQueue = new Queue<GridUpdateEvent>();
    }

    IEnumerator Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            _view.RPC("UpdateRoomPropertiesRPC", RpcTarget.MasterClient);
            // Wait until the properties have been refreshed by Host
            while (! (bool) PhotonNetwork.CurrentRoom.CustomProperties["UpToDate"]) yield return new WaitForSeconds(0.1f);
            _view.RPC("SetRoomPropertiesOutOfDateRPC", RpcTarget.MasterClient);
        }
        else
        {
            Rows = (int) PhotonNetwork.CurrentRoom.CustomProperties["Rows"];
            Columns = (int) PhotonNetwork.CurrentRoom.CustomProperties["Columns"];
            MineProbability = (float) PhotonNetwork.CurrentRoom.CustomProperties["MineProbability"];
        }

        InitializeGrid();

        _initialized = true;

        if (!PhotonNetwork.IsMasterClient) ApplyGridUpdatesReceivedDuringInit();
    }

    private void InitializeGrid()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!PhotonNetwork.IsMasterClient)
        {
            Columns = (int) properties["Columns"];
            Rows = (int) properties["Rows"];
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
                cell.Column = col;
                cell.Row = row;

                if (PhotonNetwork.IsMasterClient)
                {
                    // Make the outer edges into borders
                    if (col == 0 || col == Columns - 1 || row == 0 || row == Rows - 1) {
                        cell.CurrentSprite = 10;
                    }
                    else {
                        // Plant mines randomly, but not in the middle
                        if (Mathf.Abs(col - colMidpoint) >= 2.1 || Mathf.Abs(row - rowMidpoint) >= 2.1) {
                            if (Random.value < MineProbability) {
                                cell.IsMine = true;
                            }
                        }
                    }
                }
                else // If not Host
                {
                    byte gridStateIndicator = (byte) ((byte[]) properties["GridState_Column" + col])[row];
                    cell.CurrentSprite = gridStateIndicator;
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

    /// <summary>
    /// Applies any updates to the grid that were received during Initialization 
    /// </summary>
    private void ApplyGridUpdatesReceivedDuringInit()
    {
        while (_gridUpdateEventQueue.Count != 0)
        {
            GridUpdateEvent gridUpdateEvent = _gridUpdateEventQueue.Dequeue();
            CellGrid[gridUpdateEvent.Column][gridUpdateEvent.Row].CurrentSprite = gridUpdateEvent.CellSprite;
        }
    }

    [PunRPC]
    private IEnumerator UpdateRoomPropertiesRPC()
    {
        while (!_initialized) yield return new WaitForSeconds(0.1f);

        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!properties.TryAdd("Columns", Columns)) properties["Columns"] = Columns;
        if (!properties.TryAdd("Rows", Rows)) properties["Rows"] = Rows;

        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Cell cell = CellGrid[col][row];

                if (cell.IsMine)
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

        if (!properties.TryAdd("UpToDate", true)) properties["UpToDate"] = true;

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    [PunRPC]
    private void SetRoomPropertiesOutOfDateRPC()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!properties.TryAdd("UpToDate", false)) properties["UpToDate"] = false;

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void SetCurrentSprite(int col, int row, byte value)
    {
        _view.RPC("SetCurrentSpriteRPC", RpcTarget.All, col, row, value);
    }

    [PunRPC]
    void SetCurrentSpriteRPC(int col, int row, byte value)
    {
        if (PhotonNetwork.IsMasterClient || _initialized) CellGrid[col][row].CurrentSprite = value;
        else _gridUpdateEventQueue.Enqueue(new GridUpdateEvent(col, row, value));
    }

    public void HandleCellShotEvent(int col, int row, byte eventType)
    {
        _view.RPC("HandleCellShotEventRPC", RpcTarget.All, col, row, eventType);
        if (eventType == 4 && Random.Range(0f, 1f) < PowerupProbability) _view.RPC("DisplayPowerupRPC", RpcTarget.All, col, row);
    }

    [PunRPC]
    void HandleCellShotEventRPC(int col, int row, byte eventType)
    {
        if (PhotonNetwork.IsMasterClient || _initialized) CellGrid[col][row].DisplayEffectsOnCellShotEvent(eventType);
    }

    [PunRPC]
    void DisplayPowerupRPC(int col, int row)
    {
        Transform tf = CellGrid[col][row].transform;
        Instantiate(PowerupBombPrefab, tf.position, tf.rotation);
    }

    public void PlantBomb(int playerID, Vector3 position, Quaternion rotation)
    {
        _view.RPC("PlantBombRPC", RpcTarget.All, playerID, position, rotation);
    }

    [PunRPC]
    void PlantBombRPC(int playerID, Vector3 position, Quaternion rotation)
    {
        Bomb bomb = GameObject.Instantiate(BombPrefab, position, rotation);
        bomb.BeneficiaryID = playerID;
    }
}
