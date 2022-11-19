using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField CreateInput;
    public Button CreateButton;
    public TMP_InputField JoinInput;
    public Button JoinButton;

    private void Awake() {
        CreateButton.onClick.AddListener(() => CreateRoom());
        JoinButton.onClick.AddListener(() => JoinRoom());
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(CreateInput.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(JoinInput.text);
    }

	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("In-Game");
	}
}
