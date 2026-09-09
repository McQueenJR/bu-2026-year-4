using UnityEngine;
using UnityEngine.UI;


public class NPCQuestionManager : MonoBehaviour
{
    public static NPCQuestionManager Instance;

    [Header("Panel")]
    public GameObject askPanel;   // แผง List/Ask (4 หัวข้อ + ปุ่ม Send)

    [Header("Question Buttons")]
    public Button buttonAppearance;
    public Button buttonMagnifyingGlass;
    public Button buttonMatchstick;
    public Button buttonEntryDoc;
    public Button buttonTodayList;
    


    [Header("Dialog")]
    public DialogManager dialogManager;

    [Header("Sound")]
    public AudioSource openPanelSource;
    public AudioSource sendSource;

    // NPC ที่กำลังถูกถามอยู่ตอนนี้
    private NPC currentAskingNPC;
    
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (askPanel != null)
            askPanel.SetActive(false);

        buttonAppearance.onClick.AddListener(() => AskQuestion(0));
        buttonMagnifyingGlass.onClick.AddListener(() => AskQuestion(1));
        buttonMatchstick.onClick.AddListener(() => AskQuestion(2));
        buttonEntryDoc.onClick.AddListener(() => AskQuestion(3));
        buttonTodayList.onClick.AddListener(() => AskQuestion(4));
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
        
        // กันกดระหว่างถือแว่นขยาย / ไม้ขีดไฟ
        if (MagnifyingGlass.Instance != null && MagnifyingGlass.Instance.IsHolding)
            return;
        
        if (Matchbox.Instance != null && Matchbox.Instance.IsHolding)
            return;
        
        // กันกดระหว่างมี dialog เปิดอยู่ / กำลังเรียกตำรวจ / NPC ยังไม่ถึงจุดตรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
            {
                Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ กดไม่ได้ตอนนี้");
                return;
            }
        }

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

        if (askPanel != null)
            askPanel.SetActive(true);

        PlaySound(openPanelSource);
    }


    public void ClosePanel()
    {
        if (askPanel != null)
            askPanel.SetActive(false);

    }

    

    // =====================================================
    // กด text ข้อไหน → ถามข้อนั้นทันที
    // =====================================================

    private void AskQuestion(int questionIndex)
    {
        if (currentAskingNPC == null || currentAskingNPC.data == null)
        {
            Debug.LogWarning("ยังไม่มี NPC ที่กำลังถาม");
            return;
        }

        if (currentAskingNPC.data.checkQuestions == null ||
            questionIndex >= currentAskingNPC.data.checkQuestions.Length)
        {
            return;
        }

        string question = currentAskingNPC.data.checkQuestions[questionIndex];

        if (string.IsNullOrWhiteSpace(question))
            return;

        if (dialogManager == null)
        {
            Debug.LogError("NPCQuestionManager ไม่มี DialogManager");
            return;
        }

        PlaySound(sendSource);

        NPCMouthAnimation mouth = currentAskingNPC.GetComponentInChildren<NPCMouthAnimation>();
        if (mouth != null)
        {
            dialogManager.SetTalkingNPC(mouth);
        }
        else
        {
            Debug.LogWarning("NPC " + currentAskingNPC.data.npcName + " ไม่มี NPCMouthAnimation");
        }

        // ปิดแผงทันที ผู้เล่นคลิก NPC ใหม่เองถ้าอยากถามข้ออื่นต่อ
        ClosePanel();

        dialogManager.StartChecklistDialog(
            currentAskingNPC.data.npcName,
            question
        );
    }


    // =====================================================
    // DialogManager เรียกกลับตอน dialog คำถามจบ
    // =====================================================

    public void AskDialogFinished()
    {
        currentAskingNPC = null;
    }
    

    private void PlaySound(AudioSource source)
    {
        if (source != null)
        {
            source.Play();
        }
    }
}