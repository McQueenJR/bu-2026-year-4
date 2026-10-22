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

    [Header("NPC ลืมยื่นใบคำขอเข้าวัด")]
    [Tooltip("โอกาส (%) ที่ NPC ที่มีเอกสารจริงจะลืมยื่น (0 = ไม่มีใครลืม, 100 = ลืมทุกคน ใช้ทดสอบ)")]
    [Range(0f, 100f)]
    public float forgotDocumentChance = 25f;

    [Tooltip("ข้อความส่วนกลาง ใช้ร่วมกับ NPC ทุกตัว (ใส่หลายอันจะสุ่มเล่น)")]
    [TextArea(2, 5)]
    public string[] forgotDocumentMessages = { "ขอโทษที ลืมส่งเอกสาร" };

    //เพิ่มเข้ามา22/10
    [Header("NPC ลืมเอกสารไว้ในบ้าน")]
    [Tooltip("ข้อความส่วนกลางสำหรับ NPC ที่ลืมเอกสารไว้ในบ้าน สุ่ม 1 ข้อความต่อ NPC 1 คน แล้วจำไว้ ถามซ้ำได้ข้อความเดิม (เพิ่ม/ลบ/แก้ได้ที่นี่)")]
    [TextArea(2, 5)]
    public string[] forgotDocumentInHouseMessages =
    {
        "ฉันน่าจะลืมเอกสารไว้ข้างในบ้าน",
        "เอกสารของฉันยังอยู่ในห้องหรือเปล่านะ",
        "ฉันจำได้ว่ายังไม่ได้เอาเอกสารออกมาจากบ้าน"
    };

    [Tooltip("ปิด = Ghost จะไม่พูดประโยคลืมเอกสารในบ้าน (ค่าเริ่มต้นปลอดภัย)")]
    public bool allowGhostForgotInHouseDialog = false;
    // index ของปุ่ม buttonEntryDoc ใน AskQuestion
    private const int EntryDocQuestionIndex = 3;

    // true = บทสนทนาที่เล่นอยู่คือ "ลืมยื่น" → จบแล้วต้องวางเอกสารบนโต๊ะ
    private bool pendingForgotDocumentPlacement = false;
    
    
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
        pendingForgotDocumentPlacement = false;
        
        if (currentAskingNPC == null || currentAskingNPC.data == null)
        {
            Debug.LogWarning("ยังไม่มี NPC ที่กำลังถาม");
            return;
        }
        if (questionIndex == EntryDocQuestionIndex &&
            ShouldPlayForgotDocumentDialog(currentAskingNPC))
        {
            if (dialogManager == null)
            {
                Debug.LogError("NPCQuestionManager ไม่มี DialogManager");
                return;
            }

            PlayForgotDocumentDialog(currentAskingNPC);
            return;
        }
        
        //เพิ่มเข้ามา 22-10
        
        // ★ NPC ลืมเอกสารไว้ "ในบ้าน" (แยกจากเคสลืมไว้กับตัวข้างบน เคสเดิมมีสิทธิ์ก่อน)
        if (questionIndex == EntryDocQuestionIndex &&
            ShouldPlayForgotDocumentInHouseDialog(currentAskingNPC))
        {
            if (dialogManager == null)
            {
                Debug.LogError("NPCQuestionManager ไม่มี DialogManager");
                return;
            }

            PlayForgotDocumentInHouseDialog(currentAskingNPC);
            return;
        }
        
        if (currentAskingNPC.data.selectedCheckQuestions == null ||
            questionIndex >= currentAskingNPC.data.selectedCheckQuestions.Length)
        {
            return;
        }

        string question = currentAskingNPC.data.selectedCheckQuestions[questionIndex];

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
    // NPC ลืมยื่นใบคำขอเข้าวัด
    // =====================================================

    // เล่นบทสนทนากลางเมื่อ NPC มีเอกสารจริง + ถูกสุ่มให้ลืม + เอกสารยังไม่ถูกวางบนโต๊ะ
    // วางแล้ว = ถามครั้งต่อไปใช้บทสนทนาจาก NPCData ตามระบบเดิม
    private bool ShouldPlayForgotDocumentDialog(NPC npc)
    {
        if (npc == null || npc.applicant == null)
            return false;

        TodayApplicant a = npc.applicant;

        return a.forgotDocument &&
               a.hasTempleDocument &&
               a.templeDocument != null &&
               !a.documentPlacedOnDesk;
    }

    private void PlayForgotDocumentDialog(NPC npc)
    {
        string message = "ขอโทษที ลืมส่งเอกสาร";

        if (forgotDocumentMessages != null && forgotDocumentMessages.Length > 0)
        {
            string picked = forgotDocumentMessages[
                Random.Range(0, forgotDocumentMessages.Length)
            ];

            if (!string.IsNullOrWhiteSpace(picked))
                message = picked;
        }

        PlaySound(sendSource);

        NPCMouthAnimation mouth = npc.GetComponentInChildren<NPCMouthAnimation>();
        if (mouth != null)
            dialogManager.SetTalkingNPC(mouth);

        ClosePanel();

        pendingForgotDocumentPlacement = true;

        // ใช้ Dialog แบบ Checklist เดิม → จบแล้ว DialogManager เรียก AskDialogFinished()
        dialogManager.StartChecklistDialog(npc.data.npcName, message);
    }
    
    
    // =====================================================
    // NPC ลืมเอกสารไว้ "ในบ้าน"
    // =====================================================

    // เงื่อนไข: CampManager บอกว่าเอกสารไม่อยู่กับตัว (hasDocument = false)
    // และไม่ชนกับเคส "ลืมไว้กับตัว" เดิม / เอกสารยังไม่ถูกวางบนโต๊ะ
    private bool ShouldPlayForgotDocumentInHouseDialog(NPC npc)
    {
        if (npc == null || npc.data == null || npc.applicant == null)
            return false;

        TodayApplicant a = npc.applicant;

        // มีเอกสารติดตัว = ไม่ใช่เคส "ลืมไว้ในบ้าน"
        // (SpawnManager ตั้ง hasTempleDocument = false เมื่อสุ่มให้ลืมไว้ในบ้าน)
        if (a.hasTempleDocument)
            return false;

        // กันซ้อนกับเคสลืมไว้กับตัวเดิม
        if (a.forgotDocument)
            return false;

        // เอกสารถูกวางบนโต๊ะแล้ว ไม่ต้องพูดว่าลืมในบ้านอีก
        if (a.documentPlacedOnDesk)
            return false;

        if (a.isGhost && !allowGhostForgotInHouseDialog)
            return false;

        Debug.Log("[InHouse] " + npc.data.npcName + " ลืมเอกสารไว้ในบ้าน → ใช้ข้อความใหม่");
        return true;
    }
    
    //เพิ่มเข้ามา 22-10
    // เล่นบทสนทนา "ลืมเอกสารไว้ในบ้าน"
    // ★ ไม่ตั้ง pendingForgotDocumentPlacement → จบแล้วจะไม่วางเอกสารบนโต๊ะ
    private void PlayForgotDocumentInHouseDialog(NPC npc)
    {
        string message = GetForgotDocumentInHouseMessage(npc.applicant);

        PlaySound(sendSource);

        NPCMouthAnimation mouth = npc.GetComponentInChildren<NPCMouthAnimation>();
        if (mouth != null)
            dialogManager.SetTalkingNPC(mouth);

        ClosePanel();

        // กันพลาด: เคสนี้ห้ามไปเรียก PlaceForgottenDocumentOnDesk ตอนจบ
        pendingForgotDocumentPlacement = false;

        dialogManager.StartChecklistDialog(npc.data.npcName, message);
    }

    // สุ่มข้อความครั้งแรกแล้วเก็บไว้กับ NPC คนนั้น (TodayApplicant) ครั้งต่อไปใช้ข้อความเดิม
    private string GetForgotDocumentInHouseMessage(TodayApplicant a)
    {
        if (!string.IsNullOrWhiteSpace(a.forgotDocumentInHouseMessage))
            return a.forgotDocumentInHouseMessage;

        string picked = "ฉันน่าจะลืมเอกสารไว้ข้างในบ้าน";   // ข้อความสำรอง ถ้ารายการว่าง

        if (forgotDocumentInHouseMessages != null)
        {
            // นับเฉพาะช่องที่ไม่ว่าง
            int validCount = 0;
            for (int i = 0; i < forgotDocumentInHouseMessages.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(forgotDocumentInHouseMessages[i]))
                    validCount++;
            }

            if (validCount > 0)
            {
                int target = Random.Range(0, validCount);

                for (int i = 0; i < forgotDocumentInHouseMessages.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(forgotDocumentInHouseMessages[i]))
                        continue;

                    if (target == 0)
                    {
                        picked = forgotDocumentInHouseMessages[i];
                        break;
                    }

                    target--;
                }
            }
        }

        a.forgotDocumentInHouseMessage = picked;
        return picked;
    }
    
    
    // =====================================================
    // DialogManager เรียกกลับตอน dialog คำถามจบ
    // =====================================================

    public void AskDialogFinished()
    {
        NPC finishedNPC = currentAskingNPC;
        currentAskingNPC = null;

        // จบบทสนทนา "ลืมยื่น" → นำเอกสารจริงมาวางบนโต๊ะ
        if (pendingForgotDocumentPlacement)
        {
            pendingForgotDocumentPlacement = false;

            if (GameManager.Instance != null)
                GameManager.Instance.PlaceForgottenDocumentOnDesk(finishedNPC);
        }
    }
    

    private void PlaySound(AudioSource source)
    {
        if (source != null)
        {
            source.Play();
        }
    }
}