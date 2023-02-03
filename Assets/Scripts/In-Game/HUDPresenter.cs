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
    [SerializeField] private GameObject CountdownPanel;
    [SerializeField] private GameObject LiveScoreboard;
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private GameObject TimerPanel;
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private GameObject ScorePanel;
    [SerializeField] private GameObject PowerupPanel;
    [SerializeField] private Image FirstPowerupSlotImage;
    [SerializeField] private Image SecondPowerupSlotImage;
    [SerializeField] private GameObject EscMenuPanel;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject FinalScoreboard;
    [SerializeField] private Button RestartButton;
    [SerializeField] private SFXClipGroup EndAudio;
    public ScoreRow LiveScoreRowPrefab;
    public ScoreRow FinalScoreRowPrefab;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        Transitions.Instance.PlayEnterTransition();

        Events.OnSetScore += SetScore;
        Events.OnSetPowerupInFirstSlot += SetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot += SetPowerupInSecondSlot;
        Events.OnEndRound += EndRound;
    }

    void Start()
    {
        foreach (ScoreRow liveScoreRow in LiveScoreboard.GetComponentsInChildren<ScoreRow>()) Destroy(liveScoreRow.gameObject);
        foreach (ScoreRow finalScoreRow in FinalScoreboard.GetComponentsInChildren<ScoreRow>()) Destroy(finalScoreRow.gameObject);
        
        CountdownPanel.SetActive(true);
        LiveScoreboard.SetActive(true);
        TimerPanel.SetActive(true);
        ScorePanel.SetActive(true);
        PowerupPanel.SetActive(true);
        EscMenuPanel.SetActive(false);
        GameOverPanel.SetActive(false);
    }

    // Update timer text
    public void UpdateTimer(float time) {
        time += 0.99f; // This is so the round would end the moment it shows "0:00" which feels more natural
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        TimerText.text = minutes + ":" + (seconds < 10 ? "0" : "") + seconds;

        // Make last 5 seconds of a round red
        TimerText.color = (time < 6) ? Color.red : Color.white;
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

    private void SetScore(int value, Transform trigger)
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

    public void EndRound()
    {
        CountdownPanel.SetActive(false);
        LiveScoreboard.SetActive(false);
        TimerPanel.SetActive(false);
        ScorePanel.SetActive(false);
        PowerupPanel.SetActive(false);
        EscMenuPanel.SetActive(false);
        GameOverPanel.SetActive(true);
        RestartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient); // Only let Host press restart otherwise it gets messed up
        EndAudio.Play();

        Player[] sortedPlayerList = PhotonNetwork.PlayerList.OrderByDescending(player => player.CustomProperties["Score"]).ToArray();

        for (int i = 0; i < Mathf.Min(sortedPlayerList.Length, 7); i++)
        {
            Player player = sortedPlayerList[i];
            ScoreRow finalScoreRow = Instantiate(FinalScoreRowPrefab, FinalScoreboard.transform);
            finalScoreRow.SetPlayerNameText(Array.IndexOf(sortedPlayerList, player) + 1 + ". " + player.NickName);
            finalScoreRow.SetPlayerScoreText(player.CustomProperties["Score"].ToString());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateLiveScoreboard();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLiveScoreboard();
    }

    public override void OnPlayerLeftRoom(Player leftPlayer)
    {
        UpdateLiveScoreboard();
        RestartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient); // Need to update this in case host left
    }

    private void UpdateLiveScoreboard()
    {
        foreach (ScoreRow liveScoreRow in LiveScoreboard.GetComponentsInChildren<ScoreRow>()) Destroy(liveScoreRow.gameObject);
        Player[] sortedPlayerList = PhotonNetwork.PlayerList.OrderByDescending(player => player.CustomProperties["Score"]).ToArray();

        int i;
        for (i = 0; i < Mathf.Min(sortedPlayerList.Length, 5); i++)
        {
            Player player = sortedPlayerList[i];
            ScoreRow liveScoreRow = Instantiate(LiveScoreRowPrefab, LiveScoreboard.transform);
            liveScoreRow.SetPlayerNameText(Array.IndexOf(sortedPlayerList, player) + 1 + ". " + player.NickName);
            liveScoreRow.SetPlayerScoreText(player.CustomProperties["Score"].ToString());
        }

        float scoreBoardHeight = 10 + i * 22 + (i - 1) * 2;
        LiveScoreboard.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scoreBoardHeight);
    }

    public void ToggleEscMenu()
    {
        EscMenuPanel.SetActive(!EscMenuPanel.activeSelf);
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
        Events.OnEndRound -= EndRound;
    }
}
