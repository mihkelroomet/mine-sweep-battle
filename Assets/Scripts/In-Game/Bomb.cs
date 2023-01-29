using System.Collections.Generic;
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
    public int StunPlayerWithBombBaseScore;
    public int StunPlayerWithBombMinScore;
    public float StunPlayerWithBombSiphonAmount; // How much of the other player's score you gain by stunning them
    private List<PlayerController> _playersStunned;

    private void Awake()
    {
        _exploding = false;
        _explodeTimeLeft = ExplodeTime;
        _playersStunned = new List<PlayerController>();
    }

    private void Update() {
        if (_explodeTimeLeft < 0)
        {
            if (_view.IsMine) PhotonNetwork.Destroy(this.gameObject);
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
        if (_view.IsMine && other.CompareTag("Cell"))
        {
            Cell cell = other.GetComponent<Cell>();

            if (!cell.IsOpen())
            {
                Destruct();
                if (!cell.IsBorderCell())
                {
                    Events.SetScore(Events.GetScore() + BustCellScore, cell.transform);
                    cell.RemoveMine();
                    cell.Open();
                }
            }

        }

        PossiblyCollideWithPlayer(other);
    }

    // For when the player is already colliding with the bomb when it explodes
    private void OnTriggerStay2D(Collider2D other)
    {
        PossiblyCollideWithPlayer(other);
    }

    private void PossiblyCollideWithPlayer(Collider2D other)
    {
        if (_exploding && other.CompareTag("Player"))
        {
            PlayerController otherPlayer = other.GetComponent<PlayerController>();
            if (!_playersStunned.Contains(otherPlayer))
            {
                _playersStunned.Add(otherPlayer);
                otherPlayer.Stun();

                PhotonView otherPlayerView = otherPlayer.GetComponent<PhotonView>();
                // If I threw the bomb and the stunned player isn't me, gimme score
                if (_view.IsMine && !otherPlayerView.IsMine)
                {
                    int otherPlayerScore = (int) otherPlayerView.Owner.CustomProperties["Score"];
                    int scoreGained = Mathf.Max(StunPlayerWithBombBaseScore + (int) Mathf.Round(otherPlayerScore * StunPlayerWithBombSiphonAmount), StunPlayerWithBombMinScore);
                    Events.SetScore(Events.GetScore() + scoreGained, otherPlayer.transform);
                }
            }
        }
    }

    private void Destruct()
    {
        if (!_exploding) _view.RPC("DestructRPC", RpcTarget.All);
    }

    [PunRPC]
    void DestructRPC()
    {
        if (!_exploding)
        {
            _exploding = true;
            Instantiate(Explosion, transform.position, transform.rotation);
            BombAudio.Play(transform.position);
            _spriteRenderer.sprite = null;
            _rb.velocity = Vector2.zero;
        }
    }
}
