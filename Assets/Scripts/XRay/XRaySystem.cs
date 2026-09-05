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

        if (!magnifyingGlass.IsHolding)
            return;


        // ==========================================
        // หา NPC ใต้เมาส์
        // ==========================================

        FindTargetUnderMouse();


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
    // หา NPC ที่เมาส์กำลังชี้อยู่
    // ==========================================

    private void FindTargetUnderMouse()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;


        Vector3 mousePosition =
            Input.mousePosition;


        mousePosition.z =
            Mathf.Abs(
                cam.transform.position.z
            );


        Vector3 worldPosition =
            cam.ScreenToWorldPoint(
                mousePosition
            );


        // หา Collider ทั้งหมดตรงตำแหน่งเมาส์
        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                worldPosition
            );


        NPCXRayTarget foundTarget = null;


        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;


            // หา NPCXRayTarget จากตัวเองหรือ Parent
            NPCXRayTarget target =
                hit.GetComponentInParent<NPCXRayTarget>();


            if (target != null)
            {
                foundTarget = target;
                break;
            }
        }


        // ถ้าเจอ NPC
        if (foundTarget != null)
        {
            if (foundTarget != currentTarget)
            {
                SetTarget(foundTarget);
            }
        }
        else
        {
            // ถ้าไม่ได้ชี้ NPC
            if (currentTarget != null)
            {
                currentTarget.SetXRayLevel(0);
                currentTarget = null;
            }
        }
    }


    // ==========================================
    // SET TARGET
    // ==========================================

    public void SetTarget(NPCXRayTarget target)
    {
        if (currentTarget != null &&
            currentTarget != target)
        {
            currentTarget.SetXRayLevel(0);
        }


        currentTarget = target;


        if (currentTarget != null)
        {
            currentTarget.SetXRayLevel(
                currentLevel
            );
        }
    }


    // ==========================================
    // SET LEVEL
    // ==========================================

    private void SetLevel(int level)
    {
        currentLevel =
            Mathf.Clamp(level, 0, 2);


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


        Debug.Log(
            "X-Ray Reset → Level 1 / Default"
        );
    }
}