using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovementController playerStats = other.GetComponent<PlayerMovementController>();
            if (playerStats != null)
            {
                playerStats.isPowerUpped = true;
                Destroy(gameObject);
            }
        }
    }
}
