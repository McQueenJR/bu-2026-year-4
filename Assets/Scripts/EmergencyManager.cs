using UnityEngine;

public class EmergencyManager : MonoBehaviour
{
    public GameObject emergencyDoor;   

    public GameManager gameManager;

    public void EmergencyButton()
    {
        emergencyDoor.SetActive(true);

        gameManager.emergencyMode = true;

        Debug.Log("Emergency Activated");
    }
}