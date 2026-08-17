using UnityEngine;

public class Document2Manager : MonoBehaviour
{
    [Header("Root")]
    public GameObject documentRoot;   // ตัวเอกสารทั้งก้อน (พาเรนต์รวมทุกหน้า + ปุ่มลิสต์)

    [Header("Pages")]
    public GameObject[] pages;        // เนื้อหาเต็มหน้ากระดาษแต่ละหน้า

    [Header("PostIt (ลาก PostIt ตัวเดียวกัน เรียงให้ตรงลำดับกับ Pages ด้านบน)")]
    public GameObject[] postIts;      // เช่น index 0 = F2 theifPostIt_0, index 1 = F2 VillagerPostIt_0 ...

    [Header("Sorting (ปรับแค่ 2 ค่านี้พอ)")]
    public int frontOrder = 20;   // ค่านี้ต้อง "มากกว่า" Order in Layer ของกระดาษ
    public int backOrder = 0;     // ค่านี้ต้อง "น้อยกว่า" Order in Layer ของกระดาษ

    [Header("Blocker")]
    public GameObject blocker;        // GameObject โปร่งใสอยู่หลังสุด กดแล้วปิดเอกสาร

    private int currentPageIndex = 0;

    void Start()
    {
        if (documentRoot != null)
            documentRoot.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);
    }

    // =========================
    // OPEN
    // =========================
    public void OpenDocument()
    {
        if (documentRoot != null)
            documentRoot.SetActive(true); // บังคับเปิดเสมอ ต่อให้ก่อนหน้านี้ถูกปิดไว้ใน Hierarchy

        if (blocker != null)
            blocker.SetActive(true);

        OpenPage(0); // เปิดมาให้เห็นหน้าแรกก่อน
    }

    // =========================
    // เปลี่ยนหน้า (เรียกจากปุ่มลิสต์)
    // =========================
    public void OpenPage(int index)
    {
        if (pages == null || pages.Length == 0)
            return;

        if (index < 0 || index >= pages.Length)
        {
            Debug.LogWarning("Document2Manager: page index ไม่ถูกต้อง -> " + index);
            return;
        }

        // สลับโชว์/ซ่อนเนื้อหาแต่ละหน้า
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == index);
        }

        // PostIt ของหน้าที่เปิดอยู่ = ลอยหน้ากระดาษ, ที่เหลือ = ลอดไปหลังกระดาษ
        if (postIts != null)
        {
            for (int i = 0; i < postIts.Length; i++)
            {
                if (postIts[i] == null)
                    continue;

                // กันเหนียว: PostIt ต้องโชว์ตลอด ต่อให้ก่อนหน้านี้เผลอปิดไว้ใน Hierarchy
                if (!postIts[i].activeSelf)
                    postIts[i].SetActive(true);

                SpriteRenderer sr = postIts[i].GetComponentInChildren<SpriteRenderer>();

                if (sr != null)
                {
                    sr.sortingOrder = (i == index) ? frontOrder : backOrder;
                }
                else
                {
                    Debug.LogWarning("Document2Manager: postIts[" + i + "] (" + postIts[i].name + ") ไม่มี SpriteRenderer เลยแม้แต่ในลูก");
                }
            }
        }

        currentPageIndex = index;
    }

    // =========================
    // CLOSE (เรียกจาก Blocker)
    // =========================
    public void CloseDocument()
    {
        if (documentRoot != null)
            documentRoot.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);
    }
}