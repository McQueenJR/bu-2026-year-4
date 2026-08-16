using UnityEngine;
using UnityEngine.EventSystems;
 
/// <summary>
/// เทียบเท่า IDCardClickAbsorber แต่ใช้กับ Canvas UI
/// ติดกับพื้นหลัง/กรอบของสมุด Checklist เอง (ไม่ใช่ตัว Toggle แต่ละอัน)
/// Raycast Target = true, ไม่ทำอะไรตอนคลิก แค่ "รับ" คลิกไว้
/// เพื่อไม่ให้คลิกที่ตัวสมุด (แต่ไม่ได้โดน Toggle โดยตรง) ทะลุไปปิด Panel
/// </summary>
public class ChecklistClickAbsorber : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // ไม่ต้องทำอะไร แค่ดูดคลิกไว้เฉยๆ
    }
}

