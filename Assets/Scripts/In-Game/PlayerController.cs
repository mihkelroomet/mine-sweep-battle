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
    [SerializeField] private CircleCollider2D _circleCollider;
    [SerializeField] private SpriteRenderer _crosshair;
    [SerializeField] private PhotonView _view;
    [SerializeField] private GameObject _nametagPanel;

    // Beam
    public float RedBeamDuration;
    public float GreenBeamDuration;
    private float _beamTimer;
    private bool _defusing;
    private float _stunTimer;
    private float _stunDuration;
    private Cell _targetedCell;

    // Player
    public float Acceleration {get; set;}
    public float Deceleration {get; set;}
    public float MovementSpeed {get; set;}
    public float DefaultAcceleration;
    public float DefaultDeceleration;
    public float DefaultMovementSpeed;
    public float MaxTotalSpeedBoost;
    private float _maxSpeed;
    private float _maxAcceleration;
    private float _maxDeceleration;
    public float SpeedBoostMultiplier;
    public float SpeedBoostDuration;
    private float _speedBoostExpiresAt;
    private float _speedLimiter = 0.7f;
    private float _inputHorizontal;
    private float _inputVertical;

    // Animations and states
    [SerializeField] private Animator _animator;

    // Audio
    public AudioListener AudioListenerPrefab;
    public SFXClipGroup FootstepsAudio;
    public SFXClipGroup Fire1Audio;
    public SFXClipGroup Fire2Audio;
    public SFXClipGroup SpeedUpAudio;
    public SFXClipGroup SpeedDownAudio;

    // Powerups
    public float MaxBombVelocityMagnitude;
    public int SpeedPowerupScore;

    private void Awake()
    {
        if (_view.IsMine)
        {
            Instance = this;
            Instantiate(AudioListenerPrefab, transform);
        }
        else _crosshair.enabled = false; // Don't show crosshair of other players

        _stunTimer = -1;
        _beamTimer = -1;
        _stunDuration = 1.5f;
        MovementSpeed = DefaultMovementSpeed;
        Acceleration = DefaultAcceleration;
        Deceleration = DefaultDeceleration;
        _maxSpeed = DefaultMovementSpeed * MaxTotalSpeedBoost;
        _maxAcceleration = DefaultAcceleration * MaxTotalSpeedBoost;
        _maxDeceleration = DefaultDeceleration * MaxTotalSpeedBoost;
    }

    private void Start()
    {
        if (_view.IsMine)
        {
            Camera.main.transform.parent = this.transform; // Center camera on player
        }
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
            _rb.velocity = Vector2.MoveTowards(_rb.velocity, new Vector2(_inputHorizontal * MovementSpeed, _inputVertical * MovementSpeed), Time.deltaTime * Acceleration);

            FootstepsAudio.Play(transform);
        }

        // If the player is not moving or the game is not active
        else
        {
            _rb.velocity = Vector2.MoveTowards(_rb.velocity, Vector2.zero, Time.deltaTime * Deceleration);

            // Make player look towards crosshair
            // 0 - down, 1 - left, 2 - up, 3 - right
            float lookDirectionHorizontal = _crosshair.transform.position.x - transform.position.x;
            float lookDirectionVertical = _crosshair.transform.position.y - transform.position.y;
            int dominantLookDirection;
            if (Mathf.Abs(lookDirectionHorizontal) >= Mathf.Abs(lookDirectionVertical)) dominantLookDirection = (lookDirectionHorizontal >= 0) ? 3 : 1;
            else dominantLookDirection = (lookDirectionVertical >= 0) ? 2 : 0;
            _animator.SetInteger("DominantLookDirection", dominantLookDirection);
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
                
                // Speed boost running out
                if (MovementSpeed > DefaultMovementSpeed && _speedBoostExpiresAt > GameController.Instance.TimeLeft)
                {
                    SpeedDownAudio.Play(transform);
                    MovementSpeed = DefaultMovementSpeed;
                    Acceleration = DefaultAcceleration;
                    Deceleration = DefaultDeceleration;
                }
            }
        }

        // If stunned
        else {
            _animator.SetBool("Stunned", true);
            _stunTimer -= Time.deltaTime;
        }

        // Update crosshair placement
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // If no unopened cells in the way then target at mouse
        _crosshair.transform.position = new Vector3(mousePos.x, mousePos.y, 0); // Because z will always be wrong otherwise
        _targetedCell = null;
        // Targeting first cell in line
        RaycastHit2D[] lineHits = Physics2D.LinecastAll(_circleCollider.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (lineHits.Length > 0)
        {
            foreach (RaycastHit2D hit in lineHits)
            {
                if (hit.collider.CompareTag("Cell"))
                {
                    Cell cell = hit.transform.GetComponent<Cell>();
                    if (!cell.IsOpen())
                    {
                        _crosshair.transform.position = cell.transform.position;
                        _targetedCell = cell;
                        break;
                    }
                }
            }
        }

        CountBeamDown();
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
        _lineRenderer.SetPosition(1, _crosshair.transform.position);
        
        if (_targetedCell != null) _targetedCell.ShootWith(color, this);

        if (color == Color.red) PlayFireAudio(1);
        else PlayFireAudio(2);
    }

    // Stuns player
    public void Stun()
    {
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
                Vector2 movementDirection = Vector2.ClampMagnitude(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position, MaxBombVelocityMagnitude);
                Bomb bomb = PhotonNetwork.Instantiate("Bomb", _circleCollider.transform.position, transform.rotation).GetComponent<Bomb>();
                bomb.MovementDirection = movementDirection;
                break;
            case PowerupType.SpeedPowerup:
                Events.SetScore(Events.GetScore() + SpeedPowerupScore);
                MovementSpeed = Mathf.Min(MovementSpeed * SpeedBoostMultiplier, _maxSpeed);
                Acceleration = Mathf.Min(Acceleration * SpeedBoostMultiplier, _maxAcceleration);
                Deceleration = Mathf.Min(Deceleration * SpeedBoostMultiplier, _maxDeceleration);
                SpeedUpAudio.Play(transform);
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
        }
        else
        {
            _lineRenderer.startWidth = (float) stream.ReceiveNext();
            _lineRenderer.startColor = new Color((float) stream.ReceiveNext(), (float) stream.ReceiveNext(), (float) stream.ReceiveNext());
            _lineRenderer.positionCount = (int) stream.ReceiveNext();
            _lineRenderer.SetPositions((Vector3[]) stream.ReceiveNext());
        }
    }
}
