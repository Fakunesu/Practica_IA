using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerState playerState = other.GetComponent<PlayerState>();

        if (playerState != null)
        {
            playerState.ActivatePowerUp(duration);
            Debug.Log("Power-up collected! Duration: " + duration + " seconds.");
            Destroy(gameObject);
        }

    }
}
