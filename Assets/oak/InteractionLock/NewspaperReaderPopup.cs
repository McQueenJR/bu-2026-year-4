using System;
using UnityEngine;

// หน้าอ่านหนังสือพิมพ์ใหญ่ (World Space) — เปิดโดย NewspaperDeliveryManager
// ข้างในมี Prefab แม่แบบ (NewspaperView) ที่ถูก Bind ข้อมูลก่อนเปิดทุกครั้ง
// ปิดด้วยคลิกขวา ไม่มีระบบลาก
public class NewspaperReaderPopup : MonoBehaviour
{
    [Serializable]
    public class DayTemplate
    {
        [Min(1)] public int day = 1;

        [Tooltip("Prefab แม่แบบของวันนั้น (ต้องมี NewspaperView ที่ root)")]
        public NewspaperView template;
    }
    
    [Header("แม่แบบหนังสือพิมพ์ (instance ที่เป็นลูกของ object นี้)")]
    [SerializeField] private NewspaperView view;

    [Tooltip("object ที่จะย่อ/ขยายให้พอดีจอ (ปกติคือ object เดียวกับ View ว่าง = ใช้ View)")]
    [SerializeField] private Transform contentRoot;

    [Tooltip("SpriteRenderer ของกระดาษพื้นหลัง ใช้วัดความสูงเพื่อ Fit")]
    [SerializeField] private SpriteRenderer paperRenderer;
    
    [Header("Template แยกตามวัน (วันที่ไม่ใส่ = ใช้ View เดิมด้านบน)")]
    [SerializeField] private DayTemplate[] dayTemplates;

    [Tooltip("บวกเพิ่มกับ Sorting Order ของทุก Renderer ในแม่แบบ ให้อยู่บนสุดของฉาก")]
    [SerializeField] private int sortingOrderOffset = 500;

    [Header("ปรับขนาดอัตโนมัติ (0 = ใช้ขนาดตามที่ตั้งไว้)")]
    [Tooltip("ความสูงเป็นหน่วยโลก แนะนำ ≈ Camera Orthographic Size × 2 × 0.9")]
    [SerializeField] private float fitHeight = 0f;

    private Renderer[] renderers;
    private int[] baseSortingOrders;
    private Vector3 baseScale = Vector3.one;
    private bool initialized = false;
    
    // instance ของ Template รายวันที่กำลังเปิดอยู่ (null = ใช้ View เดิม)
    private NewspaperView activeInstance;
    
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
        if (IsOpen || data == null)
            return;

        EnsureInit();
        DestroyActiveInstance();

        NewspaperView dayPrefab = FindTemplate(data.day);

        if (dayPrefab == null && view == null)
            return;

        Transform root;
        Vector3 rootBaseScale;
        SpriteRenderer paper;

        if (dayPrefab != null)
        {
            // ----- ใช้ Template ของวันนั้น -----
            if (view != null)
                view.gameObject.SetActive(false);

            Transform parent = view != null && view.transform.parent != null
                ? view.transform.parent
                : transform;

            // worldPositionStays = false → คงตำแหน่ง/สเกล local ตามที่จัดไว้ใน Prefab
            activeInstance = Instantiate(dayPrefab, parent, false);
            activeInstance.name = dayPrefab.name;

            activeInstance.Bind(data);

            foreach (Renderer r in activeInstance.GetComponentsInChildren<Renderer>(true))
                r.sortingOrder += sortingOrderOffset;

            root = activeInstance.transform;
            rootBaseScale = root.localScale;
            paper = activeInstance.PaperRenderer;

            if (paper == null && fitHeight > 0f)
            {
                Debug.LogWarning(
                    $"[Newspaper] Template '{dayPrefab.name}' ไม่ได้ใส่ Paper Renderer → ข้ามการ Fit ขนาด",
                    dayPrefab);
            }
        }
        else
        {
            // ----- ไม่มี Template รายวัน → ใช้ View เดิม (พฤติกรรมเดิม) -----
            view.gameObject.SetActive(true);

            view.Bind(data);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].sortingOrder = baseSortingOrders[i] + sortingOrderOffset;
            }

            root = contentRoot;
            rootBaseScale = baseScale;
            paper = paperRenderer;
        }

        onClosed = closedCallback;
        IsOpen = true;
        openedFrame = Time.frameCount;

        gameObject.SetActive(true);

        FitToScreen(root, rootBaseScale, paper);
    }

    private NewspaperView FindTemplate(int day)
    {
        if (dayTemplates == null)
            return null;

        foreach (DayTemplate entry in dayTemplates)
        {
            if (entry != null && entry.day == day && entry.template != null)
                return entry.template;
        }

        return null;
    }

    private void FitToScreen(Transform root, Vector3 rootBaseScale, SpriteRenderer paper)
    {
        if (fitHeight <= 0f || root == null || paper == null)
            return;

        root.localScale = rootBaseScale;

        float currentHeight = paper.bounds.size.y;

        if (currentHeight < 0.0001f)
        {
            Debug.LogWarning("[Newspaper] Paper Renderer ไม่มี Sprite หรือขนาดเป็น 0 → Fit ไม่ได้", this);
            return;
        }

        root.localScale = rootBaseScale * (fitHeight / currentHeight);
    }

    private void DestroyActiveInstance()
    {
        if (activeInstance == null)
            return;

        Destroy(activeInstance.gameObject);
        activeInstance = null;
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
        DestroyActiveInstance();

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }
}