using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject PlayerPrefab;

    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;

    void Start()
    {
        Vector2 randomPos = new Vector2(Random.Range(MinX, MaxX), Random.Range(MinY, MaxY));
        PhotonNetwork.Instantiate(PlayerPrefab.name, Vector2.zero, Quaternion.identity);
    }
}
