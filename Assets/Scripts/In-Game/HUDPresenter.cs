using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System;
using ExitGames.Client.Photon;

public class HUDPresenter : MonoBehaviourPunCallbacks
{
    public static HUDPresenter Instance;

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
    [SerializeField] private TMP_Text LiveScores;

    private void Awake() {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        Events.OnSetScore += SetScore;
        Events.OnSetPowerupInFirstSlot += SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot += SetPowerupInSecondSlot;
        Events.OnEndRound += ShowScoreboard;

        Transitions.Instance.PlayEnterTransition();
    }

    void Start()
    {
        ScoreBoard.SetActive(false);
        EscMenu.SetActive(false);
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

    private void Update()
    {
        if (GameController.Instance.GameActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleEscMenu();
            }
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

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateScoreboard();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateScoreboard();
    }

    public override void OnPlayerLeftRoom(Player leftPlayer)
    {
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        string scoreBoard = "";
        Player[] sortedPlayerList = PhotonNetwork.PlayerList.OrderByDescending(player => player.CustomProperties["Score"]).ToArray();

        foreach (Player player in sortedPlayerList)
        {
            scoreBoard += Array.IndexOf(sortedPlayerList, player) + 1 + "." + player.NickName + "\t\t" + player.CustomProperties["Score"] + "\n";
        }

        LiveScores.text = scoreBoard;
    }

    public void ToggleEscMenu()
    {
        EscMenu.SetActive(!EscMenu.activeSelf);
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

    private void OnDestroy() {
        Events.OnSetScore -= SetScore;
        Events.OnSetPowerupInFirstSlot -= SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot -= SetPowerupInSecondSlot;
        Events.OnEndRound -= ShowScoreboard;
    }
}
