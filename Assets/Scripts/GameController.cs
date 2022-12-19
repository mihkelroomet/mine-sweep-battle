using System.Collections;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    public bool GameActive {get; set;}

    public static GameController Instance;
    [SerializeField] private PhotonView _view;
    public int Score;
    public int Powerups;

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

    private void Awake()
    {
        if (_view.IsMine) Instance = this;
        Events.OnSetScore += SetScore;
        Events.OnGetScore += GetScore;
        Events.OnSetPowerups += SetPowerups;
        Events.OnGetPowerups += GetPowerups;
        Events.OnEndOfRound += EndOfRound;
        GameActive = false; // Will wait for countdown to become active once we add it back in
        Score = 0;
        Powerups = 0;
        _gameEndCheckInterval = 0.1f;
        _nextGameEndCheckTime = Time.time + _gameEndCheckInterval;

        if (_view.IsMine)
        {
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
            if (!properties.TryAdd("Score", 0)) properties["Score"] = 0; // Try to add property "Score". If it exists, assign the value to it instead.
            if (!properties.TryAdd("Powerups", 0)) properties["Powerups"] = 0; // Try to add property "Powerups". If it exists, assign the value to it instead.
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
    }

    IEnumerator Start()
    {
        AudioSourcePool.Instance.ClearAudioSources(); // Because audiosources have parents that get destroyed on new scene load
        if (PhotonNetwork.IsMasterClient)
        {
            TimeLeft = (int) PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"];
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
        else {
            _view.RPC("UpdateTimeLeftRPC", RpcTarget.MasterClient);
            while (! (bool) PhotonNetwork.CurrentRoom.CustomProperties["TimeLeftUpToDate"]) yield return new WaitForSeconds(0.1f);
            TimeLeft = (float) PhotonNetwork.CurrentRoom.CustomProperties["TimeLeft"];
            _view.RPC("SetTimeLeftOutOfDateRPC", RpcTarget.MasterClient);
        }
        Events.SetScore(Score);
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
                Events.EndOfRound();
            }
            else {
                TimeLeft -= Time.deltaTime;
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

    private void SetPowerups(int value)
    {
        Powerups = value;

        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        properties["Powerups"] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    private int GetScore()
    {
        return Score;
    }

    private int GetPowerups()
    {
        return Powerups;
    }

    private void EndOfRound()
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
        Events.OnSetPowerups -= SetPowerups;
        Events.OnGetPowerups -= GetPowerups;
        Events.OnEndOfRound -= EndOfRound;
    }
}
