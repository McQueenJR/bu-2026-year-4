using UnityEngine;
 
/// <summary>
/// Panel เอนกประสงค์ เปิด/ปิดพร้อม Blocker ในตัวเอง
/// ใช้กับพาเนิลรายชื่อ (คนธรรมดา / พระ) หรือพาเนิลอื่นๆ ที่ไม่ต้องการ Singleton
/// แปะกับ GameObject แม่ (เช่น Panel_Applicants) แล้วลาก panel/blocker ใส่เอง
/// </summary>
public class SimplePopupPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject blocker;
 
    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
    }
 
    public void Show()
    {
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

        if (panel != null) panel.SetActive(true);
        if (blocker != null) blocker.SetActive(true);
    }
 
    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
    }
}