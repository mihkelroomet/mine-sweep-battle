using UnityEngine;
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;


    private LineRenderer lineRend;
    private float _beamTimer;
    private bool defusing;
    private float _stunTimer;
    private float _stunDuration;


    // Player
    float walkSpeed = 4f;
    private float speedLimiter = 0.7f;
    private float inputHorizontal;
    private float inputVertical;

    // Animations and states
    private Animator animator;
    private string currentState;
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
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        lineRend = gameObject.GetComponent<LineRenderer>();
        _stunTimer = -1;
        _beamTimer = -1;
        _stunDuration = 1.5f;
    }

    void Start()
    {

    }


    void Update()
    {
        // If not stunned
        if (_stunTimer < 0) {
            // If the game is active
            if (GameController.Instance.GameActive) {
                inputHorizontal = Input.GetAxisRaw("Horizontal");
                inputVertical = Input.GetAxisRaw("Vertical");

                // Shooting the laserbeam
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    defusing = false; // Breaks defusing process
                    _beamTimer = 0.1f;
                    FireBeam(0.05f, Color.red);
                }


                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    defusing = true;
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

    private void FixedUpdate()
    {
        // If the player is moving and the game is active
        if ((inputHorizontal != 0 || inputVertical != 0) && GameController.Instance.GameActive)
        {
            // If player tries to move during defusing, break the process
            if (defusing)
            {
                defusing = false;
                lineRend.positionCount = 0;
            }

            // To avoid player getting faster when moving diagonally
            if (inputHorizontal != 0 && inputVertical != 0)
            {
                inputHorizontal *= speedLimiter;
                inputVertical *= speedLimiter;
            }
            rb.velocity = new Vector2(inputHorizontal * walkSpeed, inputVertical * walkSpeed);

            // Update player animations
            if (inputVertical < 0)
            {
                ChangeAnimationState(PLAYER_MOVE_D);
            }
            else if (inputVertical > 0)
            {
                ChangeAnimationState(PLAYER_MOVE_U);
            }
            else if (inputHorizontal > 0)
            {
                ChangeAnimationState(PLAYER_MOVE_R);
            }
            else if (inputHorizontal < 0)
            {
                ChangeAnimationState(PLAYER_MOVE_L);
            }

            if (!MovingAudio.isPlaying) {
                MovingAudio.Play();
            }
        }
        else
        {
            rb.velocity = new Vector2(0f, 0f);
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
        if (lineRend.positionCount > 0) { // If the beam is active
            _beamTimer -= Time.deltaTime;
            if (_beamTimer < 0) {
                lineRend.positionCount = 0;
            }
        }
    }

    // Animation state changer
    void ChangeAnimationState(string newState)
    {
        // Stop animation from interrupting self
        if (currentState == newState) return;

        // Play new animation
        animator.Play(newState);

        // Update current state
        currentState = newState;
    }

    // Fires beam from self to what the mouse is pointing at
    private void FireBeam(float width, Color color) {
        // This is necessary because raycast also hits background and background has priority, no idea why
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 20f);

        lineRend.startWidth = width;
        lineRend.startColor = color;
        lineRend.positionCount = 2;
        lineRend.SetPosition(0, gameObject.transform.position);
        if (hits[0].collider) {
            lineRend.SetPosition(1, hits[0].point);
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.CompareTag("Cell")) {
                    Cell cell = hit.transform.GetComponent<Cell>();
                    cell.ShootWith(color);
                }
            }
        }
        else {
            lineRend.SetPosition(1, gameObject.transform.position);
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
        inputHorizontal = 0;
        inputVertical = 0;
        _stunTimer = _stunDuration;
    }


}
