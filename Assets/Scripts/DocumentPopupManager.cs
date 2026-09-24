using UnityEngine;

public class DocumentPopupManager : MonoBehaviour
{
    public static DocumentPopupManager Instance;

    public GameObject popup;
    public Transform holder;

    [Header("Template เอกสารขอเข้าวัด (ระบบใหม่ — prefab เดียวใช้กับ NPC ทุกตัว)")]
    public GameObject templeDocumentTemplatePrefab;

    [Header("กันเอกสารลากออกนอกจอ")]
    public BoxCollider2D documentDragBoundary;

    GameObject currentDocument;

    void Awake()
    {
        Instance = this;
        popup.SetActive(false);
    }

    // =========================================================
    // ระบบใหม่: เปิดเอกสารจากข้อมูลที่สุ่มไว้แล้วของ applicant
    // คืน true = เปิดสำเร็จ
    // =========================================================
    public bool Open(TodayApplicant applicant)
    {
        if (applicant == null || applicant.templeDocument == null)
        {
            Debug.Log("ไม่มีข้อมูลเอกสารในระบบใหม่ (applicant.templeDocument เป็น null)");
            return false;
        }

        if (templeDocumentTemplatePrefab == null)
        {
            Debug.LogError("DocumentPopupManager ยังไม่ได้ใส่ Temple Document Template Prefab");
            return false;
        }

        if (!CanOpen())
            return false;

        ShowDocument(templeDocumentTemplatePrefab, applicant.templeDocument);
        return true;
    }

    // =========================================================
    // ระบบเดิม (legacy): เปิดจาก prefab เอกสารรูปเดียว
    // ยังใช้กับ NPC ที่ยังไม่ได้ย้ายมาระบบใหม่
    // =========================================================
    public bool Open(GameObject documentPrefab)
    {
        // ถ้า NPC ตัวนี้ไม่มีเอกสารติดตัว ไม่ต้องเปิด popup เลย
        if (documentPrefab == null)
        {
            Debug.Log("NPC ตัวนี้ไม่มีเอกสารติดตัว (documentPrefab เป็น None)");
            return false;
        }

        if (!CanOpen())
            return false;

        ShowDocument(documentPrefab, null);
        return true;
    }

    // เงื่อนไขที่ห้ามเปิดเอกสาร (ใช้ร่วมกันทั้งสองระบบ)
    private bool CanOpen()
    {
        // กันกดระหว่างถือแว่นขยาย / ไม้ขีดไฟ
        if (MagnifyingGlass.Instance != null && MagnifyingGlass.Instance.IsHolding)
            return false;

        if (Matchbox.Instance != null && Matchbox.Instance.IsHolding)
            return false;

        // กันกดระหว่างมี dialog เปิดอยู่ / กำลังเรียกตำรวจ / NPC ยังไม่ถึงจุดตรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้");
                return false;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้");
                return false;
            }

            if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
            {
                Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ กดไม่ได้ตอนนี้");
                return false;
            }
        }

        return true;
    }

    private void ShowDocument(GameObject prefab, TempleDocumentRuntime data)
    {
        popup.SetActive(true);

        if (currentDocument != null)
            Destroy(currentDocument);

        currentDocument = Instantiate(prefab, holder);

        // ระบบใหม่: ใส่ข้อมูลที่สุ่มไว้แล้วลง Template
        if (data != null)
        {
            TempleDocumentView view = currentDocument.GetComponent<TempleDocumentView>();

            if (view != null)
                view.Bind(data);
            else
                Debug.LogError("Template prefab ไม่มี TempleDocumentView", currentDocument);
        }

        DocumentDisplayClick dragScript = currentDocument.GetComponent<DocumentDisplayClick>();
        if (dragScript != null && documentDragBoundary != null)
        {
            dragScript.SetDragBoundary(documentDragBoundary);
        }
    }

    public void Close()
    {
        if (currentDocument != null)
            Destroy(currentDocument);

        popup.SetActive(false);
    }
}