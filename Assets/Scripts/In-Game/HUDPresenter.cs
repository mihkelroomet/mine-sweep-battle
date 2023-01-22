using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class HUDPresenter : MonoBehaviourPunCallbacks
{
    public static HUDPresenter Instance;

    // Components
    [SerializeField] private PhotonView _view;

    // In-Game UI
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private GameObject ScoreBoard;
    [SerializeField] private GameObject EscMenu;
    [SerializeField] private TMP_Text FinalScores;
    [SerializeField] private Image FirstPowerupSlotImage;
    [SerializeField] private Image SecondPowerupSlotImage;
    [SerializeField] private AudioSource EndAudio;
    [SerializeField] private Button RestartButton;

    // Lobby Panel
    [SerializeField] private TMP_Text RoomName;
    [SerializeField] private TMP_Text PlayersText;
    [SerializeField] private GameObject Options;
    [SerializeField] private Button StartButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private PlayerListing PlayerListingPrefab;
    [SerializeField] private Transform Content;
    private List<PlayerListing> _playerListings = new List<PlayerListing>();

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


    private void Awake() {
        Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnSetPowerupInFirstSlot += SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot += SetPowerupInSecondSlot;
        Events.OnEndOfRound += ShowScoreboard;
        Transitions.Instance.PlayEnterTransition();
        LeaveButton.onClick.AddListener(() => GameController.Instance.BackToMainMenu());
        StartButton.onClick.AddListener(() => GameController.Instance.StartGame());

        _sliders = new Slider[] { RowsSlider, ColumnsSlider, MineFrequencySlider, RoundLengthSlider };
        _inputFields = new TMP_InputField[] { RowsInputField, ColumnsInputField, MineFrequencyInputField, RoundLengthInputField };
        _sliderHandles = new Button[] { RowsSliderHandle, ColumnsSliderHandle, MineFrequencySliderHandle, RoundLengthSliderHandle };

        RoomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        ChangeRowCount(((int) PhotonNetwork.CurrentRoom.CustomProperties["Rows"]) / 10);
        ChangeColumnCount(((int) PhotonNetwork.CurrentRoom.CustomProperties["Columns"]) / 10);
        ChangeMineFrequency(((float) PhotonNetwork.CurrentRoom.CustomProperties["MineFrequency"] * 10));
        ChangeRoundLength(((int) PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"]) / 15);

        //Options.SetActive(false);
        SetOptionsAvailable(false);
    }

    void Start()
    {
        SetOptionsAvailable(PhotonNetwork.IsMasterClient);
        ScoreBoard.SetActive(false);
        EscMenu.SetActive(false);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerListing listing = Instantiate(PlayerListingPrefab, Content);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                if (player.IsMasterClient)
                {
                    listing.HostIcon.gameObject.SetActive(true); ;
                }
            }
            _playerListings.Add(listing);
        }
        UpdatePlayerCount();
    }

    // Update timer text
    public void UpdateTimer(float time) {
        time += 0.99f; // This is so the round would end the moment it shows "0:00" which feels more natural
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        TimerText.text = minutes + ":" + (seconds < 10 ? "0" : "") + seconds;

        // Make last 5 seconds of a round red
        if (time < 6) {
            TimerText.color = Color.red;
        }
    }

    private void SetScore(int value)
    {
        ScoreText.text = "Score: " + value;
    }

    private void SetPowerupInFirstSlot(PowerupData data)
    {
        if (data == null) FirstPowerupSlotImage.color = Color.clear;
        else
        {
            FirstPowerupSlotImage.color = Color.white;
            FirstPowerupSlotImage.sprite = data.Pic64;
        }
    }

    private void SetPowerupInSecondSlot(PowerupData data)
    {
        if (data == null) SecondPowerupSlotImage.color = Color.clear;
        else
        {
            SecondPowerupSlotImage.color = Color.white;
            SecondPowerupSlotImage.sprite = data.Pic32;
        }
    }

    public void ShowScoreboard()
    {
        string finalScores = "";
        byte placing = 1;
        Player[] sortedPlayerList = PhotonNetwork.PlayerList.OrderByDescending(player => player.CustomProperties["Score"]).ToArray();

        foreach (Player player in sortedPlayerList)
        {
            finalScores += placing++ + "." + player.NickName + "\t\t\t" + player.CustomProperties["Score"] + "\n";
        }

        FinalScores.text = finalScores.TrimEnd();

        EscMenu.SetActive(false);
        ScoreBoard.SetActive(true);
        RestartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient); // Only let Host press restart otherwise it gets messed up
        EndAudio.Play();
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

        SetOptionsAvailable(PhotonNetwork.IsMasterClient);// In case the master client left and you are the new one, enable the start button and options
        UpdatePlayerCount();
    }

    public void ShowEscMenu()
    {
        EscMenu.SetActive(!EscMenu.activeSelf);
    }

    public void ChangeRowCount(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int rowsSliderValue = (int) Mathf.Clamp(value, 1, 10);
            int rows = rowsSliderValue * 10;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("Rows", rows)) properties["Rows"] = rows;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            RowsInputField.text = rows.ToString();
            _view.RPC("UpdateSlider", RpcTarget.All, 0, 0, rowsSliderValue, rows);
        }
    }

    public void ChangeColumnCount(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int columnsSliderValue = (int) Mathf.Clamp(value, 1, 10);
            int columns = columnsSliderValue * 10;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("Columns", columns)) properties["Columns"] = columns;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            ColumnsInputField.text = columns.ToString();
            _view.RPC("UpdateSlider", RpcTarget.All, 1, 1, columnsSliderValue, columns);
        }
    }

    public void ChangeMineFrequency(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int mineFrequencySliderValue = (int) Mathf.Clamp(value, 1, 6);
            float mineFrequency = ((float) mineFrequencySliderValue) / 10;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("MineFrequency", mineFrequency)) properties["MineFrequency"] = mineFrequency;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            _view.RPC("UpdateSlider", RpcTarget.All, 2, 2, mineFrequencySliderValue, mineFrequencySliderValue);
            Grid.Instance.MineFrequency = mineFrequency;
        }
    }

    public void ChangeRoundLength(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int roundLengthSliderValue = (int) Mathf.Clamp(value, 1, 20);
            int roundLength = roundLengthSliderValue * 15;
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!properties.TryAdd("RoundLength", roundLength)) properties["RoundLength"] = roundLength;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            _view.RPC("UpdateSlider", RpcTarget.All, 3, 3, roundLengthSliderValue, roundLength);
            GameController.Instance.TimeLeft = value;
        }
    }

    // Update slider and input values (0: Rows, 1: Columns, 2: MineFrequency, 3: RoundLength)
    [PunRPC]
    private void UpdateSlider(int slider, int inputField, int silderValue, int inputFieldValue)
    {
        _sliders[slider].value = silderValue;
        _inputFields[inputField].text = inputFieldValue.ToString();
    }

    private void UpdatePlayerCount()
    {
        PlayersText.text = "Players (" + _playerListings.Count.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "):";
    }

    private void SetOptionsAvailable(bool available)
    {
        foreach (Slider slider in _sliders) slider.interactable = available;
        foreach (TMP_InputField inputField in _inputFields) inputField.interactable = available;
        foreach (Button sliderHandle in _sliderHandles) sliderHandle.interactable = available;
        StartButton.interactable = available;
    }

    private void OnDestroy() {
        Events.OnSetScore -= SetScore;
        Events.OnSetPowerupInFirstSlot -= SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot -= SetPowerupInSecondSlot;
        Events.OnEndOfRound -= ShowScoreboard;
    }

    public void RestartButtonClicked()
    {
        GameController.Instance.Restart();
    }
    public void MainMenuButtonClicked()
    {
        GameController.Instance.BackToMainMenu();
    }
    public void QuitButtonClicked()
    {
        GameController.Instance.Quit();
    }

}
