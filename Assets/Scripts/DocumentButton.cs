using UnityEngine;

public class DocumentButton : MonoBehaviour
{
    // เก็บไว้เพื่อไม่ให้ค่าที่ผูกไว้ใน Inspector หาย (ตอนนี้ไม่ได้ใช้ในโค้ด)
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

        // ★ ถ้า NPC ตัวนี้ไม่มีเอกสารติดตัว (ทั้งระบบใหม่และเก่า) ไม่ต้องทำอะไร
        if (!npc.data.HasTempleDocument)
        {
            Debug.Log($"NPC '{npc.data.npcName}' ไม่มีเอกสารติดตัว");
            return;
        }

        if (DocumentPopupManager.Instance == null) return;

        // ★ ตัดการ Instantiate ซ้ำใต้ Canvas ของเดิมออกแล้ว — ให้ DocumentPopupManager สร้างที่เดียว
        bool opened;

        if (npc.applicant != null && npc.applicant.templeDocument != null)
        {
            // ระบบใหม่: Template + ข้อมูลที่สุ่มไว้แล้ว
            opened = DocumentPopupManager.Instance.Open(npc.applicant);
        }
        else
        {
            // ระบบเดิม (legacy)
            opened = DocumentPopupManager.Instance.Open(npc.data.applicantPhotoPrefab);
        }

        // เล่นเสียงเฉพาะตอนเปิดสำเร็จ
        if (opened && NPCSoundManager.Instance != null)
            NPCSoundManager.Instance.PlayDocumentOpen();
    }

    public void CloseDocument()
    {
        if (DocumentPopupManager.Instance != null)
            DocumentPopupManager.Instance.Close();

        if (NPCSoundManager.Instance != null)
            NPCSoundManager.Instance.PlayDocumentClose();
    }
}