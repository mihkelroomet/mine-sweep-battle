using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public Button PracticeButton;
    public TMP_InputField CreateNameInputField;
    public TMP_InputField CreateInputField;

    public RoomListing roomListingPrefab;
    public Transform Content;

    private List<RoomListing> _listings = new List<RoomListing>();

    public TMP_InputField RowCountField;
    public Slider RowSlider;
    public TMP_InputField ColumnCountField;
    public Slider ColumnSlider;
    public TMP_InputField MaxPlayerCountField;
    public Slider MaxPlayerSlider;
    public TMP_InputField BombFrequencyCountField;
    public Slider BombFrequencySlider;
    public TMP_InputField RoundLengthCountField;
    public Slider RoundLengthSlider;

    public Button CreateButton;

    public TMP_InputField JoinNameInputField;
    public TMP_InputField JoinInputField;
    public Button JoinButton;
    public Button QuitButton;

    [SerializeField]
    private byte _maxPlayers = 10;
    [SerializeField]
    private float _mineProbability = 0.25f;
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
        CreateRoom((int) RowSlider.value, (int) ColumnSlider.value, (byte)MaxPlayerSlider.value,BombFrequencySlider.value/10,(int) RoundLengthSlider.value, CreateNameInputField.text, CreateInputField.text);
    }

    public void CreateRoom(int rows, int columns, byte maxPlayers, float mineProbability, int roundLength, string playerName, string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps.Add("Rows", (int) rows);
        roomProps.Add("Columns", (int) columns);
        roomProps.Add("MineProbability", (float) mineProbability);
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

    // Join the lobby again after exiting a game, so the room list works
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(_listings[index].gameObject);
                    _listings.RemoveAt(index);
                }
            }
            //Added to rooms list
            else
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    _listings[index].SetRoomInfo(info);
                }
                else
                {
                    RoomListing listing = Instantiate(roomListingPrefab, Content);
                    Button listingButton = listing.GetComponent<Button>();
                    listingButton.onClick.AddListener(() => SelectRoom(listing));
                    if (listing != null)
                    {
                        listing.SetRoomInfo(info);
                        _listings.Add(listing);
                    }
                }
            }
        }
    }

    public void SelectRoom(RoomListing room)
    {
        JoinInputField.text = room.RoomInfo.Name;
    }

    public void SetRows(float rows)
    {
        RowCountField.text = Mathf.Clamp(rows, 10, 100).ToString();
    }

    public void SetColumns(float columns)
    {
        ColumnCountField.text = Mathf.Clamp(columns, 10, 100).ToString();
    }
    public void SetMaxPlayer(float maxPlayer)
    {
        MaxPlayerCountField.text = Mathf.Clamp(maxPlayer, 1, 20).ToString();
    }

    public void SetBombFrequency(float bombFrequency)
    {
        BombFrequencyCountField.text = Mathf.Clamp(bombFrequency, 1, 6).ToString();
    }
    public void SetRoundLength(float roundLength)
    {
        RoundLengthCountField.text = Mathf.Clamp(roundLength, 10, 100).ToString();
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
    public void EndMaxPlayer(string maxPlayer)
    {
        float v = 10f;
        float.TryParse(maxPlayer, out v);
        SetMaxPlayer((int)v);
        MaxPlayerSlider.value = v;
    }
    public void EndBombFrequency(string bombFrequency)
    {
        float v = 1f;
        float.TryParse(bombFrequency, out v);
        SetBombFrequency((int)v);
        BombFrequencySlider.value = v;
    }
    public void EndRoundLength(string roundLength)
    {
        float v = 10f;
        float.TryParse(roundLength, out v);
        SetRoundLength((int)v);
        RoundLengthSlider.value = v;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
