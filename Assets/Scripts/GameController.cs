using System.Collections;
using UnityEngine;
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
            if (!properties.TryAdd("Score", 0)) properties["Score"] = 0; // Try to add property "Score". If it exists, assign the value to it instead.
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HUDPresenter.Instance.ShowEscMenu();
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
        _view.RPC("RestartRPC", RpcTarget.All);
    }

    [PunRPC]
    void RestartRPC()
    {
        Transitions.Instance.ExitSceneWithTransition("In-Game");
    }

    public void BackToMainMenu()
    {
        StartCoroutine(BackToMainMenuCoroutine());
    }

    IEnumerator BackToMainMenuCoroutine()
    {
        yield return Transitions.Instance.ExitSceneWithTransitionCoroutine("MainMenu");
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
