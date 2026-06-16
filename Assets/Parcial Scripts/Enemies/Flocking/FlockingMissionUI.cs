using TMPro;
using UnityEngine;

public class FlockingMissionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlockingManager flockingManager;
    [SerializeField] private TextMeshProUGUI missionText;

    [Header("Messages")]
    [SerializeField] private string wanderMessage = "Buscá a los bichitos y guialos hasta su casa.";
    [SerializeField] private string followMessage = "Los bichitos te están siguiendo. No te separes mucho.";
    [SerializeField] private string completedMessage = "¡Todos los bichitos llegaron a casa!";

    [Header("Counter")]
    [SerializeField] private bool showCounter = true;

    private void Update()
    {
        if (flockingManager == null || missionText == null)
            return;

        int agentsLeft = flockingManager.Agents.Count;

        if (agentsLeft <= 0)
        {
            missionText.text = completedMessage;
            return;
        }

        string baseMessage;

        if (flockingManager.CurrentMode == FlockingManager.FlockMode.FollowPlayer)
        {
            baseMessage = followMessage;
        }
        else
        {
            baseMessage = wanderMessage;
        }

        if (showCounter)
        {
            missionText.text = baseMessage + "\nBichitos restantes: " + agentsLeft;
        }
        else
        {
            missionText.text = baseMessage;
        }
    }
}