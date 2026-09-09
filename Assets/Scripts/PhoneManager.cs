using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;
    public GameObject telaphone;
    public PhoneDialer phoneDialer;

    public void OpenPhone()
    {
        // กันกดระหว่างถือแว่นขยาย / ไม้ขีดไฟ
        if (MagnifyingGlass.Instance != null && MagnifyingGlass.Instance.IsHolding)
            return;

        if (Matchbox.Instance != null && Matchbox.Instance.IsHolding)
            return;
        
        // กันกดระหว่างมี dialog เปิดอยู่ / กำลังเรียกตำรวจ / NPC ยังไม่ถึงจุดตรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
            {
                Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ กดไม่ได้ตอนนี้");
                return;
            }
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