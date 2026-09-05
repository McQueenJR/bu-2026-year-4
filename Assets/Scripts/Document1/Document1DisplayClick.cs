using UnityEngine;

// ติดที่ documentRoot (ตัวที่มี Collider2D ครอบเต็มเอกสาร)
public class Document1DisplayClick : MonoBehaviour
{
    public Document1Manager manager;

    private bool isDragging = false;
    private Vector3 offset;

    private Camera mainCamera;

    [Header("กันลากออกนอกกรอบที่กำหนด")]
    public bool clampToBoundary = true;
    public BoxCollider2D dragBoundary;

    private Vector2 halfExtents;
    private float originalZ;
    private Vector3 originalPosition;

    private SpriteRenderer[] spriteRenderers;
    private int[] baseSortingOrders;
    private int currentOffset = 0;

    private void Awake()
    {
        mainCamera = Camera.main;
        halfExtents = CalculateHalfExtents();
        originalZ = transform.position.z;
        originalPosition = transform.position;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseSortingOrders = new int[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            baseSortingOrders[i] = spriteRenderers[i].sortingOrder;

        DraggableSortOrder.OnOrderOverflow += ResetSortingOrder;
    }

    private void OnDestroy()
    {
        DraggableSortOrder.OnOrderOverflow -= ResetSortingOrder;
    }

    public void ResetSortingOrder()
    {
        currentOffset = 0; // <-- เพิ่มบรรทัดนี้
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].sortingOrder = baseSortingOrders[i];

        transform.position = originalPosition;
    }

    // =========================
    // คลิกตรง documentRoot เอง (พื้นที่ที่ไม่มีปุ่มบัง)
    // =========================
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
            BeginDrag(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        if (isDragging)
            ContinueDrag(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            RequestClose();
    }

    // =========================
    // 👇 Public API — ให้ปุ่ม (TopTabs, NavButtons) เรียก forward เข้ามาได้
    // =========================
    public void BeginDrag(Vector3 mouseScreenPos)
    {
        isDragging = true;

        BringToFront();

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = transform.position.z;

        offset = transform.position - mouseWorldPos;
    }

    public void ContinueDrag(Vector3 mouseScreenPos)
    {
        if (!isDragging)
            return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = transform.position.z;

        Vector3 targetPos = mouseWorldPos + offset;

        if (clampToBoundary && dragBoundary != null)
            targetPos = ClampToBoundary(targetPos);

        transform.position = targetPos;
    }

    public void EndDrag()
    {
        isDragging = false;
    }

    public void RequestClose()
    {
        if (manager != null)
            manager.CloseDocument();
    }

    public void SetDragBoundary(BoxCollider2D boundary)
    {
        dragBoundary = boundary;
    }

    private Vector2 CalculateHalfExtents()
    {
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
        int order = DraggableSortOrder.GetNextOrder();
        currentOffset = order; // <-- เพิ่มบรรทัดนี้

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].sortingOrder = order + baseSortingOrders[i];
        }

        Vector3 pos = transform.position;
        pos.z = originalZ - (order * 0.0001f);
        transform.position = pos;
    }
    public void SetPageBaseOrder(GameObject page, int newBaseOrder)
    {
        if (page == null || spriteRenderers == null) return;

        var pageRenderers = page.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in pageRenderers)
        {
            int idx = System.Array.IndexOf(spriteRenderers, sr);
            if (idx >= 0)
            {
                baseSortingOrders[idx] = newBaseOrder;
                sr.sortingOrder = currentOffset + newBaseOrder;
            }
        }
    }
}