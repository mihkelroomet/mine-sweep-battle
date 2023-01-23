using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Lobby : MonoBehaviourPunCallbacks
{
    public static Lobby Instance;

    // Components
    [SerializeField] private PhotonView _view;

    // Lobby Panel
    [SerializeField] private TMP_Text RoomNameText;
    [SerializeField] private TMP_Text NumberOfPlayersText;
    [SerializeField] private GameObject InputColumn;
    [SerializeField] private Button StartButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private PlayerListing PlayerListingPrefab;
    [SerializeField] private Transform Content;
    private List<PlayerListing> _playerListings;

    // Lobby Inputs
    [SerializeField] private Slider RowsSlider;
    [SerializeField] private TMP_InputField RowsInputField;
    [SerializeField] private Button RowsSliderHandle;
    [SerializeField] private Slider ColumnsSlider;
    [SerializeField] private TMP_InputField ColumnsInputField;
    [SerializeField] private Button ColumnsSliderHandle;
    [SerializeField] private Slider MineFrequencySlider;
    [SerializeField] private TMP_InputField MineFrequencyInputField;
    [SerializeField] private Button MineFrequencySliderHandle;
    [SerializeField] private Slider RoundLengthSlider;
    [SerializeField] private TMP_InputField RoundLengthInputField;
    [SerializeField] private Button RoundLengthSliderHandle;
    private Slider[] _sliders;
    private TMP_InputField[] _inputFields;
    private Button[] _sliderHandles;
    
    // Audio
    public AudioClip LobbyMusic;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!roomProperties.TryAdd("CurrentScene", "Lobby")) roomProperties["CurrentScene"] = "Lobby";
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        InitializePlayerList();
        
        _sliders = new Slider[] { RowsSlider, ColumnsSlider, MineFrequencySlider, RoundLengthSlider };
        _inputFields = new TMP_InputField[] { RowsInputField, ColumnsInputField, MineFrequencyInputField, RoundLengthInputField };
        _sliderHandles = new Button[] { RowsSliderHandle, ColumnsSliderHandle, MineFrequencySliderHandle, RoundLengthSliderHandle };

        RoomNameText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;

        InitializeInputs();
        EnableInputs(PhotonNetwork.IsMasterClient);

        LeaveButton.onClick.AddListener(() => BackToMainMenu());
        StartButton.onClick.AddListener(() => StartGame());

        MusicPlayer.Instance.PlayMusic(LobbyMusic);

        Transitions.Instance.PlayEnterTransition();
    }

    private void InitializePlayerList()
    {
        _playerListings = new List<PlayerListing>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerListing listing = Instantiate(PlayerListingPrefab, Content);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                if (player.IsMasterClient)
                {
                    listing.HostIcon.gameObject.SetActive(true);
                }
            }
            _playerListings.Add(listing);
        }

        UpdatePlayerCount();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerListing listing = Instantiate(PlayerListingPrefab, Content);
        if (listing != null)
        {
            listing.SetPlayerInfo(newPlayer);
            _playerListings.Add(listing);
        }
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player leftPlayer)
    {
        int index = _playerListings.FindIndex(x => x.Player == leftPlayer);
        if (index != -1)
        {
            Destroy(_playerListings[index].gameObject);
            _playerListings.RemoveAt(index);
        }
        foreach (Player player in PhotonNetwork.PlayerList) // Find the host again in case the previous host left
        {
            if (player.IsMasterClient)
            {
                int hostIndex = _playerListings.FindIndex(x => x.Player == player);
                _playerListings[hostIndex].HostIcon.gameObject.SetActive(true);
            }
                
        }

        EnableInputs(PhotonNetwork.IsMasterClient); // In case the master client left and you are the new one, enable the start button and options
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        NumberOfPlayersText.text = "Players: " + _playerListings.Count.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    private void InitializeInputs()
    {
        int rows = Mathf.Clamp((int) PhotonNetwork.CurrentRoom.CustomProperties["Rows"], 10, 100);
        int rowsSliderValue = rows / 10;
        int columns = Mathf.Clamp((int) PhotonNetwork.CurrentRoom.CustomProperties["Columns"], 10, 100);
        int columnsSliderValue = columns / 10;
        float mineFrequency = Mathf.Clamp((float) PhotonNetwork.CurrentRoom.CustomProperties["MineFrequency"], 0.1f, 0.6f);
        int mineFrequencySliderValue = (int) Mathf.Round(mineFrequency * 10);
        int roundLength = Mathf.Clamp((int) PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"], 15, 300);
        int roundLengthSliderValue = roundLength / 15;

        UpdateInputRPC(0, rowsSliderValue, rows);
        UpdateInputRPC(1, columnsSliderValue, columns);
        UpdateInputRPC(2, mineFrequencySliderValue, mineFrequencySliderValue);
        UpdateInputRPC(3, roundLengthSliderValue, roundLength);
    }

    public void SetRows(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int rowsSliderValue = (int) Mathf.Clamp(value, 1, 10);
            int rows = rowsSliderValue * 10;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("Rows", rows)) properties["Rows"] = rows;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            RowsInputField.text = rows.ToString();
            _view.RPC("UpdateInputRPC", RpcTarget.All, 0, rowsSliderValue, rows);
        }
    }

    public void SetColumns(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int columnsSliderValue = (int) Mathf.Clamp(value, 1, 10);
            int columns = columnsSliderValue * 10;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("Columns", columns)) properties["Columns"] = columns;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            ColumnsInputField.text = columns.ToString();
            _view.RPC("UpdateInputRPC", RpcTarget.All, 1, columnsSliderValue, columns);
        }
    }

    public void SetMineFrequency(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int mineFrequencySliderValue = (int) Mathf.Clamp(value, 1, 6);
            float mineFrequency = ((float) mineFrequencySliderValue) / 10;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("MineFrequency", mineFrequency)) properties["MineFrequency"] = mineFrequency;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            _view.RPC("UpdateInputRPC", RpcTarget.All, 2, mineFrequencySliderValue, mineFrequencySliderValue);
        }
    }

    public void SetRoundLength(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int roundLengthSliderValue = (int) Mathf.Clamp(value, 1, 20);
            int roundLength = roundLengthSliderValue * 15;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("RoundLength", roundLength)) properties["RoundLength"] = roundLength;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            _view.RPC("UpdateInputRPC", RpcTarget.All, 3, roundLengthSliderValue, roundLength);
        }
    }

    // Update slider and input values (0: Rows, 1: Columns, 2: MineFrequency, 3: RoundLength)
    [PunRPC]
    private void UpdateInputRPC(int whichOne, int silderValue, int inputFieldValue)
    {
        _sliders[whichOne].value = silderValue;
        _inputFields[whichOne].text = inputFieldValue.ToString();
    }

    private void EnableInputs(bool enable)
    {
        foreach (Slider slider in _sliders) slider.interactable = enable;
        foreach (TMP_InputField inputField in _inputFields) inputField.interactable = enable;
        foreach (Button sliderHandle in _sliderHandles) sliderHandle.interactable = enable;
        StartButton.interactable = enable;
    }

    public void BackToMainMenu()
    {
        StartCoroutine(BackToMainMenuCoroutine());
    }

    IEnumerator BackToMainMenuCoroutine()
    {
        yield return Transitions.Instance.ExitSceneWithTransitionCoroutine("MainMenu");
        PhotonNetwork.LeaveRoom();
    }

    public void StartGame()
    {
        _view.RPC("StartGameRPC", RpcTarget.All);
    }

    [PunRPC]
    void StartGameRPC()
    {
        PhotonNetwork.LoadLevel("In-Game");
    }
}
