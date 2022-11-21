using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool GameActive {get; set;}

    public static GameController Instance;
    public int Score;
    public float RoundTime;

    public float TimeLeft {
        get {
            return _timeLeft;
        }
        set {
            _timeLeft = value;
            // HUDPresenter.Instance.UpdateTimer(value);
        }
    }
    private float _timeLeft;

    private void Awake() {
        Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnGetScore += GetScore;
        Events.OnEndOfRound += EndOfRound;
        // GameActive = false; // Will wait for countdown to become active
        GameActive = true;
        Score = 0;
    }

    private void Start() {
        TimeLeft = RoundTime;
        Events.SetScore(Score);
    }

    private void Update() {
        // if (GameActive) {
        //     // End the round if time has run out or if all the cells have been opened
        //     if (TimeLeft <= 0 || Grid.Instance.CellsOpened == (Grid.Instance.Columns-2)* (Grid.Instance.Rows-2)) {
        //         Events.EndOfRound();
        //     }
        //     else {
        //         TimeLeft -= Time.deltaTime;
        //     }
        // }
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
        Events.OnSetScore -= SetScore;
        Events.OnGetScore -= GetScore;
        Events.OnEndOfRound -= EndOfRound;
    }
}
