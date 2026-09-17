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

    [Header("Camp System")]
    public int campID = -1;            // หมายเลขเต็นท์ (0,1,2...)
    public bool isAlive = true;        // ยังอยู่ในเต็นท์หรือโดนฆ่าแล้ว
    public bool hasEnteredCamp = false;// เข้าเต็นท์แล้วหรือยัง
}