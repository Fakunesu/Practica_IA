using UnityEngine;

public class PlayerState : MonoBehaviour
{

    [SerializeField] private bool hasPowerUp;

    private float powerUpTimer;

    public bool HasPowerUp => hasPowerUp;   


    void Update()
    {
        if(hasPowerUp)
        {
            powerUpTimer -= Time.deltaTime;

            if (powerUpTimer <= 0f)
            {
                hasPowerUp = false;
                powerUpTimer = 0f;
                Debug.Log("Power-up expired");
            }
        }
    }

    public void ActivatePowerUp(float duration)
    {
        hasPowerUp = true;
        powerUpTimer = duration;
        Debug.Log("Power-up activated");
    }
}
