using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeathZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        Collider zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKill(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryKill(collision.gameObject);
    }

    private void TryKill(GameObject other)
    {
        if (!other.CompareTag(playerTag)) return;

        EventManager.ResetPlayerDetection();
        EventManager.TriggerPlayerDeath();
    }
}
