using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject PlayerPrefab;

    void Start()
    {
        PhotonNetwork.Instantiate(PlayerPrefab.name, Vector2.zero, Quaternion.identity);
    }
}
