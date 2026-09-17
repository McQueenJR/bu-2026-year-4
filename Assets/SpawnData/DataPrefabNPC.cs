using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCPrefabChance
{
    [Tooltip("Prefab ที่จะ Spawn")]
    public GameObject prefab;

    [Range(0, 100)]
    [Tooltip("โอกาสที่ Prefab นี้จะถูกสุ่มในกลุ่มนี้")]
    public int chance = 50;
}


[System.Serializable]
public class NPCSpawnGroup
{
    [Header("โอกาสเลือกกลุ่มนี้")]
    [Range(0, 100)]
    public int chance = 50;

    [Header("Prefab ในกลุ่มนี้")]
    public List<NPCPrefabChance> prefabs = new List<NPCPrefabChance>();
}

[CreateAssetMenu(fileName = "DataPrefabNPC", menuName = "Temple NPC/Data Prefab NPC")]
public class DataPrefabNPC : ScriptableObject
{
    [Header("อยู่ใน Today List")]
    public NPCSpawnGroup todayGood;
    public NPCSpawnGroup todayBad;

    [Header("ไม่อยู่ใน Today List")]
    public NPCSpawnGroup normalGood;
    public NPCSpawnGroup normalBad;
    
    [Header("Role ของ NPC นี้")]
    public NPCType roleType;
}