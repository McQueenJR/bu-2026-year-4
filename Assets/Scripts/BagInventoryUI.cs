using UnityEngine;

public class BagInventoryUI : MonoBehaviour
{
    public static BagInventoryUI Instance;

    [Header("UI")]
    public GameObject inventoryPanel;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private GameObject[] spawnedItems;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
    }

    // =========================
    // OPEN INVENTORY
    // =========================

    public void OpenInventory(NPCData npcData)
    {
        if (npcData == null)
        {
            Debug.LogError("ไม่มี NPCData");
            return;
        }

        inventoryPanel.SetActive(true);

        // ล้างของเก่าก่อน
        ClearInventory();

        if (npcData.bagItems == null ||
            npcData.bagItems.Length == 0)
        {
            Debug.Log("NPC ตัวนี้ไม่มีของในกระเป๋า");
            return;
        }

        int count = Mathf.Min(
            npcData.bagItems.Length,
            spawnPoints.Length
        );

        spawnedItems = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            GameObject itemPrefab = npcData.bagItems[i];

            if (itemPrefab == null)
                continue;

            Transform spawnPoint = spawnPoints[i];

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "Spawn Point ช่อง " + i + " ยังไม่ได้ใส่"
                );

                continue;
            }

            // Spawn ของ
            GameObject item = Instantiate(
                itemPrefab,
                spawnPoint.position,
                itemPrefab.transform.rotation,
                spawnPoint.parent
            );

            // ใช้ Scale จาก Prefab
            item.transform.localScale =
                itemPrefab.transform.localScale;

            spawnedItems[i] = item;

            Debug.Log(
                "Spawn ของ: " +
                itemPrefab.name +
                " → ช่อง " + i
            );
        }
    }

    // =========================
    // CLOSE
    // =========================

    public void CloseInventory()
    {
        ClearInventory();

        inventoryPanel.SetActive(false);
    }

    // =========================
    // CLEAR
    // =========================

    private void ClearInventory()
    {
        if (spawnedItems == null)
            return;

        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i] != null)
            {
                Destroy(spawnedItems[i]);
            }
        }

        spawnedItems = null;
    }
}