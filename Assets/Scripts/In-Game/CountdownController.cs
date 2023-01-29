using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class CountdownController : MonoBehaviour
{
    public static CountdownController Instance;

    // Countdown Time
    public int CountdownTime;
    public float CountdownTimeLeft {
        get
        {
            return _countdownTimeLeft;
        }
        set
        {
            _countdownTimeLeft = value;

            int number = (int) value;

            // For down to 1 second remaining display number of seconds left
            if (number > 0)
            {
                _countdownText.text = number.ToString();
                TickAudio.Play();
            }

            else
            {
                // At 0 display "GO!" and start the game
                _countdownText.text = "GO!";
                StartAudio.Play();
                GameController.Instance.GameActive = true;
                // 1 second after that remove "GO!"
                StartCoroutine(SetInactiveInOneSecond());
            }
        }
    }
    private float _countdownTimeLeft;

    // Components
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private PhotonView _view;

    // Audio
    [SerializeField] private SFXClipGroup TickAudio;
    [SerializeField] private SFXClipGroup StartAudio;

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
            InitializeCountdownTimeLeft();
            ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!roomProperties.TryAdd("CountdownTimeLeft", CountdownTimeLeft)) roomProperties["CountdownTimeLeft"] = CountdownTimeLeft;
            if (!roomProperties.TryAdd("CountdownTimeLeftValid", true)) roomProperties["CountdownTimeLeftValid"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }

        Events.OnEndRound += EndRound;
    }

    IEnumerator Start()
    {
        if (PhotonNetwork.IsMasterClient) CountdownTimeLeft = CountdownTimeLeft; // To trigger its set

        else
        {
            while (! (bool) PhotonNetwork.CurrentRoom.CustomProperties["CountdownTimeLeftValid"]) yield return new WaitForSeconds(0.1f);
            UpdateCountdownTimeLeftRPC();
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && CountdownTimeLeft >= 1)
        {
            float countdownTimeLeftPreviously = CountdownTimeLeft;
            _countdownTimeLeft -= Time.deltaTime;
            int previousCountdownNumber = (int) countdownTimeLeftPreviously;
            int newCountdownNumber = (int) CountdownTimeLeft;
            if (previousCountdownNumber != newCountdownNumber)
            {
                ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                if (!roomProperties.TryAdd("CountdownTimeLeft", CountdownTimeLeft)) roomProperties["CountdownTimeLeft"] = CountdownTimeLeft;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
                _view.RPC("UpdateCountdownTimeLeftRPC", RpcTarget.All);
            }
        }
    }

    private void InitializeCountdownTimeLeft()
    {
        _countdownTimeLeft = CountdownTime + 0.99f;
    }

    [PunRPC]
    void UpdateCountdownTimeLeftRPC()
    {
        CountdownTimeLeft = (float) PhotonNetwork.CurrentRoom.CustomProperties["CountdownTimeLeft"];
    }

    IEnumerator SetInactiveInOneSecond()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    private void EndRound()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!roomProperties.TryAdd("CountdownTimeLeftValid", false)) roomProperties["CountdownTimeLeftValid"] = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }

    private void OnDestroy()
    {
        Events.OnEndRound -= EndRound;
    }
}
