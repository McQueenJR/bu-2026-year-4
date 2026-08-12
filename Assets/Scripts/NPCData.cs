using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "Game/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Character")]
    public string npcName;
    public int age;

    [Header("Dialog")]
    [TextArea(2, 5)]
    public string[] dialogs;

    [Header("Bag")]
    public GameObject[] bagItems;
    
    [Header("ID Card")]
    public GameObject idCardPrefab;
    public GameObject idCardDisplayPrefab;
}