using UnityEngine;
using UnityEngine.EventSystems;
 
/// <summary>
/// แปะกับ Image เต็มจอ (alpha 0) เหมือน ChecklistBlocker
/// แต่รับ reference ของ SimplePopupPanel ตรงๆ ใช้ซ้ำได้กับหลายพาเนิล
/// </summary>
public class SimplePopupBlocker : MonoBehaviour, IPointerClickHandler
{
    public SimplePopupPanel target;
 
    public void OnPointerClick(PointerEventData eventData)
    {
        if (target != null)
            target.Hide();
    }
}
