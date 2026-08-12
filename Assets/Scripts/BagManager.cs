using UnityEngine;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject bagPanel;
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

    private NPCData currentData;

    void Start()
    {
        bagPanel.SetActive(false);
    }

    public void OpenBag()
    {
        if (GameManager.Instance.currentNPC == null)
        {
            Debug.Log("ไม่มี NPC");
            return;
        }

        NPC npc =
            GameManager.Instance.currentNPC.GetComponent<NPC>();

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

        currentData = npc.data;

        bagPanel.SetActive(true);

        // ถ้ายังไม่มีของใน UI ให้สร้าง
        if (itemContainer.childCount == 0)
        {
            ShowItems(currentData);
        }
    }

    void ShowItems(NPCData data)
    {
        foreach (GameObject item in data.bagItems)
        {
            if (item == null)
                continue;

            GameObject slot =
                Instantiate(
                    itemSlotPrefab,
                    itemContainer
                );

            Image image =
                slot.GetComponentInChildren<Image>();

            if (image == null)
                continue;

            SpriteRenderer spriteRenderer =
                item.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                image.sprite = spriteRenderer.sprite;
                image.preserveAspect = true;
            }
        }
    }

    public void CloseBag()
    {
        // แค่ปิด UI
        // ไม่ลบ Item Slot
        bagPanel.SetActive(false);
    }

    public void ClearBag()
    {
        // ใช้เมื่อต้องการล้างจริง ๆ
        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }

        currentData = null;
    }
}