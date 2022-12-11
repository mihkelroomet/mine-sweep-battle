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
    [SerializeField]
    private float _bombProbability = 0.25f;
    [SerializeField]
    private int _roundLength = 60;

    private void Awake() {
        PracticeButton.onClick.AddListener(() => CreatePracticeRoom());
        CreateButton.onClick.AddListener(() => CreateRoom());
        JoinButton.onClick.AddListener(() => JoinRoom());
        QuitButton.onClick.AddListener(() => QuitGame());
        Transitions.Instance.PlayEnterTransition();
    }

    public void CreatePracticeRoom()
    {
        CreateRoom(50, 50, 1, 0.25f, 60, "Trainee", Random.Range(0, 1_000_000).ToString());
    }

    public void CreateRoom()
    {
        CreateRoom((int) RowSlider.value, (int) ColumnSlider.value, _maxPlayers, _bombProbability, _roundLength, CreateNameInputField.text, CreateInputField.text);
    }

    public void CreateRoom(int rows, int columns, byte maxPlayers, float bombProbability, int roundLength, string playerName, string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps.Add("Rows", (int) rows);
        roomProps.Add("Columns", (int) columns);
        roomProps.Add("BombProbability", (float) bombProbability);
        roomProps.Add("RoundLength", (int) roundLength);
        roomProps.Add("UpToDate", false);
        roomProps.Add("TimeLeftUpToDate", false);
        roomOptions.CustomRoomProperties = roomProps;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        PhotonNetwork.LocalPlayer.NickName = playerName;
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(JoinInputField.text);
        string playerName = JoinNameInputField.text;
        PhotonNetwork.LocalPlayer.NickName = playerName.Equals("") ? "Player" : playerName;
    }

    public override void OnJoinedRoom()
	{
        PhotonNetwork.LoadLevel("In-Game");
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
