using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    public Vector2 MovementDirection {get; set;}
    public float MovementSpeed;
    public float Deceleration;
    public float ExplodeTimer;
    private CircleCollider2D _circleCollider2D;

    private void Awake() {
        _circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Update() {
        if (ExplodeTimer <= 0)
        {
            Destroy(this.gameObject);
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
            cell.RemoveMine();
            cell.Open();
        }
    }
}
