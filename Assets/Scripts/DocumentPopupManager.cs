using UnityEngine;

public class DocumentPopupManager : MonoBehaviour
{
    public static DocumentPopupManager Instance;

    public GameObject popup;
    public Transform holder;

    [Header("กันเอกสารลากออกนอกจอ")]
    public BoxCollider2D documentDragBoundary;
    
    GameObject currentDocument;

    void Awake()
    {
        Instance = this;
        popup.SetActive(false);
    }

    public void Open(GameObject documentPrefab)
    {
        
        // กันกดระหว่างถือแว่นขยาย / ไม้ขีดไฟ
        if (MagnifyingGlass.Instance != null && MagnifyingGlass.Instance.IsHolding)
            return;
        
        if (Matchbox.Instance != null && Matchbox.Instance.IsHolding)
            return;
        
        // กันกดระหว่างมี dialog เปิดอยู่ / กำลังเรียกตำรวจ / NPC ยังไม่ถึงจุดตรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
            {
                Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ กดไม่ได้ตอนนี้");
                return;
            }
        }
        
        popup.SetActive(true);

        if (currentDocument != null)
            Destroy(currentDocument);

        currentDocument = Instantiate(documentPrefab, holder);
        
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