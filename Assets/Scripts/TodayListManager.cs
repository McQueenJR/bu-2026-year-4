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
    
    [Header("SOUND")]
    public AudioSource openSound;
    public AudioSource closeSound;

    private List<GameObject> spawnedPhotos = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        popup.SetActive(false);
    }

    // เรียกจากระบบตอนเริ่มวัน — สร้างรูป/ชื่อ ไม่มีเงื่อนไขจำกัด
    public void GenerateTodayList(List<NPCData> todayNPCs)
    {
        if (spawnedPhotos.Count > 0)
            return;

        for (int i = 0; i < todayNPCs.Count && i < slots.Length; i++)
        {
            GameObject photo = Instantiate(todayNPCs[i].TodayPhotoPrefab, slots[i]);
            photo.transform.localPosition = Vector3.zero;
            photo.transform.localScale = Vector3.one;
            spawnedPhotos.Add(photo);

            if (namePrefab != null)
            {
                GameObject nameObj = Instantiate(namePrefab, slots[i]);
                nameObj.transform.localPosition = nameOffset;
                nameObj.transform.localScale = Vector3.one;

                TextMeshPro tmp = nameObj.GetComponent<TextMeshPro>();
                if (tmp != null)
                    tmp.text = todayNPCs[i].npcName;

                Renderer photoRenderer = photo.GetComponentInChildren<Renderer>();
                Renderer textRenderer = nameObj.GetComponent<Renderer>();
                if (photoRenderer != null && textRenderer != null)
                {
                    textRenderer.sortingLayerID = photoRenderer.sortingLayerID;
                    textRenderer.sortingOrder = photoRenderer.sortingOrder + 1;
                }

                spawnedPhotos.Add(nameObj);
            }
        }

        TodayListDisplayClick display = popup.GetComponentInChildren<TodayListDisplayClick>();
        if (display != null)
            display.RefreshSpriteCache();
    }

    // เรียกจากตอนผู้เล่นคลิกไอคอน — แค่เปิด popup มี 3 เงื่อนไขป้องกัน
    public void OpenTodayList()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
            {
                Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ กดไม่ได้ตอนนี้");
                return;
            }
        }

        popup.SetActive(true);
        if (openSound != null)
            openSound.Play();
    }
    public void CloseTodayList()
    {
        if (closeSound != null)
            closeSound.Play();
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