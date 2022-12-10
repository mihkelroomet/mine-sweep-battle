using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public Button PracticeButton;
    public TMP_InputField CreateNameInputField;
    public TMP_InputField CreateInputField;

    public TMP_InputField RowCountField;
    public Slider RowSlider;
    public TMP_InputField ColumnCountField;
    public Slider ColumnSlider;

    public Button CreateButton;

    public TMP_InputField JoinNameInputField;
    public TMP_InputField JoinInputField;
    public Button JoinButton;
    public Button QuitButton;

    [SerializeField]
    private byte _maxPlayers = 10;

    private void Awake() {
        PracticeButton.onClick.AddListener(() => CreatePracticeRoom());
        CreateButton.onClick.AddListener(() => CreateRoom());
        JoinButton.onClick.AddListener(() => JoinRoom());
        QuitButton.onClick.AddListener(() => QuitGame());
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = _maxPlayers;
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps.Add("UpToDate", false);
        roomProps.Add("Rows", (int) RowSlider.value);
        roomProps.Add("Columns", (int) ColumnSlider.value);
        roomOptions.CustomRoomProperties = roomProps;
        PhotonNetwork.CreateRoom(CreateInputField.text, roomOptions);
        string playerName = CreateNameInputField.text;
        PhotonNetwork.LocalPlayer.NickName = playerName.Equals("") ? "Player" : playerName;
    }

    public void CreatePracticeRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;
        PhotonNetwork.CreateRoom(Random.Range(0, 1_000_000).ToString(), roomOptions);
        PhotonNetwork.LocalPlayer.NickName = "Trainee";
    }

    public void JoinRoom()
    {
        LevelLoader.Instance.PlayTransition();
        PhotonNetwork.JoinRoom(JoinInputField.text);
        string playerName = JoinNameInputField.text;
        PhotonNetwork.LocalPlayer.NickName = playerName.Equals("") ? "Player" : playerName;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LevelLoader.Instance.ExitTransition();
    }
    public override void OnJoinedRoom()
	{
        PhotonNetwork.LoadLevel("In-Game");
        //LevelLoader.Instance.LoadLevelPhoton("In-Game");
	}

    public void SetRows(float rows)
    {
        RowCountField.text = Mathf.Clamp(rows, 10, 100).ToString();
    }

    public void SetColumns(float columns)
    {
        ColumnCountField.text = Mathf.Clamp(columns, 10, 100).ToString();
    }

    public void EndEditRows(string rows)
    {
        float v = 10f;
        float.TryParse(rows, out v);
        SetRows((int)v);
        RowSlider.value = v;
    }

    public void EndEditColumns(string columns)
    {
        float v = 10f;
        float.TryParse(columns, out v);
        SetColumns((int)v);
        ColumnSlider.value = v;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
