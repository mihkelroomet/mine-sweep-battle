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
    public TMP_Text ScoreText;
    public TMP_Text TimerText;
    public GameObject ScoreBoard;
    public GameObject EscMenu;
    public TMP_Text FinalScores;
    public Image FirstPowerupSlotImage;
    public Image SecondPowerupSlotImage;
    public AudioSource TickAudio;
    public AudioSource StartAudio;
    public AudioSource EndAudio;
    public Button RestartButton;

    // Lobby panel
    public TMP_Text RoomName;
    public TMP_Text PlayersText;
    public GameObject Options;
    public Button StartButton;
    public Button LeaveButton;
    public PlayerListing PlayerListingPrefab;
    public Transform Content;
    public Slider RowsSlider;
    public TMP_InputField RowCount;
    public Slider ColumnsSlider;
    public TMP_InputField ColumnCount;
    public Slider RoundLengthSlider;
    public TMP_InputField RoundLengthInput;
    public Slider BombFrequencySlider;
    public TMP_InputField BombFrequencyInput;

    private List<PlayerListing> _playerListings = new List<PlayerListing>();

    [SerializeField] private PhotonView _view;

    private void Awake() {
        Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnSetPowerupInFirstSlot += SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot += SetPowerupInSecondSlot;
        Events.OnEndOfRound += ShowScoreboard;
        Transitions.Instance.PlayEnterTransition();
        LeaveButton.onClick.AddListener(() => GameController.Instance.BackToMainMenu());
        StartButton.onClick.AddListener(() => GameController.Instance.StartGame());

        RoomName.text = "Room name: "+ PhotonNetwork.CurrentRoom.Name;
        RowsSlider.value = (int) PhotonNetwork.CurrentRoom.CustomProperties["Rows"];
        RowCount.text = RowsSlider.value.ToString();
        ColumnsSlider.value = (int)PhotonNetwork.CurrentRoom.CustomProperties["Columns"];
        ColumnCount.text = ColumnsSlider.value.ToString();
        RoundLengthSlider.value = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"];
        RoundLengthInput.text = RoundLengthSlider.value.ToString();
        BombFrequencySlider.value = (float)PhotonNetwork.CurrentRoom.CustomProperties["MineProbability"] * 10;
        BombFrequencyInput.text = BombFrequencySlider.value.ToString();

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

    private void OnDestroy() {
        Events.OnSetScore -= SetScore;
        Events.OnSetPowerupInFirstSlot -= SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot -= SetPowerupInSecondSlot;
        Events.OnEndOfRound -= ShowScoreboard;
    }

    public void ChangeRowCount(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties["Rows"] = (int)value;
            RowCount.text = value.ToString();
            _view.RPC("UpdateSlider", RpcTarget.All, 0, 0, (int)value);
        }
    }

    public void ChangeRowCount(string value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int count = int.Parse(value);
            PhotonNetwork.CurrentRoom.CustomProperties["Rows"] = count;
            _view.RPC("UpdateSlider", RpcTarget.All, 0, 0, count);
        }
    }

    public void ChangeColumnCount(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties["Columns"] = (int)value;
            _view.RPC("UpdateSlider", RpcTarget.All, 1, 1, (int)value);
        }
    }

    public void ChangeColumnCount(string value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int count = int.Parse(value);
            PhotonNetwork.CurrentRoom.CustomProperties["Columns"] = count;
            _view.RPC("UpdateSlider", RpcTarget.All, 1, 1, count);
        }
    }

    public void ChangeRoundLength(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"] = (int)value;
            GameController.Instance.TimeLeft = value;
            _view.RPC("UpdateSlider", RpcTarget.All, 2, 2, (int)value);
        }
    }

    public void ChangeRoundLength(string value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            float count = float.Parse(value);
            PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"] = (int)count;
            GameController.Instance.TimeLeft = count;
            _view.RPC("UpdateSlider", RpcTarget.All, 2, 2, (int)count);
        }
    }

    public void ChangeMineProbability(float value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Grid.Instance.MineProbability = value / 10;
            PhotonNetwork.CurrentRoom.CustomProperties["MineProbability"] = value / 10;
            _view.RPC("UpdateSlider", RpcTarget.All, 3, 3, (int)value);
        }
    }

    public void ChangeMineProbability(string value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            float count = float.Parse(value);
            Grid.Instance.MineProbability = count / 10;
            PhotonNetwork.CurrentRoom.CustomProperties["MineProbability"] = count / 10;
            _view.RPC("UpdateSlider", RpcTarget.All, 3, 3, (int)count);
        }
    }

    [PunRPC]
    private void UpdateSlider(int slider, int inputField, int value) // Update slider and input values (0: Rows, 1: Columns, 2: RoundLength, 3: MineProbability)
    {
        List<Slider> sliders = new List<Slider> { RowsSlider, ColumnsSlider, RoundLengthSlider, BombFrequencySlider };
        List<TMP_InputField> inputFields = new List<TMP_InputField> { RowCount, ColumnCount, RoundLengthInput, BombFrequencyInput };
        
        sliders[slider].value = value;
        inputFields[inputField].text = value.ToString();
    }

    private void UpdatePlayerCount()
    {
        PlayersText.text = "Players (" + _playerListings.Count.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "):";
    }

    private void SetOptionsAvailable(bool available)
    {
        foreach (Slider slider in Options.GetComponentsInChildren<Slider>())
        {
            slider.interactable = available;
            slider.gameObject.GetComponentInChildren<TMP_InputField>().interactable = available;
            StartButton.interactable = available;
        }
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
