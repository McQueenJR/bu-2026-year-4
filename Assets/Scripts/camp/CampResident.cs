using UnityEngine;

[System.Serializable]
public class CampResident
{
    [Header("NPC")]
    public NPCData npcData;

    [Header("Room Document")]
    public bool hasDocument = true;
}