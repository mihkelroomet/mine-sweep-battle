using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public static MainMenu Instance;

    // RoomListing
    public RoomListing roomListingPrefab;
    public Transform Content;
    private List<RoomListing> _listings;
    private RoomListing _selectedRoom;

    // Input Fields
    public TMP_InputField NameInputField;
    public TMP_InputField CreateRoomNameInputField;
    public int RoomNameMaxCharacters;
    public TMP_InputField RowsInputField;
    public Slider RowsSlider;
    public TMP_InputField ColumnsInputField;
    public Slider ColumnsSlider;
    public TMP_InputField MaxPlayersInputField;
    public Slider MaxPlayersSlider;
    public TMP_InputField MineFrequencyInputField;
    public Slider MineFrequencySlider;
    public TMP_InputField RoundLengthInputField;
    public Slider RoundLengthSlider;

    // Buttons
    public Button PracticeButton;
    public Button CreateButton;
    public Button JoinButton;
    public Button QuitButton;

    // Audio
    public AudioClip MainMenuMusic;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        Transitions.Instance.PlayEnterTransition();

        PracticeButton.onClick.AddListener(() => CreatePracticeRoom());
        CreateButton.onClick.AddListener(() => CreateRoom());
        JoinButton.onClick.AddListener(() => JoinRoom());
        QuitButton.onClick.AddListener(() => QuitGame());

        NameInputField.text = Events.GetPlayerName();
        CreateRoomNameInputField.text = GetRandomRoomName();

        MusicPlayer.Instance.PlayMusic(MainMenuMusic);
        MusicVolumeSlider.value = Events.GetMusicVolume();
        SFXVolumeSlider.value = Events.GetSFXVolume();
    }

    private void Start()
    {
        _listings = new List<RoomListing>();
        foreach (RoomListing roomListing in Content.GetComponentsInChildren<RoomListing>()) Destroy(roomListing.gameObject); // This is to keep example listing visible in editor
        UpdateRoomList(ConnectToServer.Instance.RoomList);
    }

    public void CreatePracticeRoom()
    {
        CreateRoom(50, 50, 1, 0.3f, 90, "Trainee", GetRandomRoomName(), false);
    }

    public string GetRandomRoomName()
    {
        string randomRoomName = "Room" + Random.Range(1_000, 9_999);
        while (RoomExists(randomRoomName)) randomRoomName = "Room" + Random.Range(1_000, 9_999);
        return randomRoomName;
    }

    public bool RoomExists(string roomName)
    {
        foreach (RoomInfo roomInfo in ConnectToServer.Instance.RoomList)
        {
            if (roomInfo.Name == roomName) return true;
        }
        return false;
    }

    public void CreateRoom()
    {
        CreateRoom(
            (int) RowsSlider.value * 10, (int) ColumnsSlider.value * 10, (byte) (MaxPlayersSlider.value * 2), MineFrequencySlider.value / 10,
            (int) RoundLengthSlider.value * 15, NameInputField.text, CreateRoomNameInputField.text, true
            );
    }

    public void CreateRoom(int rows, int columns, byte maxPlayers, float mineFrequency, int roundLength, string playerName, string roomName, bool isVisible)
    {
        if (roomName.Length > RoomNameMaxCharacters) roomName = roomName.Substring(0, RoomNameMaxCharacters); // Make sure room name is a set max characters
        if (roomName.Length < 1 || RoomExists(roomName)) roomName = GetRandomRoomName();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = isVisible;
        roomOptions.MaxPlayers = maxPlayers;
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps.Add("Rows", (int) rows);
        roomProps.Add("Columns", (int) columns);
        roomProps.Add("MineFrequency", (float) mineFrequency);
        roomProps.Add("RoundLength", (int) roundLength);
        roomProps.Add("UpToDate", false);
        roomProps.Add("CountdownTimeLeftValid", false);
        roomProps.Add("TimeLeftUpToDate", false);
        string currentScene = isVisible ? "Lobby" : "In-Game"; // Skip lobby if practicing
        roomProps.Add("CurrentScene", currentScene);
        roomOptions.CustomRoomProperties = roomProps;
        string[] lobbyProperties = { "Rows", "Columns", "MineFrequency", "RoundLength" }; // Properties needed to show on room display
        roomOptions.CustomRoomPropertiesForLobby = lobbyProperties;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(_selectedRoom.RoomInfo.Name);
    }

    public override void OnJoinedRoom()
    {
        Transitions.Instance.PlayExitEmptyTransition();
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        int hatColor = Events.GetHatColor();
        int shirtColor = Events.GetShirtColor();
        int pantsColor = Events.GetPantsColor();
        int bootsColor = Events.GetBootsColor();
        if (!properties.TryAdd("HatColor", hatColor)) properties["HatColor"] = hatColor;
        if (!properties.TryAdd("ShirtColor", shirtColor)) properties["ShirtColor"] = shirtColor;
        if (!properties.TryAdd("PantsColor", pantsColor)) properties["PantsColor"] = pantsColor;
        if (!properties.TryAdd("BootsColor", bootsColor)) properties["BootsColor"] = bootsColor;
        if (!properties.TryAdd("Score", 0)) properties["Score"] = 0;
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        PhotonNetwork.LoadLevel((string) PhotonNetwork.CurrentRoom.CustomProperties["CurrentScene"]);
    }

    private void SaveColorsAsCustomProperties()
    {
    }

    // Join the lobby again after exiting a game, so the room list works
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList) // On room remove
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(_listings[index].gameObject);
                    _listings.RemoveAt(index);
                }
            }
            else
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index != -1) // On room update
                {
                    _listings[index].SetRoomInfo(info);
                }
                else // On room add
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
        _selectedRoom = room;
    }

    public void SetRows(float rows)
    {
        RowsInputField.text = (Mathf.Clamp(rows, 1, 10) * 10).ToString();
    }

    public void SetColumns(float columns)
    {
        ColumnsInputField.text = (Mathf.Clamp(columns, 1, 10) * 10).ToString();
    }
    public void SetMaxPlayers(float maxPlayers)
    {
        MaxPlayersInputField.text = (Mathf.Clamp(maxPlayers, 1, 10) * 2).ToString();
    }

    public void SetMineFrequency(float mineFrequency)
    {
        MineFrequencyInputField.text = Mathf.Clamp(mineFrequency, 1, 6).ToString();
    }
    public void SetRoundLength(float roundLength)
    {
        RoundLengthInputField.text = (Mathf.Clamp(roundLength, 1, 20) * 15).ToString();
    }


    public void SetPlayerName(string value)
    {
        Events.SetPlayerName(value);
        PhotonNetwork.LocalPlayer.NickName = Events.GetPlayerName();
    }

    public void SetMusicVolume(float value)
    {
        Events.SetMusicVolume(Mathf.Clamp((int) Mathf.Round(value), 0, 10));
    }

    public void SetSFXVolume(float value)
    {
        Events.SetSFXVolume(Mathf.Clamp((int) Mathf.Round(value), 0, 10));
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
