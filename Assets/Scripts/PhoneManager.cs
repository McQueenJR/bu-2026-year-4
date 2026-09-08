using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;
    public GameObject telaphone;
    public PhoneDialer phoneDialer;

    public void OpenPhone()
    {
        // กันกดระหว่างมี dialog เปิดอยู่ หรือกำลังเรียกตำรวจ
        if (GameManager.Instance != null)
        {

            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ เปิดโทรศัพท์ไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ เปิดโทรศัพท์ไม่ได้ตอนนี้");
                return;
            }

        }
        if (!GameManager.Instance.emergencyMode)
        {
            Debug.Log("ต้องปิดประตูก่อน");
            return;
        }
        

        telaphone.SetActive(false);
        phonePanel.SetActive(true);
    }

    // ปิดโทรศัพท์
    public void ClosePhone()
    {
        if (phoneDialer != null && phoneDialer.IsCalling)
        {
            Debug.Log("กำลังโทรอยู่ ผู้เล่นปิดโทรศัพท์เองไม่ได้ตอนนี้");
            return;
        }

        ForceClosePhone();
    }

    // ปิดแบบบังคับจากระบบ
    public void ForceClosePhone()
    {
        phonePanel.SetActive(false);
        telaphone.SetActive(true);

        if (phoneDialer != null)
            phoneDialer.Clear();
    }
}