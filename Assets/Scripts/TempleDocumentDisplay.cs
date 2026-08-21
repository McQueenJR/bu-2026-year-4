using UnityEngine;

public class TempleDocumentDisplay : MonoBehaviour
{
    public void ShowDocument()
    {
        if (GameManager.Instance.currentNPC == null)
        {
            Debug.Log("ไม่มี currentNPC ตอนนี้");
            return;
        }

        NPC npc = GameManager.Instance.currentNPC.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.Log("NPC ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.Log("NPC ไม่มี NPCData");
            return;
        }

        if (npc.data.templeDocumentPrefab == null)
        {
            Debug.Log("NPC " + npc.data.npcName + " ไม่มีเอกสารเข้าวัด");
            return;
        }

        TempleDocumentPopup.Instance.Show(npc.data.templeDocumentPrefab);
    }
}