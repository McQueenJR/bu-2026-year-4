using UnityEngine;

public class BagInteract : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (GameManager.Instance == null)
            return;

        GameObject npcObject =
            GameManager.Instance.currentNPC;

        if (npcObject == null)
        {
            Debug.Log("ไม่มี NPC ปัจจุบัน");
            return;
        }

        NPC npc =
            npcObject.GetComponent<NPC>();

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

        BagInventoryUI inventory =
            FindFirstObjectByType<BagInventoryUI>();

        if (inventory == null)
        {
            Debug.LogError("ไม่พบ BagInventoryUI");
            return;
        }

        // ส่ง Data ของ NPC คนนี้เข้า Inventory
        inventory.OpenInventory(npc.data);
    }
}