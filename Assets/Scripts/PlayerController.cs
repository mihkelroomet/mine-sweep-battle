using UnityEngine;
using Photon.Pun;
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private LineRenderer _lineRenderer;
    private PhotonView _view;

    private float _beamTimer;
    private bool _defusing;
    private float _stunTimer;
    private float _stunDuration;


    // Player
    private float _walkSpeed = 4f;
    private float _speedLimiter = 0.7f;
    private float _inputHorizontal;
    private float _inputVertical;

    // Animations and states
    private Animator _animator;
    private string _currentState;
    private const string PLAYER_IDLE = "Player_Idle";
    private const string PLAYER_MOVE_L = "Player_Move_L";
    private const string PLAYER_MOVE_R = "Player_Move_R";
    private const string PLAYER_MOVE_U = "Player_Move_U";
    private const string PLAYER_MOVE_D = "Player_Move_D";
    private const string PLAYER_STUNNED = "Player_Stunned";

    public static PlayerController Instance;
    public AudioSource Fire1Audio;
    public AudioSource Fire2Audio;
    public AudioSource MovingAudio;

    private void Awake() {
        Instance = this;
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        _view = gameObject.GetComponent<PhotonView>();
        _animator = gameObject.GetComponent<Animator>();
        _stunTimer = -1;
        _beamTimer = -1;
        _stunDuration = 1.5f;
    }

    void Start()
    {

    }


    void Update()
    {
        if (_view.IsMine) // If this is the player character the player spawned
        {
            // If not stunned
            if (_stunTimer < 0) {
                // If the game is active
                if (GameController.Instance.GameActive) {
                    _inputHorizontal = Input.GetAxisRaw("Horizontal");
                    _inputVertical = Input.GetAxisRaw("Vertical");

                    // Shooting the laserbeam
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        _defusing = false; // Breaks defusing process
                        _beamTimer = 0.1f;
                        FireBeam(0.05f, Color.red);
                    }


                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        _defusing = true;
                        _beamTimer = 0.15f;
                        FireBeam(0.1f, Color.green);
                    }
                }
                
            }

            // If stunned
            else {
                _stunTimer -= Time.deltaTime;
            }

            CountBeamDown();
        }
    }

    private void FixedUpdate()
    {
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
            _rb.velocity = new Vector2(_inputHorizontal * _walkSpeed, _inputVertical * _walkSpeed);

            // Update player animations
            if (_inputVertical < 0)
            {
                ChangeAnimationState(PLAYER_MOVE_D);
            }
            else if (_inputVertical > 0)
            {
                ChangeAnimationState(PLAYER_MOVE_U);
            }
            else if (_inputHorizontal > 0)
            {
                ChangeAnimationState(PLAYER_MOVE_R);
            }
            else if (_inputHorizontal < 0)
            {
                ChangeAnimationState(PLAYER_MOVE_L);
            }

            if (!MovingAudio.isPlaying) {
                MovingAudio.Play();
            }
        }
        else
        {
            _rb.velocity = new Vector2(0f, 0f);
            if (_stunTimer > 0) {
                ChangeAnimationState(PLAYER_STUNNED);
            }
            else {
                ChangeAnimationState(PLAYER_IDLE);
            }
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

    // Animation state changer
    void ChangeAnimationState(string newState)
    {
        // Stop animation from interrupting self
        if (_currentState == newState) return;

        // Play new animation
        _animator.Play(newState);

        // Update current state
        _currentState = newState;
    }

    // Fires beam from self to what the mouse is pointing at
    private void FireBeam(float width, Color color) {
        // This is necessary because raycast also hits background and background has priority, no idea why
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 20f);

        _lineRenderer.startWidth = width;
        _lineRenderer.startColor = color;
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, gameObject.transform.position);
        if (hits[0].collider) {
            _lineRenderer.SetPosition(1, hits[0].point);
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.CompareTag("Cell")) {
                    Cell cell = hit.transform.GetComponent<Cell>();
                    cell.ShootWith(color);
                }
            }
        }
        else {
            _lineRenderer.SetPosition(1, gameObject.transform.position);
        }

        if (color == Color.red) {
            Fire1Audio.Play();
        }
        else {
            Fire2Audio.Play();
        }
    }

    // Stuns player
    public void Stun() {
        _inputHorizontal = 0;
        _inputVertical = 0;
        _stunTimer = _stunDuration;
    }


}
