using UnityEngine;

public class IDCardDisplay : MonoBehaviour
{
    public void ShowCard()
    {
        if (GameManager.Instance.currentNPC == null)
        {
            Debug.LogError("ไม่มี currentNPC ตอนนี้");
            return;
        }

        NPC npc = GameManager.Instance.currentNPC.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError("NPC ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError("NPC ไม่มี NPCData");
            return;
        }

        if (npc.data.idCardDisplayPrefab == null)
        {
            Debug.LogError("NPC " + npc.data.npcName + " ไม่ได้ใส่ idCardDisplayPrefab");
            return;
        }

        IDCardPopup.Instance.Show(npc.data.idCardDisplayPrefab);
    }
}