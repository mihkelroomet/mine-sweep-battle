using UnityEngine;

public class Powerup : MonoBehaviour
{
    private BoxCollider2D _boxCollider2D;

    private void Awake() {
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Events.SetPowerups(Events.GetPowerups() + 1);
            Destroy(this.gameObject);
        }
    }
}
