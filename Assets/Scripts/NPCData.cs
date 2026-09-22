using UnityEngine;

[System.Serializable]
public class ChecklistQuestionVariants
{
    [TextArea(2, 5)]
    public string[] messages;
}

[CreateAssetMenu(fileName = "New NPC Data", menuName = "Game/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Character")]
    public string npcName;
    public int age;
    public NpcVoiceType voiceType = NpcVoiceType.None;

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
    
    [HideInInspector] public TodayApplicant applicant;
    
    [Header("Green Button Dialog")]
    [TextArea(2, 5)]
    public string[] greenDialogs;
    
    [Header("Red Button Dialog")]        
    [TextArea(2, 5)]
    public string[] redDialogs;   

    [Header("Emergency Dialog")]
    [TextArea(2, 5)]
    public string[] emergencyDialogs;
    
    [Header("Checklist")]
    public ChecklistQuestionVariants[] checkQuestions = new ChecklistQuestionVariants[5];
    // เก็บข้อความที่ "สุ่มเลือกแล้ว" ของแต่ละหัวข้อ ตั้งแต่ตอนสปาวน์
    [HideInInspector] public string[] selectedCheckQuestions;
    
    [Header("Correct Answer")]
    public bool[] correctAnswers = new bool[5];
    
    [Header("Camp / Tent")]
    public int campID = 0;
    
    // เรียกครั้งเดียวตอน NPC สปาวน์ เพื่อสุ่มเลือกข้อความแต่ละหัวข้อไว้ล่วงหน้า
    public void InitializeChecklistQuestions()
    {
        if (checkQuestions == null) return;

        selectedCheckQuestions = new string[checkQuestions.Length];

        for (int i = 0; i < checkQuestions.Length; i++)
        {
            var variants = checkQuestions[i]?.messages;

            selectedCheckQuestions[i] = (variants != null && variants.Length > 0)
                ? variants[Random.Range(0, variants.Length)]
                : string.Empty;
        }
    }
}