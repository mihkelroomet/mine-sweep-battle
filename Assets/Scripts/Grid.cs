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
    public float PowerupSpawnProbability;
    public Bomb BombPrefab;
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

        // If master, open cells in the middle of the grid
        if (PhotonNetwork.IsMasterClient)
        {
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

        // If not master, instantiate powerups fetched from master
        if (!PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < (int) properties["NumberOfPowerups"]; i++)
            {
                int col = (int) properties["Powerup" + i + "Column"];
                int row = (int) properties["Powerup" + i + "Row"];
                byte type = (byte) properties["Powerup" + i + "Type"];
                float expiresAt = (float) properties["Powerup" + i + "ExpiresAt"];
                PowerupSpawner.Instance.SpawnPowerup(col, row, type, expiresAt);
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
            if (gridUpdateEvent.CellSprite == 255) CellGrid[gridUpdateEvent.Column][gridUpdateEvent.Row].CurrentSprite--;
            else CellGrid[gridUpdateEvent.Column][gridUpdateEvent.Row].CurrentSprite = gridUpdateEvent.CellSprite;
        }
    }

    [PunRPC]
    private IEnumerator UpdateRoomPropertiesRPC()
    {
        while (!_initialized) yield return new WaitForSeconds(0.1f);

        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!properties.TryAdd("Columns", Columns)) properties["Columns"] = Columns;
        if (!properties.TryAdd("Rows", Rows)) properties["Rows"] = Rows;

        // Cell sprites
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

        // Powerups
        int numberOfPowerups = PowerupSpawner.Instance.transform.childCount;
        if (!properties.TryAdd("NumberOfPowerups", numberOfPowerups)) properties["NumberOfPowerups"] = numberOfPowerups;
        for (int i = 0; i < numberOfPowerups; i++)
        {
            CollectablePowerup powerup = PowerupSpawner.Instance.transform.GetChild(i).GetComponent<CollectablePowerup>();
            if (!properties.TryAdd("Powerup" + i + "Column", powerup.Column)) properties["Powerup" + i + "Column"] = powerup.Column;
            if (!properties.TryAdd("Powerup" + i + "Row", powerup.Row)) properties["Powerup" + i + "Row"] = powerup.Row;
            if (!properties.TryAdd("Powerup" + i + "Type", (byte) powerup.Data.Type)) properties["Powerup" + i + "Type"] = (byte) powerup.Data.Type;
            if (!properties.TryAdd("Powerup" + i + "ExpiresAt", powerup.ExpiresAt)) properties["Powerup" + i + "ExpiresAt"] = powerup.ExpiresAt;
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
        if (PhotonNetwork.IsMasterClient || _initialized)
        {
            if (value == 255) CellGrid[col][row].CurrentSprite--;
            else CellGrid[col][row].CurrentSprite = value;
        }
        else _gridUpdateEventQueue.Enqueue(new GridUpdateEvent(col, row, value));
    }

    public void HandleCellShotEvent(int col, int row, byte eventType)
    {
        _view.RPC("HandleCellShotEventRPC", RpcTarget.All, col, row, eventType);
        // Chance to spawn powerup if empty cell is opened correctly
        if (eventType == 4 && Random.Range(0f, 1f) < PowerupSpawnProbability)
        {
            byte type = PowerupSpawner.ChooseRandomPowerup(PowerupSpawner.Instance.Powerups);
            _view.RPC("SpawnPowerupRPC", RpcTarget.All, col, row, type);
        }
    }

    [PunRPC]
    void HandleCellShotEventRPC(int col, int row, byte eventType)
    {
        if (PhotonNetwork.IsMasterClient || _initialized) CellGrid[col][row].DisplayEffectsOnCellShotEvent(eventType);
    }

    [PunRPC]
    void SpawnPowerupRPC(int col, int row, byte type)
    {
        if (PhotonNetwork.IsMasterClient || _initialized) PowerupSpawner.Instance.SpawnPowerup(col, row, type);
    }

    public void DestroyCollectablePowerup(int col, int row, byte type)
    {
        _view.RPC("DestroyCollectablePowerupRPC", RpcTarget.All, col, row, type);
    }

    [PunRPC]
    void DestroyCollectablePowerupRPC(int col, int row, byte type)
    {
        for (int i = 0; i < PowerupSpawner.Instance.transform.childCount; i++)
        {
            CollectablePowerup powerup = PowerupSpawner.Instance.transform.GetChild(i).GetComponent<CollectablePowerup>();
            if (powerup.Column == col && powerup.Row == row && (byte) powerup.Data.Type == type)
            {
                Destroy(powerup.gameObject);
            }
        }
    }
}
