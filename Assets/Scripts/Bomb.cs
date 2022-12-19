using UnityEngine;
using Photon.Pun;

public class Bomb : MonoBehaviour
{
    public float ExplodeTimer = 2f;
    public int BeneficiaryID {get; set;}
    private CircleCollider2D _circleCollider2D;

    private void Awake() {
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _circleCollider2D.enabled = false;
    }

    private void Update() {
        if (ExplodeTimer <= 0)
        {
            //if (PhotonView.Find(BeneficiaryID).IsMine) Events.SetScore(Events.GetScore() + 10_000);
            _circleCollider2D.enabled = true;
            Destroy(this.gameObject);
            return;
        }
        ExplodeTimer -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Cell"))
        {
            Cell cell = other.GetComponent<Cell>();
            cell.RemoveMine();
            cell.Open();
        }
    }
}
