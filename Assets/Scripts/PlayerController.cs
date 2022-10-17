using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;


    private LineRenderer lineRend;
    private float _beamTimer;
    private bool defusing;
    private float _stunTimer;


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

    private void Awake() {
        Instance = this;
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        lineRend = gameObject.GetComponent<LineRenderer>();
        _stunTimer = -1;
        _beamTimer = -1;
    }

    void Start()
    {

    }


    void Update()
    {
        // If not stunned
        if (_stunTimer < 0) {
            inputHorizontal = Input.GetAxisRaw("Horizontal");
            inputVertical = Input.GetAxisRaw("Vertical");

            // Shooting the laserbeam
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                defusing = false; // Breaks defusing process
                _beamTimer = 0.1f;
                fireBeam(0.05f, Color.red);
            }


            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                defusing = true;
                _beamTimer = 0.5f;
                fireBeam(0.1f, Color.green);
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
        if (inputHorizontal != 0 || inputVertical != 0)
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
    private void fireBeam(float width, Color color) {
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

                    // Stun player if they made the wrong call
                    if (cell.IsBomb() && color == Color.red || !cell.IsBomb() && color == Color.green) {
                        Stun();
                        Cell.openNo -= 1;
                    }

                    cell.DefuseBomb();
                    cell.Open();
                }
            }
        }
        else {
            lineRend.SetPosition(1, gameObject.transform.position);
        }
    }

    // Stuns player for 1.5s
    public void Stun() {
        _stunTimer = 1.5f;
    }


    public void Restart()
    {

        Debug.Log("Restart");
        Cell.openNo = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }


}
