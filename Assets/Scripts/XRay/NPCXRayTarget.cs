
using UnityEngine;

public class NPCXRayTarget : MonoBehaviour
{
    // =========================================
    // X-RAY PARTS
    // =========================================

    private SpriteRenderer[] outerParts;
    private SpriteRenderer[] bodyParts;
    private SpriteRenderer[] innerParts;

    private XRaySystem xraySystem;
    private MagnifyingGlass magnifyingGlass;

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        FindParts();

        // เริ่มต้นเป็นปกติ
        SetXRayLevel(0);
    }

    // =========================================
    // FIND PARTS
    // =========================================

    private void FindParts()
    {
        outerParts = null;
        bodyParts = null;
        innerParts = null;

        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.CompareTag("XRay_Outer"))
            {
                AddRenderer(
                    ref outerParts,
                    renderer
                );
            }
            else if (renderer.CompareTag("XRay_Body"))
            {
                AddRenderer(
                    ref bodyParts,
                    renderer
                );
            }
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
            " | Body = " + Count(bodyParts) +
            " | Inner = " + Count(innerParts)
        );
    }

    // =========================================
    // MOUSE ENTER
    // =========================================

    private void OnMouseEnter()
    {
        if (xraySystem == null)
        {
            xraySystem =
                FindFirstObjectByType<XRaySystem>();
        }

        if (magnifyingGlass == null)
        {
            magnifyingGlass =
                FindFirstObjectByType<MagnifyingGlass>();
        }

        if (xraySystem == null)
            return;

        if (magnifyingGlass == null)
            return;

        // ต้องถือแว่น
        if (!magnifyingGlass.IsHolding)
            return;

        xraySystem.SetTarget(this);
    }

    // =========================================
    // SET X-RAY LEVEL
    //
    // 0 = ปกติ
    // 1 = ทะลุเสื้อผ้า
    // 2 = ทะลุร่างกาย
    // =========================================

    public void SetXRayLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 2);

        // -------------------------------------
        // LEVEL 1
        // ปกติ
        // -------------------------------------

        if (level == 0)
        {
            SetMask(
                outerParts,
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

        // -------------------------------------
        // LEVEL 2
        // ทะลุเสื้อผ้า
        // -------------------------------------

        else if (level == 1)
        {
            // เสื้อหายในวงแว่น
            SetMask(
                outerParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // ลำตัว + แขนยังเห็น
            SetMask(
                bodyParts,
                SpriteMaskInteraction.None
            );

            // ยังไม่เห็นอวัยวะ
            SetMask(
                innerParts,
                SpriteMaskInteraction.None
            );
        }

        // -------------------------------------
        // LEVEL 3
        // ทะลุลำตัว
        // -------------------------------------

        else if (level == 2)
        {
            // เสื้อหายในวง
            SetMask(
                outerParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // ลำตัว + แขนหายในวง
            SetMask(
                bodyParts,
                SpriteMaskInteraction.VisibleOutsideMask
            );

            // Inner เห็นในวง
            SetMask(
                innerParts,
                SpriteMaskInteraction.VisibleInsideMask
            );
        }
    }

    // =========================================
    // SET MASK
    // =========================================

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

    // =========================================
    // ADD RENDERER
    // =========================================

    private void AddRenderer(
        ref SpriteRenderer[] array,
        SpriteRenderer renderer)
    {
        if (renderer == null)
            return;

        // ป้องกันซ้ำ
        if (array != null)
        {
            foreach (SpriteRenderer existing in array)
            {
                if (existing == renderer)
                    return;
            }
        }

        if (array == null)
        {
            array = new SpriteRenderer[]
            {
                renderer
            };

            return;
        }

        SpriteRenderer[] newArray =
            new SpriteRenderer[array.Length + 1];

        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }

        newArray[array.Length] = renderer;

        array = newArray;
    }

    // =========================================
    // COUNT
    // =========================================

    private int Count(
        SpriteRenderer[] array)
    {
        if (array == null)
            return 0;

        return array.Length;
    }
}