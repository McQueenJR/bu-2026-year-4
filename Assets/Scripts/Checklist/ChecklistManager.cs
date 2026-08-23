using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;

    [Header("Checklist Panel ")]
    public GameObject checklistPanel;
 
    [Header("Blocker")]
    public GameObject blocker;
    
    [Header("Page")]
    public GameObject page1;
    public GameObject page2;
    
    [Header("Post-It SwapLayer1-2")]
    public Button postItGoToPage2;
    public Button postItGoToPage1;

    [Header("Question Toggles")]
    public Toggle toggleBag;
    public Toggle toggleAppearance;
    public Toggle toggleID;
    public Toggle toggleEntryDoc;
    
    [Header("Answer Toggles - Page 2")]
    public Toggle bagAbnormal;
    public Toggle bagNormal;

    public Toggle appearanceAbnormal;
    public Toggle appearanceNormal;

    public Toggle idAbnormal;
    public Toggle idNormal;

    public Toggle entryDocAbnormal;
    public Toggle entryDocNormal;

    [Header("Dialog")]
    public DialogManager dialogManager;
    
    [Header("Score")]
    public int checklistScore = 0; // ใช้แสดงผลคะแนนรอบล่าสุดที่ตอบ (ไม่ใช่ตัวสะสมจริงแล้ว)
    
    [Header("Checklist Sounds")]
    public AudioSource checklistAudioSource;
    public AudioClip openChecklistSound;
    public AudioClip changePageSound;
    public AudioClip submitSound;

    
    private bool[] playerAnswers = new bool[4];

    // เก็บว่าผู้เล่นตอบข้อไหนแล้ว
    private bool[] answered = new bool[4];

// เก็บคะแนน checklist ล่าสุดที่ "กดส่ง" ของ NPC แต่ละคน
    private Dictionary<GameObject, int> npcChecklistScores = new Dictionary<GameObject, int>();

    // NPC ที่กำลังตรวจ
    private NPC currentNPC;

    // เก็บว่าผู้เล่นเลือกถามข้อไหน
    private bool[] selectedQuestions = new bool[4];

    // ลำดับข้อที่จะถาม
    private int[] questionOrder = new int[4];

    // จำนวนข้อที่เลือก
    private int selectedCount = 0;

    // ตำแหน่งปัจจุบันที่กำลังถาม
    private int currentQuestionIndex = 0;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false);
        
        if (blocker != null)           
            blocker.SetActive(false); 

        // -------------------------
        // Toggle
        // -------------------------

        toggleBag.onValueChanged.AddListener(
            value => SelectQuestion(0, value)
        );

        toggleAppearance.onValueChanged.AddListener(
            value => SelectQuestion(1, value)
        );

        toggleID.onValueChanged.AddListener(
            value => SelectQuestion(2, value)
        );

        toggleEntryDoc.onValueChanged.AddListener(
            value => SelectQuestion(3, value)
        );
        
        
        // -------------------------
// Toggle หน้า 2 (คำตอบ)
// -------------------------

        bagAbnormal.onValueChanged.AddListener(
            value => SetAnswer(0, true, value)
        );
        bagNormal.onValueChanged.AddListener(
            value => SetAnswer(0, false, value)
        );

        appearanceAbnormal.onValueChanged.AddListener(
            value => SetAnswer(1, true, value)
        );
        appearanceNormal.onValueChanged.AddListener(
            value => SetAnswer(1, false, value)
        );

        idAbnormal.onValueChanged.AddListener(
            value => SetAnswer(2, true, value)
        );
        idNormal.onValueChanged.AddListener(
            value => SetAnswer(2, false, value)
        );

        entryDocAbnormal.onValueChanged.AddListener(
            value => SetAnswer(3, true, value)
        );
        entryDocNormal.onValueChanged.AddListener(
            value => SetAnswer(3, false, value)
        );
        if (postItGoToPage2 != null) postItGoToPage2.onClick.AddListener(ShowPage1);
        if (postItGoToPage1 != null) postItGoToPage1.onClick.AddListener(ShowPage2);
    }
    // =====================================================
// รับคำตอบจากผู้เล่น (หน้า 2)
// =====================================================

    private void SetAnswer(int index, bool answer, bool isOn)
    {
        if (!isOn)
            return;

        if (index < 0 || index >= 4)
            return;

        playerAnswers[index] = answer;
        answered[index] = true;

        Debug.Log(
            "ข้อ " + index +
            " ผู้เล่นตอบ = " +
            (answer ? "Abnormal" : "Normal")
        );
    }
    
    


    // =====================================================
    // OPEN
    // =====================================================
    public int GetChecklistScore(GameObject npc)
    {
        if (npc != null && npcChecklistScores.TryGetValue(npc, out int s))
            return s;
        return 0;
    }

    public void ConsumeChecklistScore(GameObject npc)
    {
        if (npc != null)
            npcChecklistScores.Remove(npc);
    }

    public void ResetAllChecklistScores()
    {
        npcChecklistScores.Clear();
    }
    
    public void OpenChecklist()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ไม่มี GameManager");
            return;
        }

        GameObject npcObject =
            GameManager.Instance.currentNPC;

        if (npcObject == null)
        {
            Debug.LogWarning(
                "GameManager ไม่มี currentNPC"
            );

            return;
        }

        currentNPC =
            npcObject.GetComponent<NPC>();

        if (currentNPC == null)
        {
            Debug.LogWarning(
                "NPC ไม่มี NPC.cs"
            );

            return;
        }

        if (currentNPC.data == null)
        {
            Debug.LogWarning(
                "NPC ไม่มี NPCData"
            );

            return;
        }

        Debug.Log(
            "เปิด Checklist NPC : " +
            currentNPC.data.npcName
        );
        checklistPanel.SetActive(true);
        
        if (blocker != null)       
            blocker.SetActive(true);     

        PlaySound(openChecklistSound); 

        ShowPage1();
        ResetChecklist();
    }


    // =====================================================
    // เลือกว่าจะถามข้อไหน
    // =====================================================

    private void SelectQuestion(
        int index,
        bool isOn)
    {
        if (index < 0 || index >= 4)
            return;

        selectedQuestions[index] = isOn;

        Debug.Log(
            "Question " +
            index +
            " = " +
            isOn
        );
    }


    // =====================================================
    // กดปุ่ม "ถาม"
    // =====================================================

    public void StartAskQuestions()
    {
        if (currentNPC == null)
        {
            Debug.LogWarning("ยังไม่มี NPC");
            return;
        }

        if (currentNPC.data == null)
            return;

        // สร้างลำดับใหม่
        selectedCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (selectedQuestions[i])
            {
                questionOrder[selectedCount] = i;
                selectedCount++;
            }
        }

        // ไม่มีข้อที่เลือก
        if (selectedCount == 0)
        {
            Debug.Log("ยังไม่ได้เลือกคำถาม");
            return;
        }

        currentQuestionIndex = 0;

        Debug.Log(
            "เริ่มถามทั้งหมด " +
            selectedCount +
            " ข้อ"
        );

        // =========================
        // ปิด Checklist Panel
        // =========================

        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        if (blocker != null)             
            blocker.SetActive(false);      
        
        // =========================
        // เริ่ม Dialog
        // =========================

        AskNextQuestion();
    }

    // =====================================================
    // ถามทีละข้อ
    // =====================================================

    private void AskNextQuestion()
    {
        if (currentQuestionIndex >= selectedCount)
        {
            Debug.Log("ถาม Checklist ครบแล้ว");
            return;
        }

        int questionIndex =
            questionOrder[currentQuestionIndex];

        // เช็กว่ามีช่องคำถามจริง
        if (currentNPC.data.checkQuestions == null ||
            questionIndex >= currentNPC.data.checkQuestions.Length)
        {
            currentQuestionIndex++;
            AskNextQuestion();
            return;
        }

        string question =
            currentNPC.data.checkQuestions[questionIndex];

        // ช่องนี้ไม่มีข้อความ
        if (string.IsNullOrWhiteSpace(question))
        {
            Debug.Log(
                "ข้อ " +
                questionIndex +
                " ไม่มีข้อความ"
            );

            currentQuestionIndex++;
            AskNextQuestion();
            return;
        }

        Debug.Log(
            "ถามข้อ " +
            questionIndex +
            " : " +
            question
        );

        if (dialogManager == null)
        {
            Debug.LogError(
                "Checklist ไม่มี DialogManager"
            );

            return;
        }

        // ==========================================
        // เปิด Checklist Dialog
        // ==========================================

        dialogManager.StartChecklistDialog(
            currentNPC.data.npcName,
            question
        );
    }


    // =====================================================
    // เรียกหลัง Dialog ข้อนึงจบ
    // =====================================================

    public void ChecklistDialogFinished()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < selectedCount)
        {
            AskNextQuestion();
        }
        else
        {
            Debug.Log(
                "Checklist ถามครบทุกข้อแล้ว"
            );
        }
    }
    public void SubmitChecklist()
    {
        if (currentNPC == null)
        {
            Debug.LogWarning("ไม่มี NPC");
            return;
        }

        if (currentNPC.data == null)
            return;

        int scoreThisNPC = 0;

        for (int i = 0; i < 4; i++)
        {
            if (!answered[i])
                continue;

            bool correctAnswer = currentNPC.data.correctAnswers[i];
            bool playerAnswer = playerAnswers[i];

            if (playerAnswer == correctAnswer)
            {
                scoreThisNPC++;
            }
        }

        // เก็บ "คะแนนล่าสุดที่ส่ง" ของ NPC ตัวนี้ (ส่งซ้ำ = เขียนทับของเดิม)
        npcChecklistScores[currentNPC.gameObject] = scoreThisNPC;
        Debug.Log("บันทึกคะแนน checklist ของ " + currentNPC.data.npcName + " = " + scoreThisNPC + " | key = " + currentNPC.gameObject.GetInstanceID());
        checklistScore = scoreThisNPC; // ไว้โชว์ผล/debug

        Debug.Log(currentNPC.data.npcName + " ส่ง checklist ได้ " + scoreThisNPC + " คะแนน (ล่าสุด)");
        
        PlaySound(submitSound); 
        
        CloseChecklist();
    }


    // =====================================================
    // เปลี่ยนหน้า
    // =====================================================

    public void ShowPage1()
    {
        if (page1 != null)
            page1.SetActive(true);

        if (page2 != null)
            page2.SetActive(false);
        
        if (page1 != null) page1.transform.SetAsLastSibling();
        if (postItGoToPage1 != null) postItGoToPage1.transform.SetAsFirstSibling(); // ม่วง -> จมหลังสุด
        if (postItGoToPage2 != null) postItGoToPage2.transform.SetAsLastSibling();  // ฟ้า  -> ลอยหน้าสุด

        PlaySound(changePageSound);       
    }


    public void ShowPage2()
    {
        if (page1 != null)
            page1.SetActive(false);

        if (page2 != null)
            page2.SetActive(true);
        
        if (page2 != null) page2.transform.SetAsLastSibling();
        if (postItGoToPage2 != null) postItGoToPage2.transform.SetAsFirstSibling(); // ฟ้า -> จมหลังสุด
        if (postItGoToPage1 != null) postItGoToPage1.transform.SetAsLastSibling();  // ม่วง -> ลอยหน้าสุด

        PlaySound(changePageSound);
    }


    // =====================================================
    // RESET
    // =====================================================

    private void ResetChecklist()
    {
        for (int i = 0; i < 4; i++)
        {
            selectedQuestions[i] = false;
            questionOrder[i] = 0;

            // ★ เพิ่ม 2 บรรทัดนี้ - เคลียร์คำตอบรอบก่อนหน้า
            playerAnswers[i] = false;
            answered[i] = false;
        }

        selectedCount = 0;
        currentQuestionIndex = 0;

        toggleBag.SetIsOnWithoutNotify(false);
        toggleAppearance.SetIsOnWithoutNotify(false);
        toggleID.SetIsOnWithoutNotify(false);
        toggleEntryDoc.SetIsOnWithoutNotify(false);

        // ★ เพิ่มเคลียร์ toggle หน้า 2
        bagAbnormal.SetIsOnWithoutNotify(false);
        bagNormal.SetIsOnWithoutNotify(false);
        appearanceAbnormal.SetIsOnWithoutNotify(false);
        appearanceNormal.SetIsOnWithoutNotify(false);
        idAbnormal.SetIsOnWithoutNotify(false);
        idNormal.SetIsOnWithoutNotify(false);
        entryDocAbnormal.SetIsOnWithoutNotify(false);
        entryDocNormal.SetIsOnWithoutNotify(false);
    }


    // =====================================================
    // CLOSE
    // =====================================================

    public void CloseChecklist()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false);
        
        if (blocker != null)             
            blocker.SetActive(false);     

        currentNPC = null;
    }
    
    
    private void PlaySound(AudioClip clip)
    {
        if (checklistAudioSource != null && clip != null)
        {
            checklistAudioSource.PlayOneShot(clip);
        }
    }
    
    
}