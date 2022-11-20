using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerOld : MonoBehaviour
{
    public bool GameActive {get; set;}

    public static GameControllerOld Instance;
    public int Score;
    public float RoundTime;

    public float TimeLeft {
        get {
            return _timeLeft;
        }
        set {
            _timeLeft = value;
            HUDPresenterOld.Instance.UpdateTimer(value);
        }
    }
    private float _timeLeft;

    private void Awake() {
        Instance = this;
        EventsOld.OnSetScore += SetScore;
        EventsOld.OnGetScore += GetScore;
        EventsOld.OnEndOfRound += EndOfRound;
        GameActive = false; // Will wait for countdown to become active
        Score = 0;
    }

    private void Start() {
        TimeLeft = RoundTime;
        EventsOld.SetScore(Score);
    }

    private void Update() {
        if (GameActive) {
            // End the round if time has run out or if all the cells have been opened
            if (TimeLeft <= 0 || GridOld.Instance.CellsOpened == (GridOld.Instance.Columns-2)* (GridOld.Instance.Rows-2)) {
                EventsOld.EndOfRound();
            }
            else {
                TimeLeft -= Time.deltaTime;
            }
        }
    }

    private void SetScore(int value) {
        Score = value;
    }

    private int GetScore() {
        return Score;
    }

    private void EndOfRound() {
        GameActive = false;
    }


    public void Restart()
    {
        Debug.Log("Restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    private void OnDestroy() {
        EventsOld.OnSetScore -= SetScore;
        EventsOld.OnGetScore -= GetScore;
        EventsOld.OnEndOfRound -= EndOfRound;
    }
}
