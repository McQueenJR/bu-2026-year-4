using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CampResident
{
    [Header("NPC")]
    public NPCData npcData;

    [Tooltip("NPCData ")]
    public List<NPCData> additionalNpcData = new List<NPCData>();

    public bool ContainsNPCData(NPCData data)
    {
        if (MatchesNPCData(npcData, data))
            return true;

        if (additionalNpcData == null)
            return false;

        foreach (NPCData variant in additionalNpcData)
        {
            if (MatchesNPCData(variant, data))
                return true;
        }

        return false;
    }

    private bool MatchesNPCData(NPCData a, NPCData b)
    {
        return a != null && b != null &&
               (a == b || a.npcName == b.npcName);
    }

    [Header("Room Document")]
    public bool hasDocument = true;
    public NPCData documentHolder;
    public bool isAlive;
    public bool isHomeToday;
}