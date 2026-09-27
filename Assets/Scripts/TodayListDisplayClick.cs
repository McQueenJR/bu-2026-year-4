using UnityEngine;

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

    private Renderer[] allRenderers;
    private int[] baseSortingOrders;

    private void Awake()
    {
        mainCamera = Camera.main;
        halfExtents = CalculateHalfExtents();
        originalZ = transform.position.z;
        originalPosition = transform.position;

        allRenderers = GetComponentsInChildren<Renderer>();
        baseSortingOrders = new int[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
            baseSortingOrders[i] = allRenderers[i].sortingOrder;

        DraggableSortOrder.OnOrderOverflow += ResetSortingOrder;
    }

    private void OnDestroy()
    {
        DraggableSortOrder.OnOrderOverflow -= ResetSortingOrder;
    }

    // =========================
    // เช็คปุ่มกลางทุกเฟรม ไม่ต้องพึ่ง OnMouseDown ของใครทั้งนั้น
    // ทำงานเฉพาะตอน GameObject นี้ active (คือตอน popup เปิดอยู่เท่านั้น)
    // =========================
    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
            TryToggleMarkUnderMouse();
    }

    public void ResetSortingOrder()
    {
        for (int i = 0; i < allRenderers.Length; i++)
            allRenderers[i].sortingOrder = baseSortingOrders[i];

        transform.position = originalPosition;
    }

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
        if (!isDragging) return;

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
        if (TodayListManager.Instance != null)
            TodayListManager.Instance.CloseTodayList();
    }

    // หา Mark ที่อยู่ใต้ตำแหน่งเมาส์ แล้วสั่ง toggle ให้ตรงๆ
    // ไม่สนใจว่า raycast ปกติจะเลือกใครเป็นผู้ชนะ
    private void TryToggleMarkUnderMouse()
    {
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPoint.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
        foreach (var h in hits)
        {
            TodayListMarkToggle mark = h.GetComponent<TodayListMarkToggle>();
            if (mark != null)
            {
                mark.ToggleMark();
                break;
            }
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

        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
                allRenderers[i].sortingOrder = order + baseSortingOrders[i];
        }

        Vector3 pos = transform.position;
        pos.z = originalZ - (order * 0.0001f);
        transform.position = pos;
    }

    public void RefreshSpriteCache()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
        baseSortingOrders = new int[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
            baseSortingOrders[i] = allRenderers[i].sortingOrder;
    }
}