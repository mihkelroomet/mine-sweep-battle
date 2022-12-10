using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

	public override void OnJoinedLobby()
	{
        LevelLoader.Instance.Transition.GetComponent<Image>().enabled=true;
        LevelLoader.Instance.LoadLevel("MainMenu");
	}
}
