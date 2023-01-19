using UnityEngine;
using Photon.Pun;

public class Bomb : MonoBehaviour
{
    // Components
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private CircleCollider2D _circleCollider2D;
    [SerializeField] private PhotonView _view;

    // Movement
    public Vector2 MovementDirection {get; set;}
    public float MovementSpeed;
    public float Deceleration;
    
    // Explosion
    public ParticleSystem Explosion;
    public SFXClipGroup BombAudio;
    public float ExplosionGrowthRate;
    public float ExplodeTimer;
    public float ExplodeTime;
    private bool _exploding;
    private float _explodeTimeLeft;

    // Scoring
    public int BustCellScore;

    private void Awake()
    {
        _exploding = false;
        _explodeTimeLeft = ExplodeTime;
    }

    private void Update() {
        if (_explodeTimeLeft < 0)
        {
            Destroy(this.gameObject);
            return;
        }

        if (_exploding)
        {
            _explodeTimeLeft -= Time.deltaTime;
            _circleCollider2D.radius += ExplosionGrowthRate * Time.deltaTime;
            return;
        }

        if (ExplodeTimer <= 0)
        {
            Destruct();
            return;
        }

        ExplodeTimer -= Time.deltaTime;

        MovementSpeed = MovementSpeed * (1 - Deceleration * Time.deltaTime);

        _rb.velocity = MovementDirection * MovementSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cell"))
        {
            Cell cell = other.GetComponent<Cell>();

            if (!cell.IsOpen())
            {
                Events.SetScore(Events.GetScore() + BustCellScore);
                Destruct();
                cell.RemoveMine();
                cell.Open();
            }

        }

        if (_exploding && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.Stun();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // For when the player if already colliding with the bomb when it explodes
        if (_exploding && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.Stun();
        }
    }

    private void Destruct()
    {
        if (!_exploding) _view.RPC("DestructRPC", RpcTarget.All);
    }

    [PunRPC]
    void DestructRPC()
    {
        _exploding = true;
        Instantiate(Explosion, transform.position, transform.rotation);
        BombAudio.Play(SFXSourcePool.Instance.transform, transform.position);
        _spriteRenderer.sprite = null;
        _rb.velocity = Vector2.zero;
    }
}
