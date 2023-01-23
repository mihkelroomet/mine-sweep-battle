using System.Collections;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    public bool GameActive
    {
        get
        {
            return _gameActive;
        }
        set
        {
            _gameActive = value;
        }
    }
    private bool _gameActive;

    public static GameController Instance;
    [SerializeField] private PhotonView _view;
    public int Score;
    public PowerupData PowerupInFirstSlot;
    public PowerupData PowerupInSecondSlot;

    public ScoreChange ScoreChangePrefab;
    public float TimeLeft
    {
        get {
            return _timeLeft;
        }
        set {
            _timeLeft = value;
            HUDPresenter.Instance.UpdateTimer(value);
        }
    }
    private float _timeLeft;

    // For periodically checking if all cells have been opened
    private float _gameEndCheckInterval;
    private float _nextGameEndCheckTime;
    
    // Audio
    public AudioClip InGameMusic;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        Events.OnSetScore += SetScore;
        Events.OnGetScore += GetScore;
        Events.OnSetPowerupInFirstSlot += SetPowerupInFirstSlot;
        Events.OnGetPowerupInFirstSlot += GetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot += SetPowerupInSecondSlot;
        Events.OnGetPowerupInSecondSlot += GetPowerupInSecondSlot;
        Events.OnEndRound += EndRound;

        GameActive = false; // Will wait for countdown to become active once we add it back in -- I used it for lobby for now -Kaarel
        _gameEndCheckInterval = 0.1f;
        _nextGameEndCheckTime = Time.time + _gameEndCheckInterval;

        if (_view.IsMine)
        {
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
            if (!properties.TryAdd("Score", 0)) properties["Score"] = Events.GetScore(); // Try to add property "Score". If it exists, assign the value to it instead.
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
    }

    IEnumerator Start()
    {
        MusicPlayer.Instance.PlayMusic(InGameMusic);

        if (PhotonNetwork.IsMasterClient)
        {
            TimeLeft = (int) PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"];
        }
        else {
            _view.RPC("UpdateTimeLeftRPC", RpcTarget.MasterClient);
            while (! (bool) PhotonNetwork.CurrentRoom.CustomProperties["TimeLeftUpToDate"]) yield return new WaitForSeconds(0.1f);
            TimeLeft = (float) PhotonNetwork.CurrentRoom.CustomProperties["TimeLeft"];
            _view.RPC("SetTimeLeftOutOfDateRPC", RpcTarget.MasterClient);
        }
        Events.SetScore(Events.GetScore()); // For triggering SetScore in HUDPresenter
        GameActive = true;
    }

    private void Update()
    {
        if (GameActive)
        {
            bool allCellsOpened = false;
            if (_nextGameEndCheckTime <= Time.time && Grid.Instance.Initialized)
            {
                allCellsOpened = true;
                foreach (Cell[] cellColumn in Grid.Instance.CellGrid)
                {
                    foreach (Cell cell in cellColumn)
                    {
                        if (cell.IsMine)
                        {
                            allCellsOpened = false;
                            break;
                        }
                    }
                }
                _nextGameEndCheckTime = Time.time + _gameEndCheckInterval;
            }

            // End the round if time has run out or if all the cells have been opened
            if (TimeLeft <= 0 || allCellsOpened) {
                Events.EndRound();
            }
            else {
                TimeLeft -= Time.deltaTime;
            }
        }
    }

    private int GetScore()
    {
        return Score;
    }

    private PowerupData GetPowerupInFirstSlot()
    {
        return PowerupInFirstSlot;
    }

    private PowerupData GetPowerupInSecondSlot()
    {
        return PowerupInSecondSlot;
    }

    private void SetScore(int value)
    {
        int change = value - Events.GetScore();
        Score = value;

        if (change != 0)
        {
            ScoreChange scoreChange = Instantiate(ScoreChangePrefab, PlayerController.Instance.transform);
            if (change < 0)
            {
                scoreChange.ChangeText.text = change.ToString();
                scoreChange.ChangeText.color = scoreChange.NegativeColor;
            }
            else
            {
                scoreChange.ChangeText.text = "+" + change.ToString();
                scoreChange.ChangeText.color = scoreChange.PositiveColor;
            }
        }

        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        properties["Score"] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    private void SetPowerupInFirstSlot(PowerupData data)
    {
        PowerupInFirstSlot = data;
    }

    private void SetPowerupInSecondSlot(PowerupData data)
    {
        PowerupInSecondSlot = data;
    }

    private void EndRound()
    {
        GameActive = false;
    }

    [PunRPC]
    void UpdateTimeLeftRPC()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!properties.TryAdd("TimeLeft", TimeLeft)) properties["TimeLeft"] = TimeLeft;
        if (!properties.TryAdd("TimeLeftUpToDate", true)) properties["TimeLeftUpToDate"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    [PunRPC]
    void SetTimeLeftOutOfDateRPC()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!properties.TryAdd("TimeLeftUpToDate", false)) properties["TimeLeftUpToDate"] = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public void Restart()
    {
        _view.RPC("RestartRPC", RpcTarget.All);
    }

    [PunRPC]
    void RestartRPC()
    {
        Transitions.Instance.ExitSceneWithTransition("Lobby");
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
        Events.OnSetPowerupInFirstSlot -= SetPowerupInFirstSlot;
        Events.OnGetPowerupInFirstSlot -= GetPowerupInFirstSlot;
        Events.OnSetPowerupInSecondSlot -= SetPowerupInSecondSlot;
        Events.OnGetPowerupInSecondSlot -= GetPowerupInSecondSlot;
        Events.OnEndRound -= EndRound;
    }
}
