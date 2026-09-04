using UnityEngine;

public class NPCXRayTarget : MonoBehaviour

{

    // ==========================================

    // X-RAY PARTS

    // ==========================================

    private SpriteRenderer[] outerParts; // เสื้อผ้า / ของชั้นนอกสุด

    private SpriteRenderer[] faceParts;  // หน้า / ตา / ปาก / หู / ผม (เพิ่มใหม่)

    private SpriteRenderer[] bodyParts;  // ลำตัว / แขน / คอ

    private SpriteRenderer[] innerParts; // สมอง / กะโหลก / กระดูก

    private XRaySystem xraySystem;

    private MagnifyingGlass magnifyingGlass;

    // ==========================================

    // AWAKE

    // ==========================================

    private void Awake()

    {

        FindParts();

        // เริ่มต้นเป็นปกติ (Level 0)

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

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)

        {

            if (renderer.CompareTag("XRay_Outer"))

            {

                AddRenderer(ref outerParts, renderer);

            }

            else if (renderer.CompareTag("XRay_Face")) // ค้นหา Tag หน้า

            {

                AddRenderer(ref faceParts, renderer);

            }

            else if (renderer.CompareTag("XRay_Body"))

            {

                AddRenderer(ref bodyParts, renderer);

            }

            else if (renderer.CompareTag("XRay_Inner"))

            {

                AddRenderer(ref innerParts, renderer);

            }

        }

    }

    // ==========================================

    // MOUSE ENTER

    // ==========================================

    private void OnMouseEnter()

    {

        if (xraySystem == null)

        {

            xraySystem = FindFirstObjectByType<XRaySystem>();

        }

        if (magnifyingGlass == null)

        {

            magnifyingGlass = FindFirstObjectByType<MagnifyingGlass>();

        }

        if (xraySystem == null || magnifyingGlass == null) return;

        // ต้องถือแว่นก่อน

        if (!magnifyingGlass.IsHolding) return;

        xraySystem.SetTarget(this);

    }

    // ==========================================

    // SET X-RAY LEVEL

    // 0 = ปกติ

    // 1 = ทะลุเสื้อผ้า (เห็นลำตัว + หน้ายังโชว์)

    // 2 = ทะลุร่างทั้งหมด (เห็นสมอง/กระดูกด้านใน)

    // ==========================================

    public void SetXRayLevel(int level)

    {

        level = Mathf.Clamp(level, 0, 2);

        // LEVEL 1 (กด 1: ปกติ)

        if (level == 0)

        {

            SetMask(outerParts, SpriteMaskInteraction.None);

            SetMask(faceParts, SpriteMaskInteraction.None);

            SetMask(bodyParts, SpriteMaskInteraction.None);

            SetMask(innerParts, SpriteMaskInteraction.None);

        }

        // LEVEL 2 (กด 2: ทะลุเสื้อผ้า)

        else if (level == 1)

        {

            SetMask(outerParts, SpriteMaskInteraction.VisibleOutsideMask); // เสื้อผ้าโดนเจาะหาย

            SetMask(faceParts, SpriteMaskInteraction.None);               // หน้าแสดงผลปกติในวงแว่น!

            SetMask(bodyParts, SpriteMaskInteraction.None);               // ลำตัวแสดงผลปกติ

            SetMask(innerParts, SpriteMaskInteraction.None);              // ซ่อนอวัยวะด้านใน

        }

        // LEVEL 3 (กด 3: X-Ray ทะลุหมด)

        else if (level == 2)

        {

            SetMask(outerParts, SpriteMaskInteraction.VisibleOutsideMask); // เสื้อผ้าโดนเจาะหาย

            SetMask(faceParts, SpriteMaskInteraction.VisibleOutsideMask);  // หน้าโดนเจาะหาย!

            SetMask(bodyParts, SpriteMaskInteraction.VisibleOutsideMask);  // ลำตัวโดนเจาะหาย

            SetMask(innerParts, SpriteMaskInteraction.VisibleInsideMask);  // โชว์สมอง/กะโหลกในวงแว่น

        }

    }

    // ==========================================

    // SET MASK

    // ==========================================

    private void SetMask(SpriteRenderer[] renderers, SpriteMaskInteraction mode)

    {

        if (renderers == null) return;

        foreach (SpriteRenderer renderer in renderers)

        {

            if (renderer == null) continue;

            renderer.maskInteraction = mode;

        }

    }

    // ==========================================

    // ADD RENDERER

    // ==========================================

    private void AddRenderer(ref SpriteRenderer[] array, SpriteRenderer renderer)

    {

        if (renderer == null) return;

        // ป้องกันซ้ำ

        if (array != null)

        {

            foreach (SpriteRenderer existing in array)

            {

                if (existing == renderer) return;

            }

        }

        if (array == null)

        {

            array = new SpriteRenderer[] { renderer };

            return;

        }

        SpriteRenderer[] newArray = new SpriteRenderer[array.Length + 1];

        for (int i = 0; i < array.Length; i++)

        {

            newArray[i] = array[i];

        }

        newArray[array.Length] = renderer;

        array = newArray;

    }

}
 
