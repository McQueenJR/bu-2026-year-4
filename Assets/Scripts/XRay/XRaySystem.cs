using UnityEngine;

public class XRaySystem : MonoBehaviour
{
    [Header("Magnifying Glass")]
    [SerializeField]
    private MagnifyingGlass magnifyingGlass;

    private NPCXRayTarget currentTarget;

    // 0 = ปกติ
    // 1 = ทะลุเสื้อผ้า
    // 2 = ทะลุทั้งร่าง
    private int currentLevel = 0;

    [Header("Scroll")]
    [Tooltip("เวลาพักระหว่างการเลื่อนแต่ละครั้ง (กันทัชแพดเลื่อนทีเดียวข้ามหลายระดับ)")]
    [SerializeField] private float scrollCooldown = 0.12f;

    private float nextScrollTime = 0f;
    
    private void Update()
    {
        if (magnifyingGlass == null)
            return;

        // ถ้ายังไม่ได้ถือแว่น
        if (!magnifyingGlass.IsHolding)
            return;

        // เลื่อนล้อเมาส์: ขึ้น = ระดับสูงขึ้น / ลง = ระดับต่ำลง
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (Time.unscaledTime < nextScrollTime)
            return;

        // หยุดที่ขอบ: เลื่อนเกิน 0-2 แล้วไม่เปลี่ยนอะไร
        int newLevel = Mathf.Clamp(currentLevel + (scroll > 0f ? 1 : -1), 0, 2);

        if (newLevel == currentLevel)
            return;

        nextScrollTime = Time.unscaledTime + scrollCooldown;
        SetLevel(newLevel);
    }


    // ==========================================
    // SET TARGET
    // XRayDetector เรียกใช้
    // ==========================================

    public void SetTarget(NPCXRayTarget target)
    {
        if (target == null)
            return;


        // ถ้าเป็น Target เดิม ไม่ต้องทำอะไร
        if (currentTarget == target)
            return;


        // ปิด X-Ray ของ Target เดิม
        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(0);
        }


        // เปลี่ยน Target
        currentTarget = target;


        // ใช้ Level ปัจจุบัน
        currentTarget.SetXRayLevel(
            currentLevel
        );


        Debug.Log(
            "X-Ray Target = " +
            currentTarget.gameObject.name
        );
    }


    // ==========================================
    // CLEAR TARGET
    // XRayDetector เรียกเมื่อ NPC ออกจากวง
    // ==========================================

    public void ClearTarget(NPCXRayTarget target)
    {
        if (target == null)
            return;


        if (currentTarget != target)
            return;


        // กลับเป็นปกติ
        target.SetXRayLevel(0);


        currentTarget = null;


        Debug.Log(
            "X-Ray Target = None"
        );
    }


    // ==========================================
    // SET LEVEL
    // ==========================================

    private void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, 2);

        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(currentLevel);
        }

        if (magnifyingGlass != null)
        {
            magnifyingGlass.SetHeldSprite(currentLevel);
        }

        Debug.Log(
            "X-Ray Level = " +
            (currentLevel + 1)
        );
    }


    // ==========================================
    // RESET
    // ==========================================

    public void ResetXRay()
    {
        currentLevel = 0;

        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(0);
            currentTarget = null;
        }

        if (magnifyingGlass != null)
        {
            magnifyingGlass.SetHeldSprite(0);
        }

        Debug.Log(
            "X-Ray Reset → Level 1 / Default"
        );
    }
}