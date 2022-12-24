using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class HUDPresenter : MonoBehaviour
{
    public static HUDPresenter Instance;
    public TMP_Text ScoreText;
    public TMP_Text TimerText;
    public GameObject ScoreBoard;
    public GameObject EscMenu;
    public TMP_Text FinalScores;
    public Image FirstPowerupSlotItem;
    public Image SecondPowerupSlotItem;
    public AudioSource TickAudio;
    public AudioSource StartAudio;
    public AudioSource EndAudio;
    public Button RestartButton;

    private void Awake() {
        Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnSetFirstPowerupSlot += SetFirstPowerupSlot;
        Events.OnSetSecondPowerupSlot += SetSecondPowerupSlot;
        Events.OnEndOfRound += ShowScoreboard;
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

    private void SetScore(int value)
    {
        ScoreText.text = "Score: " + value;
    }

    private void SetFirstPowerupSlot(PowerupData data)
    {
        if (data == null) FirstPowerupSlotItem.color = Color.clear;
        else
        {
            FirstPowerupSlotItem.color = Color.white;
            FirstPowerupSlotItem.sprite = data.Pic64;
        }
    }

    private void SetSecondPowerupSlot(PowerupData data)
    {
        if (data == null) SecondPowerupSlotItem.color = Color.clear;
        else
        {
            SecondPowerupSlotItem.color = Color.white;
            SecondPowerupSlotItem.sprite = data.Pic32;
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

    public void ShowEscMenu()
    {
        EscMenu.SetActive(!EscMenu.activeSelf);
    }

    private void OnDestroy() {
        Events.OnSetScore -= SetScore;
        Events.OnSetFirstPowerupSlot -= SetFirstPowerupSlot;
        Events.OnSetSecondPowerupSlot -= SetSecondPowerupSlot;
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
