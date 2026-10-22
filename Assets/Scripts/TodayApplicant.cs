using UnityEngine;

[System.Serializable]
public class TodayApplicant
{
    [Header("ข้อมูล NPC ของวันนี้")]
    public DataPrefabNPC npcData;
    public NPCData displayData;
    public NPCSpawnGroup selectedGroup;
    
    [Header("Spawn วันนี้")]
    public GameObject spawnPrefab;
    public bool isGood;
    public bool isInTodayList;
    public bool isGhost;

    [Header("Ghost")]
    public NPCData ghostTarget;

    [Header("Temple Document")]
    public TempleDocumentRuntime templeDocument;
    
    [Tooltip("NPC คนนี้มีเอกสารขอเข้าหรือไม่")]
    public bool hasTempleDocument = true;

    [HideInInspector]
    public bool hasEnteredToday = false;

    [HideInInspector]
    public bool forgotDocumentDecided = false;   // สุ่มตัดสินแล้วหรือยัง

    [HideInInspector]
    public bool forgotDocument = false;          // มีเอกสารจริง แต่ลืมยื่น

    [HideInInspector]
    public bool documentPlacedOnDesk = false;    // เอกสารถูกวางบนโต๊ะแล้ว // ลืมใบคำขอไว้ข้างใน
    
    [HideInInspector]
    public string forgotDocumentInHouseMessage = "";   // ข้อความ "ลืมเอกสารไว้ในบ้าน" ที่สุ่มแล้ว (ว่าง = ยังไม่สุ่ม)
}