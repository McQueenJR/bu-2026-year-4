using UnityEngine;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject bagPanel;
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

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

        bagPanel.SetActive(true);

        ShowItems(npc.data);
    }

    void ShowItems(NPCData data)
    {
        ClearItems();

        foreach (ItemData item in data.bagItems)
        {
            if (item == null)
                continue;

            GameObject slot =
                Instantiate(
                    itemSlotPrefab,
                    itemContainer
                );

            Image image =
                slot.GetComponent<Image>();

            if (image != null)
            {
                image.sprite = item.itemImage;
            }
        }
    }

    void ClearItems()
    {
        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }
    }

    public void CloseBag()
    {
        ClearItems();

        bagPanel.SetActive(false);
    }
}