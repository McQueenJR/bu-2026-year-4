using UnityEngine;

// ติดที่ MarkOverlay (มี Collider2D + SpriteRenderer กากบาท)
public class TodayListMarkToggle : MonoBehaviour
{
    [Header("อ้างอิงตัวลาก/ปิดของพาเนล (ปล่อยว่างได้ จะหาให้เองจาก parent)")]
    public TodayListDisplayClick displayClick;

    private SpriteRenderer sr;
    private bool isMarked = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = isMarked;

        if (displayClick == null)
            displayClick = GetComponentInParent<TodayListDisplayClick>();
    }

    // ซ้าย → forward ไปลากพาเนลทั้งก้อน (เหมือนปุ่มของ Document1)
    // ปุ่มกลาง ไม่ต้องทำอะไรที่นี่ เพราะ TodayListDisplayClick.Update() จัดการให้แล้วทุกเฟรม
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0) && displayClick != null)
            displayClick.BeginDrag(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        if (displayClick != null)
            displayClick.ContinueDrag(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0) && displayClick != null)
            displayClick.EndDrag();
    }

    // ขวา → forward ไปปิดพาเนลเหมือนปุ่มของ Document1
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1) && displayClick != null)
            displayClick.RequestClose();
    }

    public void ToggleMark()
    {
        isMarked = !isMarked;
        if (sr != null)
            sr.enabled = isMarked;
    }

    public void ResetMark()
    {
        isMarked = false;
        if (sr != null)
            sr.enabled = false;
    }
}