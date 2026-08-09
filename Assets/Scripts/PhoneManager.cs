using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;
    public GameObject telaphone;
    public PhoneDialer phoneDialer;   // เพิ่ม: reference ไปยัง PhoneDialer

    public void OpenPhone()
    {
        if (!GameManager.Instance.emergencyMode)
        {
            Debug.Log("ต้องปิดประตูก่อน");
            return;
        }

        telaphone.SetActive(false);
        phonePanel.SetActive(true);
    }

    public void ClosePhone()
    {
        phonePanel.SetActive(false);
        telaphone.SetActive(true);

        phoneDialer.Clear();   // ล้างเลขที่ค้างไว้ กันเลขเก่าโผล่ตอนเปิดจอรอบหน้า
    }
}