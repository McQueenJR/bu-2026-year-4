using UnityEngine;

public enum NPCType
{
    Villager,
    Robber,
    Monk
}

public class NPC : MonoBehaviour
{
    public NPCType npcType;
    public NPCData data;
}