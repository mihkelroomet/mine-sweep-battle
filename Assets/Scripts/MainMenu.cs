using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class MainMenu : MonoBehaviourPunCallbacks
{
    // RoomListing
    public RoomListing roomListingPrefab;
    public Transform Content;
    private List<RoomListing> _listings = new List<RoomListing>();
    private RoomListing _selectedRoom;

    // Input Fields
    public TMP_InputField NameInputField;
    public TMP_InputField CreateRoomNameInputField;
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

    private void Awake() {
        PracticeButton.onClick.AddListener(() => CreatePracticeRoom());
        CreateButton.onClick.AddListener(() => CreateRoom());
        JoinButton.onClick.AddListener(() => JoinRoom());
        QuitButton.onClick.AddListener(() => QuitGame());
        Transitions.Instance.PlayEnterTransition();

        NameInputField.text = Events.GetPlayerName();

        MusicPlayer.Instance.PlayMusic(MainMenuMusic);
        MusicVolumeSlider.value = Events.GetMusicVolume();
        SFXVolumeSlider.value = Events.GetSFXVolume();
    }

    private void Start()
    {
        List<RoomInfo> StartingRooms = ConnectToServer.Instance.RoomList;
        foreach (RoomInfo info in StartingRooms)
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

    public void CreatePracticeRoom()
    {
        CreateRoom(50, 50, 1, 0.3f, 90, "Trainee", Random.Range(0, 1_000_000).ToString(), false);
    }

    public void CreateRoom()
    {
        CreateRoom((int)RowsSlider.value, (int)ColumnsSlider.value, (byte)MaxPlayersSlider.value, MineFrequencySlider.value / 10, (int)RoundLengthSlider.value, NameInputField.text, CreateRoomNameInputField.text, true);
    }

    public void CreateRoom(int rows, int columns, byte maxPlayers, float mineProbability, int roundLength, string playerName, string roomName, bool isVisible)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = isVisible;
        roomOptions.MaxPlayers = maxPlayers;
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps.Add("Rows", (int)rows);
        roomProps.Add("Columns", (int)columns);
        roomProps.Add("MineProbability", (float)mineProbability);
        roomProps.Add("RoundLength", (int)roundLength);
        roomProps.Add("UpToDate", false);
        roomProps.Add("TimeLeftUpToDate", false);
        roomOptions.CustomRoomProperties = roomProps;
        string[] lobbyProperties = { "Rows", "Columns", "MineProbability", "RoundLength" }; // Properties needed to show on room display
        roomOptions.CustomRoomPropertiesForLobby = lobbyProperties;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(_selectedRoom.RoomInfo.Name);
    }

    public override void OnJoinedRoom()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        int hatColor = Events.GetHatColor();
        int shirtColor = Events.GetShirtColor();
        int pantsColor = Events.GetPantsColor();
        if (!properties.TryAdd("HatColor", hatColor)) properties["HatColor"] = hatColor;
        if (!properties.TryAdd("ShirtColor", shirtColor)) properties["ShirtColor"] = shirtColor;
        if (!properties.TryAdd("PantsColor", pantsColor)) properties["PantsColor"] = pantsColor;
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
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
        _selectedRoom = room;
    }

    public void SetRows(float rows)
    {
        RowsInputField.text = Mathf.Clamp(rows, 10, 100).ToString();
    }

    public void SetColumns(float columns)
    {
        ColumnsInputField.text = Mathf.Clamp(columns, 10, 100).ToString();
    }
    public void SetMaxPlayers(float maxPlayers)
    {
        MaxPlayersInputField.text = Mathf.Clamp(maxPlayers, 1, 20).ToString();
    }

    public void SetMineFrequency(float mineFrequency)
    {
        MineFrequencyInputField.text = Mathf.Clamp(mineFrequency, 1, 6).ToString();
    }
    public void SetRoundLength(float roundLength)
    {
        RoundLengthInputField.text = Mathf.Clamp(roundLength, 15, 300).ToString();
    }


    public void EndEditRows(string rows)
    {
        float v = 10f;
        float.TryParse(rows, out v);
        SetRows((int)v);
        RowsSlider.value = v;
    }

    public void EndEditColumns(string columns)
    {
        float v = 10f;
        float.TryParse(columns, out v);
        SetColumns((int)v);
        ColumnsSlider.value = v;
    }
    public void EndEditMaxPlayers(string maxPlayer)
    {
        float v = 10f;
        float.TryParse(maxPlayer, out v);
        SetMaxPlayers((int)v);
        MaxPlayersSlider.value = v;
    }
    public void EndEditMineFrequency(string mineFrequency)
    {
        float v = 1f;
        float.TryParse(mineFrequency, out v);
        SetMineFrequency((int)v);
        MineFrequencySlider.value = v;
    }
    public void EndEditRoundLength(string roundLength)
    {
        float v = 10f;
        float.TryParse(roundLength, out v);
        SetRoundLength((int)v);
        RoundLengthSlider.value = v;
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
