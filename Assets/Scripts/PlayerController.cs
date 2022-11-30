using UnityEngine;
using TMPro;
using Photon.Pun;
public class PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PhotonView _view;
    [SerializeField] private GameObject _namePanel;
    [SerializeField] private TMP_Text _nametag;

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
    [SerializeField] private Animator _animator;

    public static PlayerController Instance;

    // Audio
    public AudioClipGroup FootstepsAudio;
    public AudioClipGroup Fire1Audio;
    public AudioClipGroup Fire2Audio;

    private void Awake() {
        Instance = this;
        _stunTimer = -1;
        _beamTimer = -1;
        _stunDuration = 1.5f;
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
            _rb.velocity = new Vector2(_inputHorizontal * _walkSpeed, _inputVertical * _walkSpeed);

            FootstepsAudio.Play();
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
                    cell.ShootWith(color, this);
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
}
