using System.Collections;
using UnityEngine;

public class Document1Manager : MonoBehaviour
{
    [System.Serializable]
    public class DocumentRow
    {
        [Tooltip("ใส่ไว้ดูเฉยๆ ใน Inspector เช่น A1, A2")]
        public string rowLabel;

        [Tooltip("หน้าเอกสารของแต่ละเต็นท์ในแถวนี้ เรียงตามลำดับ เช่น A101, A102")]
        public GameObject[] pages;
    }

    [Header("Root")]
    public GameObject documentRoot;

    [Header("Drag / Layer (แทน blocker เดิม)")]
    public Document1DisplayClick displayClick;

    [Header("Rows (แต่ละแถว = A1, A2, A3, A4 ...)")]
    public DocumentRow[] rows;

    [Header("Top Tabs (เรียงลำดับให้ตรงกับ rows[] เช่น index 0 = tab A1)")]
    [Tooltip("ลาก Transform ของปุ่ม tab แต่ละอันมาใส่ ตำแหน่ง Inspector ของมันตอนนี้จะกลายเป็น 'บ้าน' ของมัน")]
    public Transform[] rowTabs;

    [Header("Animation")]
    public float slideOutDuration = 0.18f;
    public float slideInDuration = 0.18f;
    public Vector3 sideOffset = new Vector3(-3f, -1.2f, 0f);
    public int frontBaseOrder = 10;
    public int backBaseOrder = 0;

    private GameObject[] flattenedPages;
    private Vector3[] restLocalPositions;
    private int[] rowStartIndex;
    private Vector3[] tabHomeLocalPos;

    private int frontMasterIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        BuildFlattenedList();
        BuildTabHomes();

        if (documentRoot != null) documentRoot.SetActive(false);
    }

    void BuildFlattenedList()
    {
        int total = 0;
        for (int r = 0; r < rows.Length; r++)
            total += (rows[r].pages != null) ? rows[r].pages.Length : 0;

        flattenedPages = new GameObject[total];
        restLocalPositions = new Vector3[total];
        rowStartIndex = new int[rows.Length];

        int idx = 0;
        for (int r = 0; r < rows.Length; r++)
        {
            rowStartIndex[r] = idx;
            if (rows[r].pages == null) continue;

            for (int p = 0; p < rows[r].pages.Length; p++)
            {
                flattenedPages[idx] = rows[r].pages[p];
                if (rows[r].pages[p] != null)
                    restLocalPositions[idx] = rows[r].pages[p].transform.localPosition;
                idx++;
            }
        }
    }

    void BuildTabHomes()
    {
        int n = (rowTabs != null) ? rowTabs.Length : 0;
        tabHomeLocalPos = new Vector3[n];
        for (int i = 0; i < n; i++)
            if (rowTabs[i] != null)
                tabHomeLocalPos[i] = rowTabs[i].localPosition;
    }

    int NextIndex(int i) => flattenedPages.Length == 0 ? 0 : (i + 1) % flattenedPages.Length;
    int PrevIndex(int i) => flattenedPages.Length == 0 ? 0 : (i - 1 + flattenedPages.Length) % flattenedPages.Length;

    // หา index ของแถว ถ้า page ที่ส่งมาเป็น "หน้าแรกของแถว" (หน้าที่มี tab ติดอยู่)
    // ถ้าไม่ใช่หน้าแรกของแถวไหนเลย คืน -1 (แปลว่าไม่มี tab ให้ตาม)
    int GetTabRowIndexForPage(GameObject page)
    {
        if (rows == null) return -1;
        for (int r = 0; r < rows.Length; r++)
        {
            if (rows[r].pages != null && rows[r].pages.Length > 0 && rows[r].pages[0] == page)
                return r;
        }
        return -1;
    }

    // =========================
    // OPEN / CLOSE
    // =========================
    public void OpenDocument()
    {
        if (documentRoot != null) documentRoot.SetActive(true);
        if (displayClick != null) displayClick.ResetSortingOrder();

        DraggableSortOrder.NotifyOpened();

        StopAllCoroutines();
        isAnimating = false;
        ShowFrontInstant(0);
    }

    public void CloseDocument()
    {
        if (documentRoot != null) documentRoot.SetActive(false);
        DraggableSortOrder.NotifyClosed();
    }

    void ShowFrontInstant(int idx)
    {
        if (flattenedPages == null || flattenedPages.Length == 0) return;

        for (int i = 0; i < flattenedPages.Length; i++)
        {
            if (flattenedPages[i] == null) continue;

            bool isFront = (i == idx);
            flattenedPages[i].SetActive(isFront);

            if (isFront)
            {
                flattenedPages[i].transform.localPosition = restLocalPositions[i];
                if (displayClick != null)
                    displayClick.SetPageBaseOrder(flattenedPages[i], frontBaseOrder);
            }
        }
        frontMasterIndex = idx;

        // รีเซ็ต tab ทุกอันกลับบ้านให้เรียบร้อยตอนเปิดเอกสารใหม่
        for (int r = 0; r < (rowTabs?.Length ?? 0); r++)
            if (rowTabs[r] != null) rowTabs[r].localPosition = tabHomeLocalPos[r];
    }

    // =========================
    // ปุ่มซ้าย/ขวา — วนลูปไม่รู้จบ
    // =========================
    public void NextPage()
    {
        if (isAnimating || flattenedPages == null || flattenedPages.Length <= 1) return;
        StartCoroutine(NextPageRoutine());
    }

    public void PrevPage()
    {
        if (isAnimating || flattenedPages == null || flattenedPages.Length <= 1) return;
        StartCoroutine(PrevPageRoutine());
    }

    private IEnumerator NextPageRoutine()
    {
        isAnimating = true;

        GameObject outgoing = flattenedPages[frontMasterIndex];
        int newFrontIdx = NextIndex(frontMasterIndex);
        GameObject incoming = flattenedPages[newFrontIdx];

        Vector3 restPos = restLocalPositions[frontMasterIndex];
        Vector3 sidePos = restPos + sideOffset;

        // เปิด incoming ไว้ล่วงหน้าที่ตำแหน่งพัก (ซ่อนหลัง outgoing) กันไม่ให้มีช่วงว่าง
        incoming.SetActive(true);
        incoming.transform.localPosition = restPos;
        displayClick.SetPageBaseOrder(incoming, backBaseOrder);
        displayClick.SetPageBaseOrder(outgoing, frontBaseOrder);

        // 1) ดึงหน้าปัจจุบันออกไปด้านข้าง -> incoming จะโผล่ออกมาที่ตำแหน่งพักทันทีที่ outgoing เริ่มเลื่อน
        yield return SlidePageRoutine(outgoing, restPos, restPos, sidePos, slideOutDuration);

        // 2) ปรับให้ incoming เป็นหน้าสุด, outgoing เป็นหลังสุด
        displayClick.SetPageBaseOrder(incoming, frontBaseOrder);
        displayClick.SetPageBaseOrder(outgoing, backBaseOrder);

        // 3) ยัดหน้าเดิมกลับเข้ามา (ถูกบังแล้วเพราะ order ต่ำกว่า)
        yield return SlidePageRoutine(outgoing, restPos, sidePos, restPos, slideInDuration);

        outgoing.SetActive(false);
        frontMasterIndex = newFrontIdx;
        isAnimating = false;
    }

    private IEnumerator PrevPageRoutine()
    {
        isAnimating = true;

        GameObject currentFront = flattenedPages[frontMasterIndex];
        int backIdx = PrevIndex(frontMasterIndex);
        GameObject incoming = flattenedPages[backIdx];

        Vector3 restPos = restLocalPositions[frontMasterIndex];
        Vector3 sidePos = restPos + sideOffset;

        incoming.SetActive(true);
        incoming.transform.localPosition = restPos;
        displayClick.SetPageBaseOrder(incoming, backBaseOrder);

        yield return SlidePageRoutine(incoming, restPos, restPos, sidePos, slideOutDuration);

        displayClick.SetPageBaseOrder(incoming, frontBaseOrder);
        displayClick.SetPageBaseOrder(currentFront, backBaseOrder); // <-- เพิ่มบรรทัดนี้ กันชนกัน

        yield return SlidePageRoutine(incoming, restPos, sidePos, restPos, slideInDuration);

        currentFront.SetActive(false);
        frontMasterIndex = backIdx;
        isAnimating = false;
    }

    // =========================
    // ปุ่ม TopTab
    // =========================
    public void GoToRow(int rowIndex)
    {
        if (isAnimating || rows == null || rowIndex < 0 || rowIndex >= rows.Length)
        {
            Debug.LogWarning("Document1Manager: rowIndex ไม่ถูกต้อง -> " + rowIndex);
            return;
        }

        if (rows[rowIndex].pages == null || rows[rowIndex].pages.Length == 0)
        {
            Debug.LogWarning("Document1Manager: แถว " + rows[rowIndex].rowLabel + " ยังไม่มีหน้าเลย");
            return;
        }

        int targetIdx = rowStartIndex[rowIndex];
        if (targetIdx == frontMasterIndex) return;

        StartCoroutine(GoToIndexRoutine(targetIdx));
    }

    private IEnumerator GoToIndexRoutine(int targetIdx)
    {
        isAnimating = true;

        GameObject currentFront = flattenedPages[frontMasterIndex];
        GameObject target = flattenedPages[targetIdx];

        Vector3 restPos = restLocalPositions[frontMasterIndex];
        Vector3 sidePos = restPos + sideOffset;

        target.SetActive(true);
        target.transform.localPosition = restPos;
        displayClick.SetPageBaseOrder(target, backBaseOrder);

        yield return SlidePageRoutine(target, restPos, restPos, sidePos, slideOutDuration);

        displayClick.SetPageBaseOrder(target, frontBaseOrder);
        displayClick.SetPageBaseOrder(currentFront, backBaseOrder); // <-- เพิ่มบรรทัดนี้ กันชนกัน

        yield return SlidePageRoutine(target, restPos, sidePos, restPos, slideInDuration);

        currentFront.SetActive(false);
        frontMasterIndex = targetIdx;
        isAnimating = false;
    }

    // =========================
    // เลื่อนตำแหน่ง page + ลาก tab ของแถวนั้น (ถ้ามี) ให้ตามไปด้วย
    // =========================
    private IEnumerator SlidePageRoutine(GameObject page, Vector3 restPos, Vector3 from, Vector3 to, float duration)
    {
        Transform t = page.transform;

        int tabRow = GetTabRowIndexForPage(page);
        Transform tab = (tabRow >= 0 && rowTabs != null && tabRow < rowTabs.Length) ? rowTabs[tabRow] : null;
        Vector3 tabHome = (tab != null) ? tabHomeLocalPos[tabRow] : Vector3.zero;

        void Apply(Vector3 pos)
        {
            t.localPosition = pos;
            if (tab != null)
                tab.localPosition = tabHome + (pos - restPos);
        }

        if (duration <= 0f)
        {
            Apply(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            p = p * p * (3f - 2f * p); // smoothstep
            Apply(Vector3.Lerp(from, to, p));
            yield return null;
        }
        Apply(to);
    }
}