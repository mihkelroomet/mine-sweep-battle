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
    public TMP_Text FinalScores;
    public AudioSource TickAudio;
    public AudioSource StartAudio;
    public AudioSource EndAudio;
    public Button RestartButton;
    public Button MainMenuButton;
    public Button QuitButton;

    private void Awake() {
        Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnEndOfRound += ShowScoreboard;
        RestartButton.onClick.AddListener(() => GameController.Instance.Restart());
        MainMenuButton.onClick.AddListener(() => GameController.Instance.BackToMainMenu());
        QuitButton.onClick.AddListener(() => GameController.Instance.Quit());
    }

    void Start()
    {
        ScoreBoard.SetActive(false);
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

    private void SetScore(int value) {
        ScoreText.text = "Score: " + value;
    }

    public void ShowScoreboard()
    {
        string finalScores = "";
        byte placing = 1;
        Player[] sortedPlayerList = PhotonNetwork.PlayerList.OrderByDescending(player => player.CustomProperties["Score"]).ToArray();

        foreach (Player player in sortedPlayerList)
        {
            finalScores += placing++ + "." + player.NickName + "\t" + player.CustomProperties["Score"] + "\n";
        }

        FinalScores.text = finalScores.TrimEnd();

        ScoreBoard.SetActive(true);
        EndAudio.Play();
    }


    private void OnDestroy() {
        Events.OnEndOfRound -= ShowScoreboard;
    }
}
