using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
public class PlayerController : MonoBehaviour, IPunObservable
{
    public static PlayerController Instance;

    // Components
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PhotonView _view;
    [SerializeField] private GameObject _namePanel;
    [SerializeField] private TMP_Text _nametag;

    // Beam
    public float RedBeamDuration;
    public float GreenBeamDuration;
    private float _beamTimer;
    private bool _defusing;
    private float _stunTimer;
    private float _stunDuration;


    // Player
    public float MovementSpeed {get; set;}
    public float DefaultMovementSpeed;
    public float BoostedMovementSpeed;
    public float SpeedBoostDuration;
    private float _speedBoostExpiresAt;
    private float _speedLimiter = 0.7f;
    private float _inputHorizontal;
    private float _inputVertical;

    // Animations and states
    [SerializeField] private Animator _animator;

    // Audio
    public AudioListener AudioListenerPrefab;
    public AudioClipGroup FootstepsAudio;
    public AudioClipGroup Fire1Audio;
    public AudioClipGroup Fire2Audio;

    private void Awake() {
        if (_view.IsMine)
        {
            Instance = this;
            Instantiate(AudioListenerPrefab, transform);
        }
        _stunTimer = -1;
        _beamTimer = -1;
        _stunDuration = 1.5f;
        MovementSpeed = DefaultMovementSpeed;
    }

    private void Start() {
        if (_view.IsMine)
        {
            Camera.main.transform.parent = this.transform; // Center camera on player
            _nametag.text = PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            _namePanel.SetActive(false); // Temporary solution until we implement showing everyone's nametags to everyone
        }
    }

    void Update()
    {
        if (!_view.IsMine) return; // Only update if this is the player character the player spawned

        // If not stunned
        if (_stunTimer < 0) {
            _animator.SetBool("Stunned", false);

            // If the game is active
            if (GameController.Instance.GameActive) {
                _inputHorizontal = Input.GetAxisRaw("Horizontal");
                _inputVertical = Input.GetAxisRaw("Vertical");

                // If mouse not over UI
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // Firing red beam
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        _defusing = false; // Breaks defusing process
                        _beamTimer = RedBeamDuration;
                        FireBeam(0.05f, Color.red);
                    }

                    // Firing green beam
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        _defusing = true;
                        _beamTimer = GreenBeamDuration;
                        FireBeam(0.1f, Color.green);
                    }

                    // Using powerups
                    if (Input.GetKeyDown(KeyCode.Space)) UsePowerup();
                    if (Input.GetKeyDown(KeyCode.LeftShift)) SwitchPowerups();
                }
                
                if (_speedBoostExpiresAt > GameController.Instance.TimeLeft) MovementSpeed = DefaultMovementSpeed;
            }
        }

        // If stunned
        else {
            _animator.SetBool("Stunned", true);
            _stunTimer -= Time.deltaTime;
        }

        CountBeamDown();
    }

    private void FixedUpdate()
    {
        if (!_view.IsMine) return; // Only update if this is the player character the player spawned

        // Update player animations
        _animator.SetFloat("HorizontalInput", _inputHorizontal);
        _animator.SetFloat("VerticalInput", _inputVertical);

        // If the player is moving and the game is active
        if ((_inputHorizontal != 0 || _inputVertical != 0) && GameController.Instance.GameActive)
        {
            // If player tries to move during defusing, break the process
            if (_defusing)
            {
                _defusing = false;
                _lineRenderer.positionCount = 0;
            }

            // To avoid player getting faster when moving diagonally
            if (_inputHorizontal != 0 && _inputVertical != 0)
            {
                _inputHorizontal *= _speedLimiter;
                _inputVertical *= _speedLimiter;
            }
            _rb.velocity = new Vector2(_inputHorizontal * MovementSpeed, _inputVertical * MovementSpeed);

            FootstepsAudio.Play(transform);
        }
        else
        {
            _rb.velocity = new Vector2(0f, 0f);
        }
    }

    // Makes sure the beam will eventually disappear
    private void CountBeamDown() {
        if (_lineRenderer.positionCount > 0) { // If the beam is active
            _beamTimer -= Time.deltaTime;
            if (_beamTimer < 0) {
                _lineRenderer.positionCount = 0;
            }
        }
    }

    // Fires beam from self to what the mouse is pointing at
    private void FireBeam(float width, Color color)
    {
        _lineRenderer.startWidth = width;
        _lineRenderer.startColor = color;
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition)); // If no hits on unopened cell draw line to mouse
        
        // Shooting first cell in line
        RaycastHit2D[] lineHits = Physics2D.LinecastAll(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (lineHits.Length > 0)
        {
            foreach (RaycastHit2D hit in lineHits)
            {
                if (hit.collider.CompareTag("Cell"))
                {
                    Cell cell = hit.transform.GetComponent<Cell>();
                    if (!cell.IsOpen())
                    {
                        _lineRenderer.SetPosition(1, hit.point);
                        cell.ShootWith(color, this);
                        break;
                    }
                }
            }
        }
        else
        {
            _lineRenderer.SetPosition(1, transform.position);
        }

        if (color == Color.red) PlayFireAudio(1);
        else PlayFireAudio(2);
    }

    // Stuns player
    public void Stun() {
        _inputHorizontal = 0;
        _inputVertical = 0;
        _stunTimer = _stunDuration;
    }

    private void UsePowerup()
    {
        PowerupData powerup = Events.GetPowerupInFirstSlot();
        if (powerup == null) return;
        switch (powerup.Type)
        {
            case PowerupType.BombPowerup:
                Vector2 movementDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                Bomb bomb = PhotonNetwork.Instantiate("Bomb", transform.position, transform.rotation).GetComponent<Bomb>();
                bomb.MovementDirection = movementDirection;
                break;
            case PowerupType.SpeedPowerup:
                MovementSpeed = BoostedMovementSpeed;
                _speedBoostExpiresAt = GameController.Instance.TimeLeft - SpeedBoostDuration;
                break;
        }
        Events.SetPowerupInFirstSlot(Events.GetPowerupInSecondSlot());
        Events.SetPowerupInSecondSlot(null);
    }

    private void SwitchPowerups()
    {
        PowerupData dataInFirstSlot = Events.GetPowerupInFirstSlot();
        Events.SetPowerupInFirstSlot(Events.GetPowerupInSecondSlot());
        Events.SetPowerupInSecondSlot(dataInFirstSlot);
    }

    public void PlayFireAudio(byte fireType)
    {
        _view.RPC("PlayFireAudioRPC", RpcTarget.All, fireType);
    }

    [PunRPC]
    void PlayFireAudioRPC(byte fireType)
    {
        switch (fireType)
        {
            case 1:
                Fire1Audio.Play(transform);
                break;
            case 2:
                Fire2Audio.Play(transform);
                break;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_lineRenderer.startWidth);
            stream.SendNext(_lineRenderer.startColor.r);
            stream.SendNext(_lineRenderer.startColor.g);
            stream.SendNext(_lineRenderer.startColor.b);
            stream.SendNext(_lineRenderer.positionCount);
            Vector3[] positions = new Vector3[_lineRenderer.positionCount];
            _lineRenderer.GetPositions(positions);
            stream.SendNext(positions);

            stream.SendNext(_namePanel.transform.position);
        }
        else
        {
            _lineRenderer.startWidth = (float) stream.ReceiveNext();
            _lineRenderer.startColor = new Color((float) stream.ReceiveNext(), (float) stream.ReceiveNext(), (float) stream.ReceiveNext());
            _lineRenderer.positionCount = (int) stream.ReceiveNext();
            _lineRenderer.SetPositions((Vector3[]) stream.ReceiveNext());

            _namePanel.transform.position = (Vector3) stream.ReceiveNext();
        }
    }

    private void OnDestroy() {
        // Removes any AudioSources attached to leaving player from the AudioSourcePool to prevent NullPointerExceptions
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("AudioSource"))
            {
                AudioSourcePool.Instance.RemoveAudioSource(child.GetComponent<AudioSource>());
            }
        }
    }
}
