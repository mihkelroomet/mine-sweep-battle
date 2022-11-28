using UnityEngine;
using TMPro;

public class HUDPresenterOld : MonoBehaviour
{
    public static HUDPresenterOld Instance;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI FinalScores;
    // Panel
    public GameObject ScoreBoard;
    public AudioSource TickAudio;
    public AudioSource StartAudio;
    public AudioSource EndAudio;

    // ref var for my TMP text component
    public TMP_Text TimerText;

    private void Awake() {
        Instance = this;
        EventsOld.OnSetScore += SetScore;
        EventsOld.OnEndOfRound += ShowScoreboard;
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
        // This is just a mock way of showing scores for now
        if (EventsOld.GetScore() > 10_000) {
            FinalScores.text = "1.You\t" + EventsOld.GetScore() + "\n2.Noob\t\t\t-10";
        }
        else if (EventsOld.GetScore() >= -10) {
            FinalScores.text = "1.You\t\t\t" + EventsOld.GetScore() + "\n2.Noob\t\t\t-10";
        }
        else {
            FinalScores.text = "1.Noob\t\t\t-10\n2.You\t\t\t" + EventsOld.GetScore();
        }
        ScoreBoard.SetActive(true);
        EndAudio.Play();
    }


    private void OnDestroy() {
        EventsOld.OnEndOfRound -= ShowScoreboard;
    }
}
