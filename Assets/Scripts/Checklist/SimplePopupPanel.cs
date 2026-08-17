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
        if (panel != null) panel.SetActive(true);
        if (blocker != null) blocker.SetActive(true);
    }
 
    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
    }
}