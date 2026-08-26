using UnityEngine;
using UnityEngine.UI;


public class NPCQuestionManager : MonoBehaviour
{
    public static NPCQuestionManager Instance;

    [Header("Panel")]
    public GameObject askPanel;   // แผง List/Ask (4 หัวข้อ + ปุ่ม Send)
    public GameObject blocker;    // กันคลิกทะลุตอนแผงเปิดอยู่ (ถ้ามี)

    [Header("Question Toggles")]
    public Toggle toggleBag;
    public Toggle toggleAppearance;
    public Toggle toggleID;
    public Toggle toggleEntryDoc;

    [Header("Buttons")]
    public Button sendButton;     // ปุ่ม "Ask" / "Send"

    [Header("Dialog")]
    public DialogManager dialogManager;

    [Header("Sound")]
    public AudioSource askAudioSource;
    public AudioClip openPanelSound;
    public AudioClip sendSound;

    // NPC ที่กำลังถูกถามอยู่ตอนนี้
    private NPC currentAskingNPC;

    private bool[] selectedQuestions = new bool[4];
    private int[] questionOrder = new int[4];
    private int selectedCount = 0;
    private int currentQuestionIndex = 0;
 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (askPanel != null)
            askPanel.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        toggleBag.onValueChanged.AddListener(v => SelectQuestion(0, v));
        toggleAppearance.onValueChanged.AddListener(v => SelectQuestion(1, v));
        toggleID.onValueChanged.AddListener(v => SelectQuestion(2, v));
        toggleEntryDoc.onValueChanged.AddListener(v => SelectQuestion(3, v));

        if (sendButton != null)
            sendButton.onClick.AddListener(StartAskQuestions);
    }


    // =====================================================
    // เรียกจากจุดที่ตรวจจับการคลิก NPC (เช่นใน NPC.cs ตอน OnMouseDown
    // หรือระบบ Obj.ClickEvent ที่มีอยู่แล้วในโปรเจกต์)
    // ตัวอย่างการเรียกใช้:
    //   NPCQuestionManager.Instance.TryOpenQuestionPanel(gameObject);
    // =====================================================

    public void TryOpenQuestionPanel(GameObject npcObject)
    {
        if (GameManager.Instance == null)
            return;

        // ต้องเป็น NPC ตัวปัจจุบันเท่านั้น
        if (GameManager.Instance.currentNPC != npcObject)
        {
            Debug.Log("NPC ตัวนี้ไม่ใช่ currentNPC เปิดถามไม่ได้");
            return;
        }

        // ต้องอยู่สถานะ Inspecting (หยุดที่จุดตรวจแล้ว)
        // เดินอยู่ (WalkingToCheckpoint) หรือกำลังจะออก (Leaving) เปิดไม่ได้
        if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
        {
            Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังจะออก เปิดถามไม่ได้");
            return;
        }

        NPC npc = npcObject.GetComponent<NPC>();

        if (npc == null || npc.data == null)
        {
            Debug.LogWarning("NPC ไม่มี NPC.cs หรือ NPCData");
            return;
        }

        currentAskingNPC = npc;

        OpenPanel();
    }


    // =====================================================
    // OPEN / CLOSE PANEL
    // =====================================================

    private void OpenPanel()
    {
        Debug.Log("[" + Time.frameCount + "] OpenPanel เรียก");
        ResetSelection();

        if (askPanel != null)
            askPanel.SetActive(true);

        if (blocker != null)
            blocker.SetActive(true);

        PlaySound(openPanelSound);
    }


    public void ClosePanel()
    {
        if (askPanel != null)
            askPanel.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);
    }


    // =====================================================
    // เลือกหัวข้อที่จะถาม
    // =====================================================

    private void SelectQuestion(int index, bool isOn)
    {
        if (index < 0 || index >= 4)
            return;

        selectedQuestions[index] = isOn;
    }


    // =====================================================
    // กดปุ่ม Ask / Send
    // =====================================================

    public void StartAskQuestions()
    {
        if (currentAskingNPC == null)
        {
            Debug.LogWarning("ยังไม่มี NPC ที่กำลังถาม");
            return;
        }

        selectedCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (selectedQuestions[i])
            {
                questionOrder[selectedCount] = i;
                selectedCount++;
            }
        }

        if (selectedCount == 0)
        {
            Debug.Log("ยังไม่ได้เลือกหัวข้อคำถาม");
            return;
        }

        currentQuestionIndex = 0;

        PlaySound(sendSound);

        // ปิดแผงเลือกหัวข้อทันทีตามที่ต้องการ
        // (ผู้เล่นคลิก NPC ใหม่เองถ้าอยากถามรอบถัดไป)
        ClosePanel();

        AskNextQuestion();
    }


    // =====================================================
    // ถามทีละข้อ ผ่าน DialogManager
    // =====================================================

    private void AskNextQuestion()
    {
        if (currentAskingNPC == null || currentAskingNPC.data == null)
        {
            currentAskingNPC = null;
            return;
        }

        if (currentQuestionIndex >= selectedCount)
        {
            Debug.Log("ถามครบทุกข้อที่เลือกแล้ว");
            currentAskingNPC = null;
            return;
        }

        int questionIndex = questionOrder[currentQuestionIndex];

        if (currentAskingNPC.data.checkQuestions == null ||
            questionIndex >= currentAskingNPC.data.checkQuestions.Length)
        {
            currentQuestionIndex++;
            AskNextQuestion();
            return;
        }

        string question = currentAskingNPC.data.checkQuestions[questionIndex];

        if (string.IsNullOrWhiteSpace(question))
        {
            currentQuestionIndex++;
            AskNextQuestion();
            return;
        }

        if (dialogManager == null)
        {
            Debug.LogError("NPCQuestionManager ไม่มี DialogManager");
            return;
        }

        dialogManager.StartChecklistDialog(
            currentAskingNPC.data.npcName,
            question
        );
    }


    // =====================================================
    // DialogManager เรียกกลับตอน dialog คำถาม (DialogType.Checklist)
    // ข้อนึงจบแล้ว
    // =====================================================

    public void AskDialogFinished()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < selectedCount)
        {
            AskNextQuestion();
        }
        else
        {
            Debug.Log("ถามคำถามครบทุกข้อในรอบนี้แล้ว");
            currentAskingNPC = null;
        }
    }


    // =====================================================
    // RESET
    // =====================================================

    private void ResetSelection()
    {
        for (int i = 0; i < 4; i++)
        {
            selectedQuestions[i] = false;
            questionOrder[i] = 0;
        }

        selectedCount = 0;
        currentQuestionIndex = 0;

        toggleBag.SetIsOnWithoutNotify(false);
        toggleAppearance.SetIsOnWithoutNotify(false);
        toggleID.SetIsOnWithoutNotify(false);
        toggleEntryDoc.SetIsOnWithoutNotify(false);
    }


    private void PlaySound(AudioClip clip)
    {
        if (askAudioSource != null && clip != null)
        {
            askAudioSource.PlayOneShot(clip);
        }
    }
}