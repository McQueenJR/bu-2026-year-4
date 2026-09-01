using UnityEngine;

// ติดที่ TodaylistPanel (ตัวที่มี Collider2D ครอบเต็มพาเนลอยู่แล้ว)
public class TodayListDisplayClick : MonoBehaviour
{
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

    private void Awake()
    {
        mainCamera = Camera.main;
        halfExtents = CalculateHalfExtents();
        originalZ = transform.position.z;
        originalPosition = transform.position;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
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
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].sortingOrder = baseSortingOrders[i];   

        transform.position = originalPosition;
    }

    // =========================
    // ลาก
    // =========================
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
        if (!isDragging)
            return;

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
        {
            isDragging = false;
        }
    }

    // =========================
    // คลิกขวา → ปิด
    // =========================
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (TodayListManager.Instance != null)
                TodayListManager.Instance.CloseTodayList();
        }
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

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].sortingOrder = order + baseSortingOrders[i];
        }

        Vector3 pos = transform.position;
        pos.z = originalZ - (order * 0.0001f);
        transform.position = pos;
    }
    
    public void RefreshSpriteCache()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        baseSortingOrders = new int[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            baseSortingOrders[i] = spriteRenderers[i].sortingOrder;
    }
    
}