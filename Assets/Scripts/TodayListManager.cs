using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TodayListManager : MonoBehaviour
{
    public static TodayListManager Instance;

    public GameObject popup;
    public Transform holder;
    public Transform[] slots;

    // ===== เพิ่มบรรทัดนี้ =====
    public GameObject namePrefab;          // ลาก prefab "Text (TMP)" มาใส่ใน Inspector
    public Vector3 nameOffset = new Vector3(0, -1.5f, 0); // ตำแหน่งใต้รูป ปรับเลขตามจริง

    private List<GameObject> spawnedPhotos = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        popup.SetActive(false);
    }

    public void OpenTodayList(List<NPCData> todayNPCs)
    {
        popup.SetActive(true);

        if (spawnedPhotos.Count > 0)
            return;

        for (int i = 0; i < todayNPCs.Count && i < slots.Length; i++)
        {
            // สร้างรูป (เหมือนเดิม)
            GameObject photo = Instantiate(todayNPCs[i].TodayPhotoPrefab, slots[i]);
            photo.transform.localPosition = Vector3.zero;
            photo.transform.localScale = Vector3.one;
            spawnedPhotos.Add(photo);

            // ===== เพิ่มส่วนนี้: สร้าง text แยก แต่ parent เข้ากับ slot เดียวกัน =====
            if (namePrefab != null)
            {
                GameObject nameObj = Instantiate(namePrefab, slots[i]);
                nameObj.transform.localPosition = nameOffset;
                nameObj.transform.localScale = Vector3.one;

                TextMeshPro tmp = nameObj.GetComponent<TextMeshPro>();
                if (tmp != null)
                    tmp.text = todayNPCs[i].npcName;

                // ===== เพิ่มบรรทัดนี้: ทำให้ text อยู่ชั้นเดียวกับรูป ไม่จมหลังกระดาษ =====
                Renderer photoRenderer = photo.GetComponentInChildren<Renderer>();
                Renderer textRenderer = nameObj.GetComponent<Renderer>();
                if (photoRenderer != null && textRenderer != null)
                {
                    textRenderer.sortingLayerID = photoRenderer.sortingLayerID; // เอา Sorting Layer เดียวกับรูป (Display)
                    textRenderer.sortingOrder = photoRenderer.sortingOrder + 1;  // Order สูงกว่ารูปนิดหน่อย ให้อยู่บนสุด
                }

                spawnedPhotos.Add(nameObj);
            }
        }

        TodayListDisplayClick display = popup.GetComponentInChildren<TodayListDisplayClick>();
        if (display != null)
            display.RefreshSpriteCache();
    }

    public void CloseTodayList()
    {
        popup.SetActive(false);
    }

    public void ResetForNewDay()
    {
        ClearTodayList();
    }

    private void ClearTodayList()
    {
        foreach (GameObject photo in spawnedPhotos)
            Destroy(photo);

        spawnedPhotos.Clear();
    }
}