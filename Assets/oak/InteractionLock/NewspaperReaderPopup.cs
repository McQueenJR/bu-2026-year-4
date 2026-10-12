using System;
using UnityEngine;

// หน้าอ่านหนังสือพิมพ์ใหญ่ (World Space) — เปิดโดย NewspaperDeliveryManager
// ข้างในมี Prefab แม่แบบ (NewspaperView) ที่ถูก Bind ข้อมูลก่อนเปิดทุกครั้ง
// ปิดด้วยคลิกขวา ไม่มีระบบลาก
public class NewspaperReaderPopup : MonoBehaviour
{
    [Header("แม่แบบหนังสือพิมพ์ (instance ที่เป็นลูกของ object นี้)")]
    [SerializeField] private NewspaperView view;

    [Tooltip("object ที่จะย่อ/ขยายให้พอดีจอ (ปกติคือ object เดียวกับ View ว่าง = ใช้ View)")]
    [SerializeField] private Transform contentRoot;

    [Tooltip("SpriteRenderer ของกระดาษพื้นหลัง ใช้วัดความสูงเพื่อ Fit")]
    [SerializeField] private SpriteRenderer paperRenderer;

    [Tooltip("บวกเพิ่มกับ Sorting Order ของทุก Renderer ในแม่แบบ ให้อยู่บนสุดของฉาก")]
    [SerializeField] private int sortingOrderOffset = 500;

    [Header("ปรับขนาดอัตโนมัติ (0 = ใช้ขนาดตามที่ตั้งไว้)")]
    [Tooltip("ความสูงเป็นหน่วยโลก แนะนำ ≈ Camera Orthographic Size × 2 × 0.9")]
    [SerializeField] private float fitHeight = 0f;

    private Renderer[] renderers;
    private int[] baseSortingOrders;
    private Vector3 baseScale = Vector3.one;
    private bool initialized = false;

    private Action onClosed;
    private int openedFrame = -1;

    public bool IsOpen { get; private set; }

    // Awake จะไม่ทำงานจนกว่า object จะถูกเปิดครั้งแรก จึงตั้งค่าเริ่มต้นตอนเรียก Open แทน
    private void EnsureInit()
    {
        if (initialized)
            return;

        initialized = true;

        if (contentRoot == null && view != null)
            contentRoot = view.transform;

        if (contentRoot != null)
            baseScale = contentRoot.localScale;

        renderers = GetComponentsInChildren<Renderer>(true);
        baseSortingOrders = new int[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            baseSortingOrders[i] = renderers[i].sortingOrder;
    }

    public void Open(NewspaperRuntime data, Action closedCallback)
    {
        if (IsOpen || view == null || data == null)
            return;

        EnsureInit();

        // เติมข้อมูลลงแม่แบบก่อนเปิด
        view.Bind(data);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].sortingOrder = baseSortingOrders[i] + sortingOrderOffset;
        }

        onClosed = closedCallback;
        IsOpen = true;
        openedFrame = Time.frameCount;

        gameObject.SetActive(true);

        FitToScreen();
    }

    private void FitToScreen()
    {
        if (fitHeight <= 0f || contentRoot == null || paperRenderer == null)
            return;

        contentRoot.localScale = baseScale;

        float currentHeight = paperRenderer.bounds.size.y;

        if (currentHeight < 0.0001f)
        {
            Debug.LogWarning("[Newspaper] Paper Renderer ไม่มี Sprite หรือขนาดเป็น 0 → Fit ไม่ได้", this);
            return;
        }

        contentRoot.localScale = baseScale * (fitHeight / currentHeight);
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Time.frameCount == openedFrame) return;

        // คลิกขวา → ปิด
        if (Input.GetMouseButtonDown(1))
            Close();
    }

    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        gameObject.SetActive(false);

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }
}