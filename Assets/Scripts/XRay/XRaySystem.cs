
using UnityEngine;

public class XRaySystem : MonoBehaviour
{
    [Header("Magnifying Glass")]
    [SerializeField]
    private MagnifyingGlass magnifyingGlass;

    private NPCXRayTarget currentTarget;

    // ==========================================
    // X-Ray Level
    //
    // 0 = ปกติ (Default)
    // 1 = ทะลุเสื้อผ้า
    // 2 = ทะลุร่างกาย / เห็นอวัยวะภายใน
    // ==========================================

    private int currentLevel = 0;

    private void Update()
    {
        if (magnifyingGlass == null)
            return;

        // ต้องถือแว่นก่อน
        if (!magnifyingGlass.IsHolding)
            return;

        // ======================================
        // เลข 1 = ปกติ
        // ======================================

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetLevel(0);
        }

        // ======================================
        // เลข 2 = ทะลุเสื้อผ้า
        // ======================================

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetLevel(1);
        }

        // ======================================
        // เลข 3 = ทะลุร่างกาย
        // เห็นอวัยวะภายใน
        // ======================================

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetLevel(2);
        }
    }

    // ==========================================
    // ตั้ง NPC ที่กำลังถูกส่อง
    // ==========================================

    public void SetTarget(NPCXRayTarget target)
    {
        // คืน NPC เก่าเป็นปกติ
        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(0);
        }

        currentTarget = target;

        // ถ้ามี NPC ใหม่
        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(currentLevel);
        }
    }

    // ==========================================
    // เปลี่ยนระดับ X-Ray
    // ==========================================

    private void SetLevel(int level)
    {
        currentLevel =
            Mathf.Clamp(level, 0, 2);

        // ส่งระดับไปยัง NPC
        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(
                currentLevel
            );
        }

        Debug.Log(
            "X-Ray Level = " +
            (currentLevel + 1)
        );
    }

    // ==========================================
    // Reset X-Ray
    //
    // เรียกใช้ตอนวางแว่น
    // กลับเป็น Default / Level 1
    // ==========================================

    public void ResetXRay()
    {
        // กลับเป็น Default
        currentLevel = 0;

        // คืน NPC ที่กำลังถูกส่องให้ปกติ
        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(0);

            currentTarget = null;
        }

        Debug.Log(
            "X-Ray Reset → Level 1 / Default"
        );
    }
}

