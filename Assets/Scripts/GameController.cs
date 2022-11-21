using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    public bool GameActive {get; set;}

    public static GameController Instance;
    [SerializeField] private PhotonView _view;
    public int Score;
    public float RoundTime;

    public float TimeLeft
    {
        get {
            return _timeLeft;
        }
        set {
            _timeLeft = value;
            // HUDPresenter.Instance.UpdateTimer(value);
        }
    }
    private float _timeLeft;

    private void Awake()
    {
        Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnGetScore += GetScore;
        Events.OnEndOfRound += EndOfRound;
        // GameActive = false; // Will wait for countdown to become active
        GameActive = true;
        Score = 0;

        if (_view.IsMine)
        {
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
            properties.Add("Score", 0);
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
    }

    private void Start()
    {
        TimeLeft = RoundTime;
        Events.SetScore(Score);
    }

    private void Update()
    {
        if (GameActive) {
            // End the round if time has run out or if all the cells have been opened
            if (TimeLeft <= 0 || Grid.Instance.CellsOpened == (Grid.Instance.Columns-2)* (Grid.Instance.Rows-2)) {
                Events.EndOfRound();
            }
            else {
                // TimeLeft -= Time.deltaTime;
            }
        }
    }

    private void SetScore(int value)
    {
        Score = value;

        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        properties["Score"] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    private int GetScore()
    {
        return Score;
    }

    private void EndOfRound()
    {
        GameActive = false;
    }


    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        PhotonNetwork.LeaveRoom();
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        Events.OnSetScore -= SetScore;
        Events.OnGetScore -= GetScore;
        Events.OnEndOfRound -= EndOfRound;
    }
}
