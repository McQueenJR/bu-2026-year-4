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
    
    [Header("Dialogue Set (ระบบใหม่ — ใช้แชร์บทพูดข้าม NPC ได้)")]
    public NPCDialogueSet dialogueSet;
    
    // ถ้ามี dialogueSet ใช้ชุดนั้น ถ้าไม่มีใช้ array เดิม (ของเก่า NPC ที่ยังไม่ย้าย)
    public string[] Dialogs => dialogueSet != null ? dialogueSet.dialogs : dialogs;
    
    public string[] GreenDialogs => dialogueSet != null ? dialogueSet.greenDialogs : greenDialogs;
    public string[] RedDialogs => dialogueSet != null ? dialogueSet.redDialogs : redDialogs;
    public string[] EmergencyDialogs => dialogueSet != null ? dialogueSet.emergencyDialogs : emergencyDialogs;
    
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
    
    [Header("Temple Document (ระบบใหม่)")]
    public TempleDocumentData templeDocumentData;
    
    // มีเอกสารติดตัวไหม (ระบบใหม่ หรือ prefab เดิม)
    public bool HasTempleDocument => templeDocumentData != null || applicantPhotoPrefab != null;
    
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
    
    
    [Header("Phone Dialog")]

    [TextArea(2, 5)]
    public string[] phoneGreetingDialogs;

    [TextArea(2, 5)]
    public string[] phonePeopleCountDialogs;

    [TextArea(2, 5)]
    public string[] phoneDocumentDialogs;
    
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