using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;

    public void OpenPhone()
    {
        phonePanel.SetActive(true);
    }

    public void ClosePhone()
    {
        phonePanel.SetActive(false);
    }
}