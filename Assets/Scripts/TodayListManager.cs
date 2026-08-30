using UnityEngine;
using System.Collections.Generic;

public class TodayListManager : MonoBehaviour
{
    public static TodayListManager Instance;

    public GameObject popup;      // Panel Today List
    public Transform holder;      // Holder ของรูป
    public Transform[] slots;     // SpawnPointList ทั้งหมด

    private List<GameObject> spawnedPhotos = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        popup.SetActive(false);
    }

    public void OpenTodayList(List<NPCData> todayNPCs)
    {
        popup.SetActive(true);

        ClearTodayList();

        for (int i = 0; i < todayNPCs.Count && i < slots.Length; i++)
        {
            GameObject photo = Instantiate(
                todayNPCs[i].TodayPhotoPrefab,
                slots[i]
            );

            photo.transform.localPosition = Vector3.zero;
            photo.transform.localScale = Vector3.one;

            spawnedPhotos.Add(photo);
        }
    }

    public void CloseTodayList()
    {
        popup.SetActive(false);
        ClearTodayList();
    }

    private void ClearTodayList()
    {
        foreach (GameObject photo in spawnedPhotos)
            Destroy(photo);

        spawnedPhotos.Clear();
    }
}