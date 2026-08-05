using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;

    public void OpenPhone()
    {
        if (!GameManager.Instance.emergencyMode)
        {
            Debug.Log("ต้องปิดประตูก่อน");

            return;
        }

        phonePanel.SetActive(true);
    }

    public void ClosePhone()
    {
        phonePanel.SetActive(false);
    }
    
}