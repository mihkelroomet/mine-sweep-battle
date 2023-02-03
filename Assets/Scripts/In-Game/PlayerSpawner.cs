using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject PlayerPrefab;

    private void Awake()
    {
        // The player needs to exist by the end of awake for other scripts, so we spawn them to a far away location initially
        PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(1000, 1000, 0), Quaternion.identity);
    }

    IEnumerator Start()
    {
        while (!Grid.Instance.Initialized) yield return new WaitForSeconds(0.05f); // Waiting for grid init to complete

        // Making a 4x4 clearance for the player in a random spot on the grid that leaves at least two cells unopened on each side
        int bottomLeftCol = Random.Range(3, Grid.Instance.Columns - 6);
        int bottomLeftRow = Random.Range(3, Grid.Instance.Rows - 6);
        Grid.Instance.OpenCellsForSpawningPlayer(bottomLeftCol, bottomLeftRow);

        // Setting the player's position to the middle of that clearance
        Cell midBottomLeftCell = Grid.Instance.CellGrid[bottomLeftCol + 1][bottomLeftRow + 1];
        float halfOfCellWidth = midBottomLeftCell.GetComponent<Renderer>().bounds.size.x / 2;
        Vector3 playerStartingPos = midBottomLeftCell.transform.position + new Vector3(halfOfCellWidth, halfOfCellWidth, 0);
        PlayerController.Instance.transform.position = playerStartingPos;
    }
}
