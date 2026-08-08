using UnityEngine;

// ติดสคริปต์นี้ที่ตัวปุ่ม Quit โดยตรง (GameObject เดียวกับ Button component)
[RequireComponent(typeof(RectTransform))]
public class DodgeOnHover : MonoBehaviour
{
    [Header("ระยะและความไว")]
    public float detectRadius = 150f;   // เมาส์เข้าใกล้แค่ไหนถึงเริ่มหลบ (หน่วย pixel บนจอ)
    public float dodgeDistance = 120f;  // หลบไปไกลแค่ไหนต่อครั้ง
    public float moveSpeed = 15f;       // ความเร็วตอนขยับหนี/กลับที่เดิม

    [Header("ขอบเขตที่อนุญาตให้หลบ (relative to parent)")]
    public float maxOffsetX = 300f; // หลบได้ไกลสุดในแนวนอนจากตำแหน่งเริ่มต้น
    public float maxOffsetY = 150f; // หลบได้ไกลสุดในแนวตั้งจากตำแหน่งเริ่มต้น

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Vector2 originalAnchoredPos;
    private Vector2 targetAnchoredPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;
        originalAnchoredPos = rectTransform.anchoredPosition;
        targetAnchoredPos = originalAnchoredPos;
    }

    void Update()
    {
        Vector2 mouseScreenPos = Input.mousePosition;

        // แปลงตำแหน่งเมาส์ให้อยู่ใน local space เดียวกับ parent เพื่อเทียบระยะห่างกับปุ่ม
        Vector2 mouseLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, mouseScreenPos, null, out mouseLocalPos);

        float distance = Vector2.Distance(mouseLocalPos, rectTransform.anchoredPosition);

        if (distance < detectRadius)
        {
            // ทิศทางหนี = จากเมาส์ไปหาปุ่ม (ตรงข้ามกับเมาส์)
            Vector2 dodgeDir = (rectTransform.anchoredPosition - mouseLocalPos).normalized;

            // ถ้าเมาส์อยู่ตำแหน่งเดียวกับปุ่มเป๊ะ (ป้องกัน NaN) สุ่มทิศทางแทน
            if (dodgeDir == Vector2.zero)
                dodgeDir = Random.insideUnitCircle.normalized;

            Vector2 desiredOffset = dodgeDir * dodgeDistance;

            // จำกัดไม่ให้หลบไกลเกินขอบเขตที่ตั้งไว้ จากตำแหน่งเริ่มต้น
            desiredOffset.x = Mathf.Clamp(desiredOffset.x, -maxOffsetX, maxOffsetX);
            desiredOffset.y = Mathf.Clamp(desiredOffset.y, -maxOffsetY, maxOffsetY);

            targetAnchoredPos = originalAnchoredPos + desiredOffset;
        }
        else
        {
            // เมาส์ไม่ใกล้แล้ว ค่อยๆ กลับตำแหน่งเดิม
            targetAnchoredPos = originalAnchoredPos;
        }

        // เคลื่อนที่แบบนุ่มนวลไปยังตำแหน่งเป้าหมาย (ใช้ unscaledDeltaTime กันเหนียวเผื่อ timeScale = 0)
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetAnchoredPos,
            Time.unscaledDeltaTime * moveSpeed);
    }
}