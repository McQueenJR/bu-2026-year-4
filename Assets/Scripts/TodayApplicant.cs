using UnityEngine;

[System.Serializable]
public class TodayApplicant
{
    [Header("ข้อมูล NPC ของวันนี้")]
    public DataPrefabNPC npcData;      // ScriptableObject ของ NPC
    public NPCData displayData;        // NPCData ที่ใช้แสดง Dialog / Today List

    [Header("Spawn วันนี้")]
    public GameObject spawnPrefab;     // Prefab ที่จะ Spawn วันนี้
    public bool isGood;                // Good / Bad เวอร์ชันของ NPC
    public bool isInTodayList;         // อยู่ใน Today List หรือไม่

    
    [Header("Temple Document")]
    public TempleDocumentRuntime templeDocument;
    [HideInInspector]
    public bool hasEnteredToday = false;
}