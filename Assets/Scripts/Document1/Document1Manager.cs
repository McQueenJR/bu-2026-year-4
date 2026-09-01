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
    public Document1DisplayClick displayClick; // ติดอยู่ที่ documentRoot

    [Header("Rows (แต่ละแถว = A1, A2, A3, A4 ... เพิ่ม/ลดเต็นท์ที่ pages ของแต่ละแถวได้เลย)")]
    public DocumentRow[] rows;

    private GameObject[] flattenedPages;
    private int[] rowStartIndex;
    private int currentIndex = 0;

    void Start()
    {
        BuildFlattenedList();

        if (documentRoot != null) documentRoot.SetActive(false);
    }

    void BuildFlattenedList()
    {
        int total = 0;
        for (int r = 0; r < rows.Length; r++)
            total += (rows[r].pages != null) ? rows[r].pages.Length : 0;

        flattenedPages = new GameObject[total];
        rowStartIndex = new int[rows.Length];

        int idx = 0;
        for (int r = 0; r < rows.Length; r++)
        {
            rowStartIndex[r] = idx;
            if (rows[r].pages == null) continue;

            for (int p = 0; p < rows[r].pages.Length; p++)
            {
                flattenedPages[idx] = rows[r].pages[p];
                idx++;
            }
        }
    }

    // =========================
    // OPEN / CLOSE
    // =========================
    public void OpenDocument()
    {
        if (documentRoot != null) documentRoot.SetActive(true);

        if (displayClick != null)
            displayClick.ResetSortingOrder(); // รีตำแหน่ง/เลเยอร์ทุกครั้งที่เปิดใหม่

        DraggableSortOrder.NotifyOpened();

        ShowPage(0);
    }

    public void CloseDocument()
    {
        if (documentRoot != null) documentRoot.SetActive(false);

        DraggableSortOrder.NotifyClosed();
    }

    // =========================
    // ปุ่มซ้าย/ขวา ไล่ทุกหน้ารวมกันเป็นเส้นเดียว ไม่สนแถว
    // =========================
    public void NextPage()
    {
        if (flattenedPages == null || flattenedPages.Length == 0) return;

        int next = currentIndex + 1;
        if (next >= flattenedPages.Length)
        {
            Debug.Log("Document1Manager: ถึงหน้าสุดท้ายแล้ว");
            return;
        }

        ShowPage(next);
    }

    public void PrevPage()
    {
        if (flattenedPages == null || flattenedPages.Length == 0) return;

        int prev = currentIndex - 1;
        if (prev < 0)
        {
            Debug.Log("Document1Manager: อยู่หน้าแรกสุดแล้ว");
            return;
        }

        ShowPage(prev);
    }

    // =========================
    // ปุ่มลิสด้านบน (A1, A2, A3, A4 ...) กระโดดไปเต็นท์แรกของแถวนั้น
    // =========================
    public void GoToRow(int rowIndex)
    {
        if (rows == null || rowIndex < 0 || rowIndex >= rows.Length)
        {
            Debug.LogWarning("Document1Manager: rowIndex ไม่ถูกต้อง -> " + rowIndex);
            return;
        }

        if (rows[rowIndex].pages == null || rows[rowIndex].pages.Length == 0)
        {
            Debug.LogWarning("Document1Manager: แถว " + rows[rowIndex].rowLabel + " ยังไม่มีหน้าเลย");
            return;
        }

        ShowPage(rowStartIndex[rowIndex]);
    }

    // =========================
    // แสดงหน้าตาม index ในเส้นรวม (flattened)
    // =========================
    void ShowPage(int flatIndex)
    {
        if (flattenedPages == null || flatIndex < 0 || flatIndex >= flattenedPages.Length)
            return;

        for (int i = 0; i < flattenedPages.Length; i++)
        {
            if (flattenedPages[i] != null)
                flattenedPages[i].SetActive(i == flatIndex);
        }

        currentIndex = flatIndex;
    }
}