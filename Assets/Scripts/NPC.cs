using UnityEngine;

public enum NPCType
{
    Villager,
    Robber
}

public class NPC : MonoBehaviour
{
    public NPCType npcType;
    public NPCData data;
}

