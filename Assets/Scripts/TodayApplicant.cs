using UnityEngine;

[System.Serializable]
public class TodayApplicant
{
    [Header("ข้อมูล NPC ของวันนี้")]
    public DataPrefabNPC npcData;
    public NPCData displayData;

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
}