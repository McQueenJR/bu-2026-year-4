using UnityEngine;

public abstract class Document1ChildBase : MonoBehaviour
{
    [Header("อ้างอิงตัวลาก/ปิดของเอกสาร (ลากมาจาก documentRoot)")]
    public Document1DisplayClick displayClick;

    [Header("ระยะขยับ (พิกเซล) ก่อนจะถือว่าเป็นการลาก ไม่ใช่คลิก")]
    public float dragThreshold = 6f;

    private Vector3 mouseDownScreenPos;
    private bool isPotentialDrag = false;
    private bool draggedPastThreshold = false;

    // ให้คลาสลูก (RowButton, NavButton) กำหนดว่าเมื่อ "คลิกจริง" ให้ทำอะไร
    protected abstract void OnClick();

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPotentialDrag = true;
            draggedPastThreshold = false;
            mouseDownScreenPos = Input.mousePosition;

            if (displayClick != null)
                displayClick.BeginDrag(Input.mousePosition);
        }
    }

    private void OnMouseDrag()
    {
        if (!isPotentialDrag)
            return;

        float moved = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
        if (moved > dragThreshold)
            draggedPastThreshold = true;

        if (displayClick != null)
            displayClick.ContinueDrag(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (displayClick != null)
                displayClick.EndDrag();

            // ไม่เคยขยับเกิน threshold → นับเป็นคลิกปุ่มจริง
            if (isPotentialDrag && !draggedPastThreshold)
                OnClick();

            isPotentialDrag = false;
        }
    }

    private void OnMouseOver()
    {
        // คลิกขวา → forward ไปปิดเอกสารเหมือนกัน
        if (Input.GetMouseButtonDown(1))
        {
            if (displayClick != null)
                displayClick.RequestClose();
        }
    }
}