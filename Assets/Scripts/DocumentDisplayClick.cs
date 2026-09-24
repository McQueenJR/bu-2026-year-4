using UnityEngine;
using UnityEngine.Rendering;

// แปะที่ root ของเอกสารแต่ละใบ (ต้องมี BoxCollider2D บน root เดียวกัน)
// - เอกสารเดิม: root มี SpriteRenderer ตัวเดียว
// - Template ใหม่: root มี SortingGroup + ลูกหลายชิ้น (Paper, Photo, Text ...)
public class DocumentDisplayClick : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    private Camera mainCamera;

    [Header("กันลากออกนอกกรอบที่กำหนด (ไม่ใส่ก็ได้ถ้าไม่ต้องการ)")]
    public bool clampToBoundary = false;
    public BoxCollider2D dragBoundary;

    [Header("Template ใหม่: ใส่ Paper เพื่อใช้คำนวณขอบ (ว่าง = รวมทุก Renderer เหมือนเดิม)")]
    [SerializeField] private Renderer extentsSource;

    private Vector2 halfExtents;
    private float originalZ;

    private SpriteRenderer sr;
    private SortingGroup sortingGroup;
    private int baseSortingOrder;

    private void Awake()
    {
        mainCamera = Camera.main;
        halfExtents = CalculateHalfExtents();
        originalZ = transform.position.z;

        sr = GetComponent<SpriteRenderer>();
        sortingGroup = GetComponent<SortingGroup>();

        // มี SortingGroup → ใช้ group เป็นตัวจัดลำดับทั้งใบ (ลูกทุกชิ้นตามไปด้วย)
        if (sortingGroup != null)
            baseSortingOrder = sortingGroup.sortingOrder;
        else if (sr != null)
            baseSortingOrder = sr.sortingOrder;

        DraggableSortOrder.OnOrderOverflow += ResetSortingOrder;
    }

    public void SetDragBoundary(BoxCollider2D boundary)
    {
        dragBoundary = boundary;
        clampToBoundary = true;
    }

    private void OnDestroy()
    {
        DraggableSortOrder.OnOrderOverflow -= ResetSortingOrder;
    }

    public void ResetSortingOrder()
    {
        SetSortingOrder(baseSortingOrder);
    }

    private void SetSortingOrder(int order)
    {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = order;
        else if (sr != null)
            sr.sortingOrder = order;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            BringToFront();

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z;

            offset = transform.position - mouseWorldPos;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;

        Vector3 targetPos = mouseWorldPos + offset;

        if (clampToBoundary && dragBoundary != null)
            targetPos = ClampToBoundary(targetPos);

        transform.position = targetPos;
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
            isDragging = false;
    }

    private void OnMouseOver()
    {
        // คลิกขวา → ปิด popup เอกสาร
        if (Input.GetMouseButtonDown(1))
        {
            if (NPCSoundManager.Instance != null)
                NPCSoundManager.Instance.PlayDocumentClose();

            if (DocumentPopupManager.Instance != null)
                DocumentPopupManager.Instance.Close();
        }
    }

    private Vector2 CalculateHalfExtents()
    {
        // Template ใหม่: ใช้ขอบของ Paper อย่างเดียว (ไม่ให้ TMP / ลูกอื่นทำให้ขอบเพี้ยน)
        if (extentsSource != null)
        {
            Bounds paperBounds = extentsSource.bounds;
            return new Vector2(paperBounds.extents.x, paperBounds.extents.y);
        }

        // เอกสารเดิม: รวมทุก Renderer
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
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
        if (sortingGroup == null && sr == null) return;

        int order = DraggableSortOrder.GetNextOrder();
        SetSortingOrder(order + baseSortingOrder);

        Vector3 pos = transform.position;
        pos.z = originalZ - (order * 0.0001f);
        transform.position = pos;
    }
}