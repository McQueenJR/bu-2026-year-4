using UnityEngine;

public class DocumentButton : MonoBehaviour
{
    private GameObject currentDocument;
    public GameObject documentPopup;

    public void OpenDocument()
    {
        // กันกดระหว่างถือแว่นขยาย / ไม้ขีดไฟ
        if (MagnifyingGlass.Instance != null && MagnifyingGlass.Instance.IsHolding)
            return;

        if (Matchbox.Instance != null && Matchbox.Instance.IsHolding)
            return;

        if (GameManager.Instance.currentNPC == null) return;

        NPC npc = GameManager.Instance.currentNPC.GetComponent<NPC>();
        if (npc == null || npc.data == null) return;

        // ลบใบเก่าถ้ามี
        if (currentDocument != null)
            Destroy(currentDocument);

        // สร้างเอกสารจาก Prefab
        currentDocument = Instantiate(
            npc.data.applicantPhotoPrefab,
            GameObject.Find("Canvas").transform
        );
        
        DocumentPopupManager.Instance.Open(npc.data.applicantPhotoPrefab);
    }

    public void CloseDocument()
    {
        if (currentDocument != null)
        {
            Destroy(currentDocument);
            currentDocument = null;
        }
    }
}