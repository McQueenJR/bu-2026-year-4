using UnityEngine;
using UnityEngine.UI;

public class BagInventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;

    public Transform itemGrid;

    public GameObject itemSlotPrefab;

    private void Start()
    {
        inventoryPanel.SetActive(false);
    }

    public void OpenInventory(NPCData npcData)
    {
        if (npcData == null)
            return;

        inventoryPanel.SetActive(true);

        ClearInventory();

        foreach (GameObject item in npcData.bagItems)
        {
            if (item == null)
                continue;

            GameObject slot =
                Instantiate(
                    itemSlotPrefab,
                    itemGrid
                );

            // หา Image ใน Slot
            Image image =
                slot.GetComponentInChildren<Image>();

            if (image != null)
            {
                // ดึง Sprite จาก GameObject ของ Item
                SpriteRenderer spriteRenderer =
                    item.GetComponent<SpriteRenderer>();

                if (spriteRenderer != null)
                {
                    image.sprite = spriteRenderer.sprite;
                }
            }
        }
    }

    public void CloseInventory()
    {
        ClearInventory();

        inventoryPanel.SetActive(false);
    }

    private void ClearInventory()
    {
        for (int i = itemGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(itemGrid.GetChild(i).gameObject);
        }
    }
}