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
    public float ExplosionGrowthRate;
    public float ExplodeTimer;
    public float ExplodeTime;
    private bool _exploding;
    private float _explodeTimeLeft;

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

            if (!cell.IsOpen()) Destruct();

            cell.RemoveMine();
            cell.Open();
        }
    }

    private void Destruct()
    {
        if (!_exploding)
        {
            _exploding = true;
            Instantiate(Explosion, transform.position, transform.rotation);
            _spriteRenderer.sprite = null;
        }
    }
}
