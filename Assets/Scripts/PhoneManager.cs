using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;
    public GameObject telaphone;   // เพิ่ม: ตัว GameObject โทรศัพท์ในฉาก

    public void OpenPhone()
    {
        if (!GameManager.Instance.emergencyMode)
        {
            Debug.Log("ต้องปิดประตูก่อน");

            return;
        }

        telaphone.SetActive(false);   // ซ่อนโทรศัพท์ในฉากก่อน
        phonePanel.SetActive(true);   // แล้วค่อยเปิด UI
    }

    public void ClosePhone()
    {
        phonePanel.SetActive(false);  // ปิด UI ก่อน
        telaphone.SetActive(true);    // แล้วคืนโทรศัพท์กลับมาโชว์
    }
}