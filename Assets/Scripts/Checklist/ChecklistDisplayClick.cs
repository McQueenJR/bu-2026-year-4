using UnityEngine;

/// <summary>
/// รวมพฤติกรรมของ ChecklistClickAbsorber + ChecklistPanelRightClickClose เดิม
/// เข้าไว้ในตัวเดียว พร้อมเพิ่มระบบลาก (คลิกค้าง-ลาก) แบบเดียวกับ BagDisplayClick
///
/// วิธีใช้: แปะสคริปต์นี้ไว้ที่ GameObject "Background" (ลูกของ ChecklistPanel)
/// ซึ่งต้องมี Collider2D ครอบคลุมพื้นที่ทั้งสมุด — สคริปต์จะลาก/จัดเลเยอร์
/// "ตัวพ่อ" (ChecklistPanel) ทั้งก้อน ไม่ใช่แค่ตัว Background เอง
/// เพื่อให้ toggle ทุกช่องเคลื่อนตามไปด้วยตอนลากสมุด
/// </summary>
public class ChecklistDisplayClick : MonoBehaviour
{
    [Header("Checklist")]
    [SerializeField] private ChecklistManager checklistManager;

    [Header("กันลากออกนอกกรอบที่กำหนด")]
    public bool clampToBoundary = true;
    public BoxCollider2D dragBoundary;

    private bool isDragging = false;
    private Vector3 offset;

    private Camera mainCamera;

    // ตัวที่จะถูกลากจริง = พ่อของ Background (คือ ChecklistPanel)
    // ถ้าไม่มีพ่อ (เผื่อไว้) จะลากตัวเองแทน
    private Transform panelTransform;

    private Vector2 halfExtents;
    private float originalZ;
    private Vector3 originalPosition; // ตำแหน่งกลางจอเริ่มต้น ใช้รีเซ็ตทุกครั้งที่เปิดใหม่

    private SpriteRenderer[] spriteRenderers;
    private int[] baseSortingOrders;

    private void Awake()
    {
        mainCamera = Camera.main;

        panelTransform = transform.parent != null ? transform.parent : transform;

        halfExtents = CalculateHalfExtents();
        originalZ = panelTransform.position.z;
        originalPosition = panelTransform.position;

        spriteRenderers = panelTransform.GetComponentsInChildren<SpriteRenderer>();
        baseSortingOrders = new int[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            baseSortingOrders[i] = spriteRenderers[i].sortingOrder;

        DraggableSortOrder.OnOrderOverflow += ResetSortingOrder;
    }

    private void OnDestroy()
    {
        DraggableSortOrder.OnOrderOverflow -= ResetSortingOrder;
    }

    // ★ ทุกครั้งที่ panel ถูกเปิดขึ้นมาใหม่ (SetActive(true)) ให้กลับไปอยู่ตำแหน่งกลางจอเสมอ
    private void OnEnable()
    {
        if (panelTransform != null)
            panelTransform.position = originalPosition;
    }

    public void ResetSortingOrder()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].sortingOrder = baseSortingOrders[i];

        panelTransform.position = originalPosition;
    }

    private void OnMouseDown()
    {
        // คลิกซ้าย → เริ่มลาก
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;

            BringToFront();

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = panelTransform.position.z;

            offset = panelTransform.position - mouseWorldPos;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = panelTransform.position.z;

        Vector3 targetPos = mouseWorldPos + offset;

        if (clampToBoundary && dragBoundary != null)
            targetPos = ClampToBoundary(targetPos);

        panelTransform.position = targetPos;
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void OnMouseOver()
    {
        // คลิกขวา → ปิด checklist
        if (Input.GetMouseButtonDown(1))
        {
            if (checklistManager != null)
                checklistManager.CloseChecklist();
        }
    }

    private Vector2 CalculateHalfExtents()
    {
        Renderer[] renderers = panelTransform.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return Vector2.zero;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return new Vector2(bounds.extents.x, bounds.extents.y);
    }

    private Vector3 ClampToBoundary(Vector3 pos)
    {
        Bounds b = dragBoundary.bounds;

        pos.x = Mathf.Clamp(pos.x, b.min.x + halfExtents.x, b.max.x - halfExtents.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y + halfExtents.y, b.max.y - halfExtents.y);

        return pos;
    }

    private void BringToFront()
    {
        int order = DraggableSortOrder.GetNextOrder();

        if (spriteRenderers.Length == 0) return;

        int minOrder = spriteRenderers[0].sortingOrder;
        for (int i = 1; i < spriteRenderers.Length; i++)
            if (spriteRenderers[i].sortingOrder < minOrder)
                minOrder = spriteRenderers[i].sortingOrder;

        int offsetOrder = order - minOrder;

        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].sortingOrder += offsetOrder;

        Vector3 pos = panelTransform.position;
        pos.z = originalZ - (order * 0.0001f);
        panelTransform.position = pos;
    }
}