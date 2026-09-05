using UnityEngine;

public class NPCXRayTarget : MonoBehaviour
{
    // ==========================================
    // X-RAY PARTS
    // ==========================================

    private SpriteRenderer[] outerParts; // เสื้อผ้า / ของชั้นนอก
    private SpriteRenderer[] faceParts;  // หน้า / ตา / ปาก / หู / ผม
    private SpriteRenderer[] bodyParts;  // ลำตัว / แขน / คอ
    private SpriteRenderer[] innerParts; // สมอง / กะโหลก / กระดูก


    // ==========================================
    // AWAKE
    // ==========================================

    private void Awake()
    {
        FindParts();

        // เริ่มต้นเป็นปกติ
        SetXRayLevel(0);
    }


    // ==========================================
    // FIND PARTS
    // ==========================================

    private void FindParts()
    {
        outerParts = null;
        faceParts = null;
        bodyParts = null;
        innerParts = null;

        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            // เสื้อผ้า
            if (renderer.CompareTag("XRay_Outer"))
            {
                AddRenderer(
                    ref outerParts,
                    renderer
                );
            }

            // ใบหน้า
            else if (renderer.CompareTag("XRay_Face"))
            {
                AddRenderer(
                    ref faceParts,
                    renderer
                );
            }

            // ร่างกาย
            else if (renderer.CompareTag("XRay_Body"))
            {
                AddRenderer(
                    ref bodyParts,
                    renderer
                );
            }

            // อวัยวะภายใน
            else if (renderer.CompareTag("XRay_Inner"))
            {
                AddRenderer(
                    ref innerParts,
                    renderer
                );
            }
        }

        Debug.Log(
            gameObject.name +
            " | Outer = " + Count(outerParts) +
            " | Face = " + Count(faceParts) +
            " | Body = " + Count(bodyParts) +
            " | Inner = " + Count(innerParts)
        );
    }


    // ==========================================
    // SET X-RAY LEVEL
    //
    // Level 0 = ปกติ
    // Level 1 = ทะลุเสื้อผ้า
    // Level 2 = ทะลุร่างกาย
    // ==========================================

    public void SetXRayLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 2);


        // ======================================
        // LEVEL 0
        // ปกติ
        // ======================================

        if (level == 0)
        {
            SetMask(
                outerParts,
                SpriteMaskInteraction.None
            );

            SetMask(
                faceParts,
                SpriteMaskInteraction.None
            );

            SetMask(
                bodyParts,
                SpriteMaskInteraction.None
            );

            SetMask(
                innerParts,
                SpriteMaskInteraction.None
            );
        }


        // ======================================
        // LEVEL 1
        // ทะลุเสื้อผ้า
        // ======================================

        else if (level == 1)
        {
            // เสื้อผ้าหายเฉพาะในวงแว่น
            SetMask(
                outerParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // หน้าแสดงปกติ
            SetMask(
                faceParts,
                SpriteMaskInteraction.None
            );

            // ตัวแสดงปกติ
            SetMask(
                bodyParts,
                SpriteMaskInteraction.None
            );

            // อวัยวะภายในยังไม่แสดง
            SetMask(
                innerParts,
                SpriteMaskInteraction.None
            );
        }


        // ======================================
        // LEVEL 2
        // X-RAY ทะลุทั้งร่างกาย
        // ======================================

        else if (level == 2)
        {
            // เสื้อผ้าหาย
            SetMask(
                outerParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // หน้าหาย
            SetMask(
                faceParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // ร่างกายหาย
            SetMask(
                bodyParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // อวัยวะภายในเห็นเฉพาะในวงแว่น
            SetMask(
                innerParts,
                SpriteMaskInteraction.VisibleInsideMask
            );
        }
    }


    // ==========================================
    // SET MASK
    // ==========================================

    private void SetMask(
        SpriteRenderer[] renderers,
        SpriteMaskInteraction mode)
    {
        if (renderers == null)
            return;

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            renderer.maskInteraction = mode;
        }
    }


    // ==========================================
    // ADD RENDERER
    // ==========================================

    private void AddRenderer(
        ref SpriteRenderer[] array,
        SpriteRenderer renderer)
    {
        if (renderer == null)
            return;


        // ป้องกัน Renderer ซ้ำ
        if (array != null)
        {
            foreach (SpriteRenderer existing in array)
            {
                if (existing == renderer)
                    return;
            }
        }


        // ถ้ายังไม่มี Array
        if (array == null)
        {
            array = new SpriteRenderer[]
            {
                renderer
            };

            return;
        }


        // สร้าง Array ใหม่
        SpriteRenderer[] newArray =
            new SpriteRenderer[array.Length + 1];


        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }


        newArray[array.Length] = renderer;

        array = newArray;
    }


    // ==========================================
    // COUNT
    // ==========================================

    private int Count(SpriteRenderer[] array)
    {
        if (array == null)
            return 0;

        return array.Length;
    }
}