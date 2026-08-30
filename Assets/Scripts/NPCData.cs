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
    
    [Header("Applicant Photo")]
    public GameObject applicantPhotoPrefab;
    
    [Header("Temple Entry Document")]
    public GameObject templeDocumentPrefab;
    
    [Header("Today List")]
    public GameObject TodayPhotoPrefab;
    
   
    
    [Header("Green Button Dialog")]
    [TextArea(2, 5)]
    public string[] greenDialogs;

    [Header("Emergency Dialog")]
    [TextArea(2, 5)]
    public string[] emergencyDialogs;
    [Header("Checklist")]
    public string[] checkQuestions = new string[4];

    [Header("Correct Answer")]
    public bool[] correctAnswers = new bool[4];
}