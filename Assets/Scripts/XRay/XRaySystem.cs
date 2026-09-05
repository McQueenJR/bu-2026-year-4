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


    private void Update()
    {
        if (magnifyingGlass == null)
            return;

        // ถ้ายังไม่ได้ถือแว่น
        if (!magnifyingGlass.IsHolding)
            return;


        // ==========================================
        // กด 1 = ปกติ
        // ==========================================

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetLevel(0);
        }


        // ==========================================
        // กด 2 = ทะลุเสื้อผ้า
        // ==========================================

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetLevel(1);
        }


        // ==========================================
        // กด 3 = ทะลุทั้งร่าง
        // ==========================================

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetLevel(2);
        }
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