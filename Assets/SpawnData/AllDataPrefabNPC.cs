using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCGroupChance
{
    public DataPrefabNPC npc;

    [Range(0,100)]
    public int chance = 50;
}

[System.Serializable]
public class RoleGroup
{
    [Range(0,100)]
    public int spawnChance = 40;

    public List<NPCGroupChance> npcs = new();
}

[CreateAssetMenu(fileName = "AllDataPrefabNPC", menuName = "Temple NPC/All Data Prefab NPC")]
public class AllDataPrefabNPC : ScriptableObject
{
    [Header("Daily NPC")]
    public int minNPCPerDay = 9;
    public int maxNPCPerDay = 11;

    [Header("Today List")]
    public int minTodayList = 3;
    public int maxTodayList = 6;

    [Header("Villager")]
    public RoleGroup villager;

    [Header("Monk")]
    public RoleGroup monk;

    [Header("Special (Robber / VIP / Dropper)")]
    public RoleGroup special;
}