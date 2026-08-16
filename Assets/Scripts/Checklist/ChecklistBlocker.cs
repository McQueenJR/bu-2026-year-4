using UnityEngine;
using UnityEngine.EventSystems;
 
/// <summary>
/// เทียบเท่า IDCardBlocker แต่ใช้กับ Canvas UI
/// ติดกับ Image ที่ขยายเต็ม Panel, Raycast Target = true, Alpha = 0
/// วางเป็น sibling แรกสุด (บนสุดของลิสต์ Hierarchy) ให้เนื้อหา Checklist (Toggle/ปุ่ม/โพสต์อิท)
/// เป็น sibling หลังจากนี้ เพื่อให้เนื้อหาเรนเดอร์ทับข้างบนและรับคลิกของตัวเองก่อน
/// คลิกส่วนที่ว่าง (ไม่มีเนื้อหาบัง) จะทะลุมาโดน Blocker แล้วปิด Panel
/// </summary>
public class ChecklistBlocker : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        ChecklistPopup.Instance.Hide();
    }
}
