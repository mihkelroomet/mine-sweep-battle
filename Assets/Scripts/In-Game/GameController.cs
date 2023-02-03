using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameController : MonoBehaviourPunCallbacks
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
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!roomProperties.TryAdd("CurrentScene", "In-Game")) roomProperties["CurrentScene"] = "In-Game";
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }

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
        Events.SetScore(Events.GetScore(), transform); // For triggering SetScore in HUDPresenter
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
            if (TimeLeft <= 0 || allCellsOpened)
            {
                if (PhotonNetwork.IsMasterClient) _view.RPC("EndRoundRPC", RpcTarget.All);
            }
            else
            {
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

    private void SetScore(int value, Transform trigger)
    {
        int change = value - Events.GetScore();
        Score = value;

        if (change != 0)
        {
            ScoreChange scoreChange = Instantiate(ScoreChangePrefab, trigger);
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

    public override void OnPlayerLeftRoom(Player leftPlayer)
    {
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.DestroyPlayerObjects(leftPlayer);
    }

    private void EndRound()
    {
        GameActive = false;
    }

    [PunRPC]
    void EndRoundRPC()
    {
        Events.EndRound();
    }

    public void Restart()
    {
        _view.RPC("RestartRPC", RpcTarget.All);
    }

    [PunRPC]
    void RestartRPC()
    {
        string nextScene = PhotonNetwork.CurrentRoom.IsVisible ? "Lobby" : "In-Game"; // Skip lobby if practicing
        // If not for the below line there would be no Camera nor AudioListener for the duration of the transition
        Camera.main.transform.parent = HUDPresenter.Instance.transform;
        // Destroy player's views that weren't part of the base In-Game scene
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        Transitions.Instance.ExitSceneWithTransition(nextScene);
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
