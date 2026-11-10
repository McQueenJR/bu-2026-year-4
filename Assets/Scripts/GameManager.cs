using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool greenDialogTriggered = false;  
    public static GameManager Instance;
    private bool releasedToCamp = false;
   public GameObject currentNPC;
    public SpawnManager spawner;
    public TodayListManager todayListManager;

    public Transform enterPoint;
    public Transform exitPoint;

    [Header("Day System")]
    public int currentDay = 1;
    public int maxDay = 7;
    public int npcPerDay;

    public int currentHour;
    public int startHour = 20;
    public int endHour = 6;

    public ClockManager clockManager;
    public CalendarManager calendarManager;

    [Header("Emergency")]
    public bool emergencyMode = false;
    public EmergencyManager emergencyManager;
    
    [Header("Phone")]
    public PhoneManager phoneManager;
    
    [Header("Special Event")]
    public SpecialEventManager specialEventManager;

    public enum NPCState
    {
        WalkingToCheckpoint,
        WaitingDecision,
        Inspecting,
        Leaving
    }

    public NPCState currentState;

    [Header("Dialog")]
    public DialogManager dialogManager;
    private GameObject emergencyDialogNPC;
    /*
    [Header("Bag")]
    public GameObject bagPrefab;
    public Transform spawnPointBag;
    public Vector3 bagScale = Vector3.one;
    public Vector3 bagRotation = Vector3.zero;   

    private GameObject currentBag;

    [Header("ID Card")]
    public GameObject idCardPrefab;        // บัตรคนทั่วไป
    public GameObject monkIdCardPrefab;    // บัตรพระ
    public Transform spawnPointIDCard;
    public Vector3 idCardScale = Vector3.one;
    public Vector3 idCardRotation = Vector3.zero;

    private GameObject currentIDCard;
  */  
    [Header("Temple Document")]
    public GameObject templeDocumentPrefab;     // Prefab เอกสาร
    public Transform spawnPointDocument;        // จุด Spawn เอกสาร

    private GameObject currentDocument;
    
    public bool currentNPCMissingDocument = false;

    // =========================
    // SPAWN SOUNDS
    // =========================
   /* [Header("Spawn Sounds")]
    public AudioSource spawnAudioSource;
    public AudioClip bagAndCardSpawnSound;*/

    [Header("Police Call")]
    public GameObject policePrefab;
    public Transform spawnPolice;
    public Transform exitPolicePoint;
    public AudioSource policeSound;
    public bool isPoliceSequenceActive = false;

    private GameObject currentPolice;

    [Header("Day Stats")]
    public int npcProcessedCount = 0;

    public int score = 0;
    public int villagerPassed = 0;
    public int villagerArrested = 0;
    public int robberPassed = 0;
    public int robberArrested = 0;

    public int villagerRejected = 0;
    public int robberRejected = 0;
    
    // [ENDING-SCORE] คะแนนต่อ NPC 1 ตัว: ตัดสิน 80% + เช็คลิสต์ 20% (ค่าจริงอยู่ที่ ScoreRankUtility)
    private const int PointsCorrectDecision = ScoreRankUtility.PointsCorrectDecision;   // ปล่อยชาวบ้าน / กำจัดผี
    private const int PointsRejectGhost = ScoreRankUtility.PointsRejectGhost;           // ปฏิเสธผี
    private const int PointsRejectVillager = ScoreRankUtility.PointsRejectVillager;     // ปฏิเสธชาวบ้านทั่วไป
    // ผิดร้ายแรง (ปล่อยผี / Emergency ใส่ชาวบ้าน / ปล่อยชาวบ้านเข้าห้องที่มีผี) = NPC ตัวนั้นได้ 0

    [Header("End Day UI")]
    public EndDayUI endDayUI;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Developer Tools (Editor Only)")]
    [SerializeField] private bool enableDeveloperHotkeys = true;
    private bool devSkipDayTriggered = false;
    private bool showNPCStatusWindow = false;
    private Rect npcStatusWindowRect = new Rect(20, 80, 520, 390);
#endif


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentDay = 1;
        currentHour = startHour;
        
        // เริ่มเกมใหม่ → เจ้าอาวาสกลับมามีชีวิต
        if (AbbotStatusManager.Instance != null)
            AbbotStatusManager.Instance.ResetForNewGame();
        
        if (CampManager.Instance != null)
        {
            CampManager.Instance.StartNewDay(currentDay);
        }
        
        clockManager.SetDay(currentDay);
        clockManager.SetHour(currentHour);

        calendarManager.SetDay(currentDay);

        if (todayListManager != null)
            todayListManager.ResetForNewDay();

        spawner.GenerateTodayApplicants();
        BeginNPCFlowOfTheDay();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // =====================================================
    // DEVELOPER HOTKEYS (Unity Editor only)
    // F1 = NPC truth/debug log
    // F2 = Force Approve (ข้ามบทพูด/คัตซีน)
    // F3 = Force Reject (ข้ามบทพูด/คัตซีน)
    // F4 = Force End Day (ข้าม NPC ที่เหลือ)
    // F5 = Toggle NPC status window (player checklist + NPC type)
    // =====================================================
    private void Update()
    {
        if (!enableDeveloperHotkeys || Keyboard.current == null)
            return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            DevLogCurrentNPC();

        if (Keyboard.current.f2Key.wasPressedThisFrame)
            DevForceApprove();

        if (Keyboard.current.f3Key.wasPressedThisFrame)
            DevForceReject();

        if (Keyboard.current.f4Key.wasPressedThisFrame)
            DevSkipDay();

        if (Keyboard.current.f5Key.wasPressedThisFrame)
            showNPCStatusWindow = !showNPCStatusWindow;
    }

    private void OnGUI()
    {
        if (!enableDeveloperHotkeys || !showNPCStatusWindow)
            return;

        npcStatusWindowRect = GUI.Window(
            9051,
            npcStatusWindowRect,
            DrawNPCStatusWindow,
            "NPC STATUS (F5)"
        );
    }

    private void DrawNPCStatusWindow(int windowID)
    {
        if (currentNPC == null)
        {
            GUILayout.Label("ไม่มี NPC อยู่ที่จุดตรวจ");
            GUILayout.Space(8);
            GUILayout.Label("กด F5 เพื่อปิดหน้าต่างนี้");
            GUI.DragWindow();
            return;
        }

        NPC npc = currentNPC.GetComponent<NPC>();
        if (npc == null)
        {
            GUILayout.Label("ไม่พบ Component NPC");
            GUI.DragWindow();
            return;
        }

        bool isGhost = npc.applicant != null && npc.applicant.isGhost;
        string npcName = currentNPC.name;
        if (npc.applicant != null && npc.applicant.displayData != null)
            npcName = npc.applicant.displayData.npcName;
        else if (npc.data != null)
            npcName = npc.data.npcName;

        GUILayout.Label("NPC: " + npcName);
        GUILayout.Label("ประเภท: " + (isGhost ? "ผี (GHOST)" : "คน (PERSON)"));
        GUILayout.Space(8);

        bool[] playerAnswers = null;
        bool[] answeredFlags = null;
        bool hasPlayerAnswers = ChecklistManager.Instance != null &&
            ChecklistManager.Instance.TryGetPlayerAnswers(currentNPC, out playerAnswers, out answeredFlags);
        bool[] correctAnswers = npc.data != null ? npc.data.correctAnswers : null;

        string[] labels =
        {
            "รูปลักษณ์ภายนอก",
            "แว่นขยาย",
            "ไม้ขีดไฟ",
            "ใบคำขอเข้าวัด",
            "รายชื่อคนที่มาวันนี้"
        };

        GUILayout.Label("CHECKLIST DEBUG");
        GUILayout.BeginHorizontal();
        GUILayout.Label("รายการ", GUILayout.Width(165));
        GUILayout.Label("เฉลย (Inspector)", GUILayout.Width(125));
        GUILayout.Label("ผู้เล่นตอบ", GUILayout.Width(120));
        GUILayout.EndHorizontal();

        for (int i = 0; i < labels.Length; i++)
        {
            bool hasCorrectAnswer = correctAnswers != null && i < correctAnswers.Length;
            bool correctAnswer = hasCorrectAnswer && correctAnswers[i];

            // ใช้เงื่อนไขเดียวกับ ChecklistManager.SubmitChecklist()
            if (i == 3 && npc.applicant != null && !npc.applicant.hasTempleDocument)
            {
                correctAnswer = true;
                hasCorrectAnswer = true;
            }

            string truthText = !hasCorrectAnswer ? "ไม่มีเฉลย" :
                (correctAnswer ? "ผิดปกติ" : "ปกติ");

            string playerText = "ยังไม่ได้ติ๊ก";
            if (hasPlayerAnswers && playerAnswers != null && answeredFlags != null &&
                i < playerAnswers.Length && i < answeredFlags.Length && answeredFlags[i])
            {
                playerText = playerAnswers[i] ? "ผิดปกติ" : "ปกติ";
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(labels[i], GUILayout.Width(165));
            GUILayout.Label(truthText, GUILayout.Width(125));
            GUILayout.Label(playerText, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }

        int answeredCount = 0;
        if (hasPlayerAnswers && answeredFlags != null)
        {
            for (int i = 0; i < Mathf.Min(5, answeredFlags.Length); i++)
                if (answeredFlags[i]) answeredCount++;
        }

        GUILayout.Space(8);
        GUILayout.Label("ตอบแล้ว: " + answeredCount + "/5 ข้อ");
        GUILayout.Label("เฉลยอ้างอิง Correct Answers ใน NPCData");
        GUILayout.Space(8);
        GUILayout.Label("กด F5 เพื่อปิดหน้าต่างนี้");
        GUI.DragWindow();
    }

    private bool DevCanDecide()
    {
        if (currentNPC == null)
        {
            Debug.LogWarning("[DEV] ไม่มี NPC ปัจจุบัน");
            return false;
        }

        if (currentState != NPCState.Inspecting)
        {
            Debug.LogWarning($"[DEV] ยังตัดสินใจไม่ได้ State ปัจจุบัน: {currentState}");
            return false;
        }

        return true;
    }

    private void DevLogCurrentNPC()
    {
        if (currentNPC == null)
        {
            Debug.LogWarning("[DEV F1] ไม่มี NPC ปัจจุบัน");
            return;
        }

        NPC npc = currentNPC.GetComponent<NPC>();
        if (npc == null || npc.data == null)
        {
            Debug.LogWarning("[DEV F1] NPC ไม่มี Component NPC หรือ NPCData");
            return;
        }

        bool isGhost = npc.applicant != null && npc.applicant.isGhost;
        string shownName = npc.data.npcName;
        if (isGhost && npc.applicant.ghostTarget != null)
            shownName = npc.applicant.ghostTarget.npcName;
        else if (npc.applicant != null && npc.applicant.displayData != null)
            shownName = npc.applicant.displayData.npcName;

        bool[] answers = npc.data.correctAnswers;
        string[] labels =
        {
            "Appearance",
            "Magnifying Glass",
            "Matchbox",
            "Entry Document",
            "Today List"
        };

        System.Text.StringBuilder log = new System.Text.StringBuilder();
        log.AppendLine("========== NPC DEBUG (F1) ==========");
        log.AppendLine($"NPC: {shownName}");
        log.AppendLine($"Prefab/Object: {currentNPC.name}");
        log.AppendLine($"Type: {(isGhost ? "GHOST" : "PERSON")}");
        log.AppendLine($"Ghost Target: {(npc.applicant != null && npc.applicant.ghostTarget != null ? npc.applicant.ghostTarget.npcName : "None")}");
        log.AppendLine("[CHECKLIST TRUTH]");

        for (int i = 0; i < labels.Length; i++)
        {
            if (answers == null || i >= answers.Length)
            {
                log.AppendLine($"{labels[i]}: UNKNOWN (ไม่มี correctAnswers[{i}])");
                continue;
            }

            bool abnormal = answers[i];

            // ใช้เงื่อนไขเดียวกับ ChecklistManager.SubmitChecklist()
            if (i == 3 && npc.applicant != null && !npc.applicant.hasTempleDocument)
                abnormal = true;

            log.AppendLine($"{labels[i]}: {(abnormal ? "ABNORMAL" : "NORMAL")}");
        }

        log.AppendLine($"SUMMARY: {(isGhost ? "GHOST" : "PERSON")}");
        log.AppendLine("====================================");
        Debug.Log(log.ToString(), currentNPC);
    }

    private void DevForceApprove()
    {
        if (!DevCanDecide())
            return;

        Debug.Log("[DEV F2] Force Approve → เรียก ReleaseCurrentNPC() โดยตรง");
        ReleaseCurrentNPC();
    }

    private void DevForceReject()
    {
        if (!DevCanDecide())
            return;

        Debug.Log("[DEV F3] Force Reject → เรียก RejectCurrentNPC() โดยตรง");
        RejectCurrentNPC();
    }

    private void DevSkipDay()
    {
        if (devSkipDayTriggered)
        {
            Debug.LogWarning("[DEV F4] ข้ามวันไปแล้ว รอเริ่มวันถัดไปก่อน");
            return;
        }

        if (currentState == NPCState.Leaving)
        {
            Debug.LogWarning("[DEV F4] NPC กำลังเดินออก กรุณารอให้การตัดสินใจปัจจุบันจบก่อน");
            return;
        }

        // ปิด Checklist หากเปิดอยู่
        if (ChecklistManager.Instance != null &&
            ChecklistManager.Instance.checklistPanel != null &&
            ChecklistManager.Instance.checklistPanel.activeSelf)
        {
            ChecklistManager.Instance.CloseChecklist();
        }

        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.HideDecisionButtons();

        // ข้าม NPC ที่ค้างอยู่โดยไม่บันทึกการตัดสินใจหรือคะแนน
        if (currentNPC != null)
        {
            DestroyBagAndSlideBack();
            Destroy(currentNPC);
            currentNPC = null;
        }

        currentState = NPCState.WalkingToCheckpoint;
        devSkipDayTriggered = true;

        Debug.Log("[DEV F4] Force End Day → ข้าม NPC ที่เหลือและเปิดสรุปวัน");
        EndGame();
    }
#endif

    // =========================
    // BUTTON
    // =========================

  

    public void GreenDialogFinished()
    {
        if (currentNPC == null)
            return;
        if (currentState != NPCState.Inspecting) return;   

        Debug.Log("Green Dialog จบ → ปล่อย NPC");

        ReleaseCurrentNPC();
    }

    public void RedDialogFinished()                  
    {
        if (currentNPC == null)
            return;
        if (currentState != NPCState.Inspecting) return;

        Debug.Log("Red Dialog จบ → ปฏิเสธ NPC");

        RejectCurrentNPC();
    }
    

    
    // =========================
    // NPC CHECKPOINT
    // =========================

    public void NPCReachedCheckpoint(GameObject npc)
    {
        currentNPC = npc;
        currentState = NPCState.Inspecting;
        greenDialogTriggered = false;

        if (GreenRedButtonManager.Instance != null)
                    GreenRedButtonManager.Instance.HideDecisionButtons();
        
        // เพิ่มใหม่: ตรวจว่าเป็น NPC อีเวนต์พิเศษหรือไม่
        SpecialEventNPC special = npc.GetComponent<SpecialEventNPC>();

        if (special != null && special.IsSpecialEvent)
        {
            if (specialEventManager != null)
            {
                specialEventManager.StartSpecialEvent(
                    npc,
                    special.eventConfig
                );
                return;
            }
            else
            {
                Debug.LogError(
                    "SpecialEventManager is not assigned in GameManager!"
                );
            }
        }

        // ==========================================
        // เช็กเอกสารของ NPC เมื่อถึงจุดตรวจ
        // ==========================================
        currentNPCMissingDocument = false;

        NPC npcScript = npc.GetComponent<NPC>();

        if (npcScript != null && npcScript.applicant != null)
        {
            currentNPCMissingDocument =
                !npcScript.applicant.hasTempleDocument;

            if (currentNPCMissingDocument)
            {
                Debug.Log(
                    $"📄❌ {npcScript.applicant.displayData.npcName} " +
                    $"ไม่มีเอกสาร → ต้องใช้โทรศัพท์ตรวจสอบ"
                );
            }
            else
            {
                Debug.Log(
                    $"📄✅ {npcScript.applicant.displayData.npcName} " +
                    $"มีเอกสาร"
                );
            }

            // สุ่มว่า NPC ที่มีเอกสารจริงจะลืมยื่นหรือไม่
            DecideForgotDocument(npcScript.applicant);
        }

        if (phoneManager != null &&
            npcScript != null &&
            npcScript.applicant != null &&
            npcScript.applicant.displayData != null)
        {
            phoneManager.StartPhoneCheck(
                npcScript.applicant.displayData
            );
        }

        StartNPCDialog();
    }
    

    // =========================
    // FORGOT DOCUMENT
    // =========================

    // ตัดสินครั้งเดียวต่อ applicant
    // เฉพาะ NPC ที่มีเอกสารจริงเท่านั้น (เคสเอกสารอยู่ Camp จะไม่ถูกเลือก)
    private void DecideForgotDocument(TodayApplicant applicant)
    {
        if (applicant == null || applicant.forgotDocumentDecided)
            return;

        applicant.forgotDocumentDecided = true;
        applicant.forgotDocument = false;
        applicant.documentPlacedOnDesk = false;

        if (!applicant.hasTempleDocument || applicant.templeDocument == null)
            return;

        // ค่า % ตั้งที่ NPCQuestionManager (ไม่มี = ปิดระบบ)
        float chancePercent = NPCQuestionManager.Instance != null
            ? NPCQuestionManager.Instance.forgotDocumentChance
            : 0f;

        if (chancePercent <= 0f)
            return;

        if (Random.value < chancePercent / 100f)
        {
            applicant.forgotDocument = true;

            Debug.Log(
                $"📄😅 {applicant.displayData?.npcName} มีเอกสาร แต่ลืมยื่น"
            );
        }
    }

    // เรียกจาก NPCQuestionManager หลังบทสนทนา "ลืมยื่น" จบ
    // นำเอกสารจริงมาวางบนโต๊ะ (ใช้ SpawnDocument เดิม)
    public void PlaceForgottenDocumentOnDesk(NPC npc)
    {
        if (npc == null || currentNPC == null)
            return;

        if (npc.gameObject != currentNPC)
            return;

        if (currentState != NPCState.Inspecting)
            return;

        TodayApplicant a = npc.applicant;

        if (a == null)
            return;

        if (!a.forgotDocument || a.documentPlacedOnDesk)
            return;

        if (!a.hasTempleDocument || a.templeDocument == null)
            return;

        SpawnDocument();

        if (currentDocument != null)
            a.documentPlacedOnDesk = true;
    }
    
    
    // =========================
    // DIALOG
    // =========================

    private void StartNPCDialog()
    {
        if (currentNPC == null)
            return;

        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError("NPC ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError("NPC ไม่มี NPCData");
            return;
        }

        SetCurrentNPCMouthTalking();
        dialogManager.StartDialog(npc.data);
    }

    public void DialogFinished()
    {
        if (currentNPC == null)
            return;

        if (currentState != NPCState.Inspecting)
            return;

        Debug.Log("Dialog จบ");


        // NPC ที่ลืมยื่นเอกสาร → ยังไม่วางเอกสารบนโต๊ะ
        NPC forgotCheckNpc = currentNPC.GetComponent<NPC>();

        if (forgotCheckNpc != null &&
            forgotCheckNpc.applicant != null &&
            forgotCheckNpc.applicant.forgotDocument)
        {
            return;
        }
        
        SpecialEventNPC special =
            currentNPC != null
                ? currentNPC.GetComponent<SpecialEventNPC>()
                : null;

        if (special != null && special.IsSpecialEvent)
        {
            if (specialEventManager != null)
            {
                specialEventManager.OnSpecialDialogFinished(
                    currentNPC,
                    special.eventConfig
                );
            }

            return;
        }
        
         //   SpawnBag();
         SpawnDocument();

    }
    
    public void StartEmergencyDialog()
    {
        if (currentNPC == null)
        {
            Debug.Log("ไม่มี NPC สำหรับ Emergency Dialog");
            return;
        }

        // NPC ตัวนี้เคยพูด Emergency แล้ว
        
        if (emergencyDialogNPC == currentNPC)
        {
            Debug.Log("NPC ตัวนี้เคยพูด Emergency Dialog แล้ว");
            return;
        }
        

        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError("NPC ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError("NPC ไม่มี NPCData");
            return;
        }

        // ไม่มี Dialog → ข้ามไปเลย
        if (npc.data.emergencyDialogs == null ||
            npc.data.emergencyDialogs.Length == 0)
        {
            Debug.Log("NPC " + npc.data.npcName + " ไม่มี Emergency Dialog");
            return;
        }

        // จำ NPC ตัวนี้ไว้ว่าเคยพูดแล้ว
        emergencyDialogNPC = currentNPC;
        SetCurrentNPCMouthTalking();
        dialogManager.StartEmergencyDialog(npc.data);
    }
    
    // =========================
    // DIALOG MOUTH ANIMATION
    // =========================

    public void SetCurrentNPCMouthTalking()
    {
        if (currentNPC == null)
            return;

        NPCMouthAnimation mouth =
            currentNPC.GetComponentInChildren<NPCMouthAnimation>();

        if (mouth == null)
        {
            Debug.LogWarning(
                "NPC " + currentNPC.name +
                " ไม่มี NPCMouthAnimation"
            );

            return;
        }

        dialogManager.SetTalkingNPC(mouth);
    }
    
    

    // =========================
    // BAG
    // =========================

 /*   private void SpawnBag()
    {
        if (bagPrefab == null)
        {
            Debug.LogError("ไม่ได้ใส่ Bag Prefab");
            return;
        }

        if (spawnPointBag == null)
        {
            Debug.LogError("ไม่ได้ใส่ Spawn Point Bag");
            return;
        }

        currentBag = Instantiate(
            bagPrefab,
            spawnPointBag.position,
            Quaternion.Euler(bagRotation)
        );

        currentBag.transform.localScale = bagScale;

        SpawnIDCard();
        SpawnDocument();
    }

    private void SpawnIDCard()
    {
        if (currentNPC == null)
        {
            Debug.LogError("ไม่มี NPC ปัจจุบัน");
            return;
        }

        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError("NPC ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError("NPC ไม่มี NPCData");
            return;
        }

        GameObject prefabToSpawn = npc.data.idCardPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError(
                "NPC " + npc.data.npcName +
                " ยังไม่ได้ใส่ ID Card Prefab"
            );
            return;
        }

        currentIDCard = Instantiate(
            prefabToSpawn,
            spawnPointIDCard.position,
            Quaternion.Euler(idCardRotation)
        );

        currentIDCard.transform.localScale = idCardScale;

        // เล่นเสียงเมื่อกระเป๋า + บัตร Spawn ครบแล้ว
        if (spawnAudioSource != null && bagAndCardSpawnSound != null)
        {
            spawnAudioSource.PlayOneShot(bagAndCardSpawnSound);
        }
    }
  */  
 private void SpawnDocument()
 {
     if (currentNPC == null)
         return;

     NPC npc = currentNPC.GetComponent<NPC>();

     if (npc == null || npc.applicant == null)
     {
         Debug.LogError("NPC ไม่มีข้อมูล Applicant");
         return;
     }

     // เช็กว่า Applicant คนนี้มีเอกสารหรือไม่
     if (!npc.applicant.hasTempleDocument ||
         npc.applicant.templeDocument == null)
     {
         Debug.Log(
             $"NPC '{npc.applicant.displayData.npcName}' ไม่มีเอกสาร → ไม่ Spawn เอกสาร"
         );
         return;
     }

     if (templeDocumentPrefab == null)
     {
         Debug.LogError("ไม่ได้ใส่ Temple Document Prefab");
         return;
     }

     if (spawnPointDocument == null)
     {
         Debug.LogError("ไม่ได้ใส่ Spawn Point Document");
         return;
     }

     // ลบเอกสารเก่าก่อน ป้องกันเอกสารซ้อน
     if (currentDocument != null)
     {
         Destroy(currentDocument);
         currentDocument = null;
     }

     currentDocument = Instantiate(
         templeDocumentPrefab,
         spawnPointDocument.position,
         Quaternion.identity
     );

     Debug.Log(
         $"NPC '{npc.applicant.displayData.npcName}' มีเอกสาร → Spawn เอกสารแล้ว"
     );
 }

    private void DestroyBagAndSlideBack()
    {
     /*   if (currentBag != null)
        {
            Destroy(currentBag);
            currentBag = null;
        }

        if (currentIDCard != null)
        {
            Destroy(currentIDCard);
            currentIDCard = null;
        }*/
        if (currentDocument != null)
        {
            Destroy(currentDocument);
            currentDocument = null;
        }
        
    }

    // =========================
    // RELEASE NPC
    // =========================

    public void ReleaseCurrentNPC()
    {
        if (currentNPC == null) return;
        if (currentState == NPCState.Leaving) return;

        RecordDecision(currentNPC, wasArrested: false);
        releasedToCamp = true;
    
        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc != null && npc.applicant != null)
        {
            npc.applicant.hasEnteredToday = true;

            // ==========================
            // GHOST
            // ==========================
            if (npc.applicant.isGhost &&
                npc.applicant.ghostTarget != null)
            {
                if (CampManager.Instance != null)
                {
                    CampManager.Instance.GhostEnterCamp(
                        npc.applicant.ghostTarget
                    );
                }
            }
        }

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        NPCMovement move = currentNPC.GetComponent<NPCMovement>();
        move.MoveTo(enterPoint.position);

        StartCoroutine(WaitForExitThenAdvanceHour(currentNPC));
    }

    // =========================
    // REJECT NPC
    // =========================

    public void RejectCurrentNPC()
    {
        if (currentNPC == null)
            return;

        // ==========================
        // นับสถิติการปฏิเสธ
        // ==========================
        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc != null)
        {
            bool isRobber =
                (npc.applicant != null && npc.applicant.isGhost) ||
                npc.npcType == NPCType.Special;

            // [ENDING-SCORE] คะแนน NPC ตัวนี้ = checklist + ปฏิเสธ
            //   ปฏิเสธผี 14 | ปฏิเสธชาวบ้านทั่วไป 6 | ปฏิเสธชาวบ้านที่ห้องมีผีแล้ว = ตัดสินถูก 20
            int rejectPoints =
                isRobber ? PointsRejectGhost
                : IsVillagerRoomHaunted(npc) ? PointsCorrectDecision
                : PointsRejectVillager;

            int npcPoints = CalcNpcPoints(currentNPC, rejectPoints);
            
            // กันนับซ้ำ: NPC ตัวนี้ถูกตัดสินไปแล้ว → ไม่ทำซ้ำ
            if (CampaignStatsTracker.Instance != null &&
                !CampaignStatsTracker.Instance.Record(
                    currentNPC,
                    isRobber,
                    CampaignStatsTracker.Decision.Rejected,
                    currentDay,
                    npcPoints))
            {
                return;
            }
            
            score += npcPoints;
            
            if (isRobber)
                robberRejected++;
            else
            {
                villagerRejected++;

                // ★ แจ้ง DeathTracker ว่าคนนี้ถูกปฏิเสธ
                //   รอ roll หายสาบสูญตอนเริ่มวันใหม่
                if (DeathTracker.Instance != null && npc.data != null)
                    DeathTracker.Instance.NotifyRejected(npc.data);
            }

            npcProcessedCount++;
        }

        // NPC ตัวนี้ไม่เข้า Camp
        releasedToCamp = false;

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        NPCMovement move = currentNPC.GetComponent<NPCMovement>();
        move.MoveTo(exitPoint.position);

        StartCoroutine(WaitForExitThenAdvanceHour(currentNPC));
    }

    // =========================
    // NPC DESTROY
    // =========================

    private IEnumerator WaitForExitThenAdvanceHour(GameObject npc)
    {
        NPCMovement move = npc.GetComponent<NPCMovement>();

        while (move.IsMoving())
            yield return null;

        // NPC เดินกลับถึง Camp แล้ว
        if (releasedToCamp)
        {
            NPC npcScript = npc.GetComponent<NPC>();

            if (npcScript != null &&
                npcScript.applicant != null &&
                CampManager.Instance != null)
            {
                // Ghost ห้ามทำให้ NPC ตัวจริงกลายเป็นอยู่ Camp
                if (!npcScript.applicant.isGhost)
                {
                    CampManager.Instance.EnterCampToday(
                        npcScript.applicant.displayData
                    );

                    Debug.Log(
                        $"🏕️ {npcScript.applicant.displayData.npcName} " +
                        $"เข้า Camp แล้ว → Runtime ควรเป็น 🟢"
                    );
                }
            }
        }

        Destroy(npc);

        currentNPC = null;
        
        
        // ★ เช็ค BadEnd หลังปล่อย NPC เดินออกจากจุดตรวจ
        //   ถ้าเข้าเงื่อนไข → หยุดเกม ไม่ต้อง AdvanceHour
        if (EndingManager.Instance != null &&
            EndingManager.Instance.CheckBadEnd())
        {
            yield break;
        }

        AdvanceHour();
    }

    // =========================
    // TIME
    // =========================

    private void AdvanceHour()
    {
        currentHour++;

        if (currentHour >= 24)
            currentHour = 0;

        clockManager.SetHour(currentHour);

        // NPC วันนี้หมดแล้ว
        if (npcProcessedCount >= npcPerDay)
        {
            Debug.Log("===== NPC วันนี้หมดแล้ว → จบวัน =====");

            EndGame();
            return;
        }

        spawner.SpawnNextNPC();
    }

    // =========================
    // END
    // =========================

    //อันเก่า
    /* public void EndGame()
    {
        Debug.Log(
            "จบเกม เวลา " +
            currentHour.ToString("00") +
            ":00");
    } */

    public void EndGame()
    {
        Debug.Log("จบวัน คะแนน NPC วันนี้: " + score);

        // วันสุดท้าย → ตัดสินฉากจบ ห้ามขึ้น EndDayUI
        if (EndingManager.Instance != null &&
            EndingManager.Instance.TryResolveFinalDay())
        {
            return;
        }
        
        
        // ★ DailyReport: เก็บรายชื่อคนตายของวันนี้ก่อนแสดงหน้าสรุป
        SnapshotDeadVillagersForReport();
        // คะแนนรายวัน (ชั่วคราว: หักเฉพาะคนตายที่เกิดแล้ว หายสาบสูญยังไม่ roll)
        int earned = score;
        int max = npcProcessedCount * ScoreRankUtility.MaxPerNPC;

        if (CampaignStatsTracker.Instance != null)
            CampaignStatsTracker.Instance.GetDayPreview(currentDay, out earned, out max);

        if (endDayUI != null)
        {
            endDayUI.Show(
                earned,
                max,
                villagerPassed,
                villagerArrested,
                robberPassed,
                robberArrested,
                villagerRejected,
                robberRejected,
                CampManager.Instance != null
                    ? CampManager.Instance.villagersKilledToday
                    : 0
            );
        }
    }

    // =========================
    // NEXT DAY
    // =========================

    public void StartNextDay()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        devSkipDayTriggered = false;
#endif
        
        if (CampManager.Instance != null)
        {
            CampManager.Instance.ResolveGhosts();
        }
        
        // ★ roll หายสาบสูญของคนที่โดนปุ่มแดงไล่ออกเมื่อคืน
        if (DeathTracker.Instance != null)
            DeathTracker.Instance.RollPendingVanish(currentDay);
        
        // ★ ปิดบัญชีคะแนนของวันนี้ (หลังรู้ผลหายสาบสูญแล้ว)
        if (CampaignStatsTracker.Instance != null)
            CampaignStatsTracker.Instance.FinalizeDay(currentDay);
        
        // ★ ตายหมดตอน roll หายสาบสูญ → Bad End ทันที ไม่รอ NPC คนแรกของวันถัดไป
        if (EndingManager.Instance != null &&
            EndingManager.Instance.CheckBadEnd())
        {
            if (endDayUI != null)
                endDayUI.Hide();
            return;
        }
        
        // ---------- จบเกมเมื่อครบ 7 วัน ----------
        if (currentDay >= maxDay)
        {
            Debug.Log("จบเกมครบ 7 วัน");
            // ใส่หน้า Ending UI ตรงนี้ภายหลัง
            return;
        }

        // ---------- เปลี่ยนวัน ----------
        currentDay++;
        if (CampManager.Instance != null)
        {
            CampManager.Instance.StartNewDay(currentDay);
        }

        // รีเซ็ตสถิติของวัน
        npcProcessedCount = 0;
        score = 0;
        villagerPassed = 0;
        villagerArrested = 0;
        robberPassed = 0;
        robberArrested = 0;
        villagerRejected = 0;
        robberRejected = 0;

        if (ChecklistManager.Instance != null)
            ChecklistManager.Instance.ResetAllChecklistScores();

        if (endDayUI != null)
            endDayUI.Hide();
        
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayGameplayBGM();

        // รีเซ็ตเวลา
        currentHour = startHour;
        clockManager.SetHour(currentHour);

        // อัปเดต Day UI
        clockManager.SetDay(currentDay);

        // เปลี่ยนปฏิทิน
        if (calendarManager != null)
            calendarManager.SetDay(currentDay);

        // รีเซ็ต NPC
        currentNPC = null;
        currentState = NPCState.WalkingToCheckpoint;

        if (todayListManager != null)
            todayListManager.ResetForNewDay();

        if (spawner != null)
        {
            spawner.ResetToday();
            spawner.GenerateTodayApplicants();
            BeginNPCFlowOfTheDay();
        }

        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.HideDecisionButtons();

        Debug.Log("===== DAY " + currentDay + " =====");
    }
    
    // =========================
    // MORNING NEWSPAPER
    // =========================

    // เริ่มลำดับ NPC ของวัน: ส่งหนังสือพิมพ์ก่อน แล้วค่อย Spawn NPC
    private void BeginNPCFlowOfTheDay()
    {
        if (spawner == null)
            return;

        if (NewspaperDeliveryManager.Instance != null)
        {
            NewspaperDeliveryManager.Instance.BeginMorningDelivery(
                currentDay,
                OnMorningDeliveryFinished
            );
        }
        else
        {
            // ไม่มีระบบหนังสือพิมพ์ในฉาก → ทำงานแบบเดิม
            spawner.SpawnNextNPC();
        }
    }

    private void OnMorningDeliveryFinished()
    {
        if (spawner != null)
            spawner.SpawnNextNPC();
    }
    
    private void StartPoliceDialog()
    {
        if (currentPolice == null)
        {
            Debug.LogError("ไม่มี Police");
            return;
        }

        NPC npc = currentPolice.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError("Police Prefab ไม่มี NPC.cs");
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError("Police ไม่มี NPCData");
            return;
        }

        Debug.Log("เปิด Dialog ของ " + npc.data.npcName);
        SetPoliceMouthTalking();
        dialogManager.StartDialog(npc.data);
    }

    public void StartPoliceCallDialog()
    {
        SetPoliceMouthTalking();
        dialogManager.StartSimpleDialog(
            "191",
            new string[]
            {
                "coming soon"
            }
        );
    }

    // =========================
    // POLICE CALL
    // =========================

    // เรียกจาก PhoneDialer.Call() หลังกด 191 ถูกต้อง และโชว์ "Calling..." ค้างไว้แล้ว
    public void OnPoliceCalled()
    {
        // กันเรียกซ้ำระหว่างลำดับตำรวจกำลังทำงาน
        if (isPoliceSequenceActive)
            return;

        StartCoroutine(PoliceSequence());
    }

    private IEnumerator PoliceSequence()
    {
        isPoliceSequenceActive = true;

        // 🔥 1. เล่นเสียงสัญญาณเตือนทันที ไม่ต้องรอ NPC เดินออกก่อน
        if (policeSound != null)
        {
            policeSound.Play();
        }

        // 2. NPC ปัจจุบันเดินออกไปก่อน (เดินคู่กับเสียงสัญญาณที่เล่นอยู่แล้ว)
        if (currentNPC != null)
        {
            NPCMovement npcMove = currentNPC.GetComponent<NPCMovement>();
            npcMove.MoveTo(exitPoint.position, 6f);

            while (npcMove.IsMoving())
                yield return null;

            RecordDecision(currentNPC, wasArrested: true);
            Destroy(currentNPC);
            currentNPC = null;
        }

        // 🔥 3. ถ้าเสียงยังเล่นไม่จบ (เผื่อเสียงยาวกว่าระยะเวลาเดิน) ค่อยรอให้จบ
        if (policeSound != null)
        {
            while (policeSound.isPlaying)
                yield return null;
        }

        // 4. เปิดประตูฉุกเฉิน
        if (emergencyManager != null)
            emergencyManager.ForceOpenDoor();

        // 5. Spawn ตำรวจ
        currentPolice = Instantiate(
            policePrefab,
            spawner.spawnPoint.position,
            Quaternion.identity
        );

        NPCMovement policeMove = currentPolice.GetComponent<NPCMovement>();

        // 6. ตำรวจเดินเข้ามากลางจอ
        policeMove.MoveTo(spawnPolice.position);

        while (policeMove.IsMoving())
            yield return null;

        // 7. ตำรวจมาถึงแล้ว → เปิด Dialog
        StartPoliceDialog();

        yield return new WaitUntil(() => !dialogManager.IsDialogOpen());

        DestroyBagAndSlideBack();

        // 8. ตำรวจเดินออก
        policeMove.MoveTo(exitPolicePoint.position);

        while (policeMove.IsMoving())
            yield return null;

        Destroy(currentPolice);
        currentPolice = null;
        
        // ปลด flag ก่อนเสมอ กันเข้า BadEnd แล้ว flag ค้าง
        isPoliceSequenceActive = false;
        
        // ★ เช็ค BadEnd เคสตายจาก emergency ด้วย
        if (EndingManager.Instance != null &&
            EndingManager.Instance.CheckBadEnd())
        {
            yield break;
        }

        // 9. กลับเข้าสู่เกมปกติ
        AdvanceHour();
        
    }
    private void SetPoliceMouthTalking()
    {
        if (currentPolice == null)
            return;

        NPCMouthAnimation mouth =
            currentPolice.GetComponentInChildren<NPCMouthAnimation>();

        if (mouth == null)
        {
            Debug.LogWarning(
                "Police " + currentPolice.name +
                " ไม่มี NPCMouthAnimation"
            );

            return;
        }

        dialogManager.SetTalkingNPC(mouth);
    }

    //โอ๊ค
    // เรียกตอนที่ตัดสินใจ NPC 1 คนเสร็จแล้ว (ปล่อย หรือ จับ)
    private void RecordDecision(GameObject npcObj, bool wasArrested)
    {
        NPC npc = npcObj.GetComponent<NPC>();
        if (npc == null) return;

        bool isRobber =
            (npc.applicant != null && npc.applicant.isGhost) ||
            npc.npcType == NPCType.Special;

        // [ENDING-SCORE] ผลตัดสิน → คะแนนส่วนตัดสินใจ (20) หรือผิดร้ายแรง (NPC ตัวนี้ 0)
        //   ปล่อยชาวบ้าน (ห้องปลอดภัย) 20 | กำจัดผีด้วย Emergency 20 + เช็คลิสต์เต็มอัตโนมัติ
        //   ปล่อยผี / Emergency ใส่ชาวบ้าน / ปล่อยชาวบ้านเข้าห้องที่มีผีแล้ว = ผิดร้ายแรง (0)
        bool severe;
        bool fullChecklist = false;

        if (!wasArrested)
        {
            severe = isRobber || IsVillagerRoomHaunted(npc);
        }
        else
        {
            severe = !isRobber;
            fullChecklist = isRobber;
        }

        int npcPoints = CalcNpcPoints(npcObj, PointsCorrectDecision, severe, fullChecklist);

        // กันนับซ้ำ: NPC ตัวนี้ถูกตัดสินไปแล้ว → ไม่ทำซ้ำ
        if (CampaignStatsTracker.Instance != null &&
            !CampaignStatsTracker.Instance.Record(
                npcObj,
                isRobber,
                wasArrested
                    ? CampaignStatsTracker.Decision.Arrested
                    : CampaignStatsTracker.Decision.Passed,
                currentDay,
                npcPoints))
        {
            return;
        }

        score += npcPoints;

        if (!wasArrested)
        {
            if (isRobber)
            {
                robberPassed++;

                // ★ DailyReport: ผีที่ได้รับอนุญาตให้ผ่าน
                //   (อยู่หลัง Record() ผ่านด่านกันซ้ำแล้วเท่านั้น)
                AddGhostPassedToReport(npc);
            }
            else
                villagerPassed++;
        }
        else
        {
            if (isRobber)
            {
                robberArrested++;
            }
            else
            {
                villagerArrested++;

                // ★ ตายจาก emergency (เรียกหมอผีมาฆ่าคน)
                //   ผ่าน CampManager เพื่อให้ aliveNPC เป็นตายจริง
                //   (CampManager จะบันทึกลง DeathTracker ให้เอง)
                if (npc.data != null)
                {
                    const string detail = "ถูกหมอผีกำจัด";

                    bool killed =
                        CampManager.Instance != null &&
                        CampManager.Instance.KillResident(
                            npc.data, DeathCause.EmergencyKill, detail);

                    // ไม่เจอในค่าย → บันทึกลงสมุดตรง ๆ เหมือนเดิม
                    if (!killed && DeathTracker.Instance != null)
                    {
                        DeathTracker.Instance.RecordDeath(
                            npc.data,
                            DeathCause.EmergencyKill,
                            currentDay,
                            detail
                        );
                    }
                }
            }
        }

        npcProcessedCount++;
    }

    // [ENDING-SCORE] ห้องของชาวบ้านคนนี้มีผีเข้าไปแล้ววันนี้หรือไม่
    private bool IsVillagerRoomHaunted(NPC npc)
    {
        return npc != null &&
               npc.data != null &&
               CampManager.Instance != null &&
               CampManager.Instance.IsGhostInRoomOf(npc.data);
    }

    // [ENDING-SCORE] คะแนน NPC 1 ตัว = ตัดสิน + เช็คลิสต์ (ต่ำสุด 0 สูงสุด 25)
    //   severe = ผิดร้ายแรง → NPC ตัวนี้ได้ 0 ทั้งก้อน (เช็คลิสต์ไม่ช่วย)
    //   fullChecklist = กำจัดผีถูกด้วย Emergency → เช็คลิสต์เต็มอัตโนมัติ
    // ดึงแล้วเคลียร์คะแนน checklist ของ NPC ตัวนั้นทิ้งด้วย
    private int CalcNpcPoints(
        GameObject npcObj,
        int decisionPoints,
        bool severe = false,
        bool fullChecklist = false)
    {
        int checklistPoints = 0;

        if (ChecklistManager.Instance != null)
        {
            checklistPoints = ChecklistManager.Instance.GetChecklistScore(npcObj);

            // ★ DailyReport: เก็บผลรายข้อ "ก่อน" Consume (เก็บค่าที่ผู้เล่นทำจริง)
            SaveChecklistToReport(npcObj, checklistPoints);

            ChecklistManager.Instance.ConsumeChecklistScore(npcObj);
        }
        else
        {
            Debug.LogWarning("ChecklistManager.Instance เป็น null!");
        }

        // Emergency กำจัดผีถูก → เช็คลิสต์เต็มอัตโนมัติ (ได้ไม่น้อยกว่าที่ติ๊กไว้)
        if (fullChecklist)
            checklistPoints = ScoreRankUtility.MaxChecklistPoints;

        checklistPoints = Mathf.Clamp(
            checklistPoints, 0, ScoreRankUtility.MaxChecklistPoints);

        int total = severe
            ? 0
            : Mathf.Clamp(
                checklistPoints + decisionPoints,
                0,
                ScoreRankUtility.MaxPerNPC);

        Debug.Log(
            $"[Score] {npcObj.name} | checklist {checklistPoints} " +
            $"+ ตัดสิน {decisionPoints} | ผิดร้ายแรง={severe} | เช็คลิสต์เต็มอัตโนมัติ={fullChecklist} → {total}"
        );

        return total;
    }

        // =========================
    // DAILY REPORT (snapshot รายวัน)
    // =========================

    // ชื่อที่แสดง: ghostTarget (ตัวตนที่ผีใช้) → ถ้าไม่มี ใช้ displayData → สุดท้ายใช้ npc.data
    private void AddGhostPassedToReport(NPC npc)
    {
        if (npc == null || CampaignStatsTracker.Instance == null)
            return;

        TodayApplicant a = npc.applicant;

        string shownName = null;

        if (a != null && a.ghostTarget != null)
            shownName = a.ghostTarget.npcName;
        else if (a != null && a.displayData != null)
            shownName = a.displayData.npcName;
        else if (npc.data != null)
            shownName = npc.data.npcName;

        CampaignStatsTracker.Instance
            .GetOrCreateDailyReport(currentDay)
            .AddGhostPassed(shownName);

        Debug.Log(
            $"[DailyReport] ผีผ่าน วัน {currentDay} → แสดงชื่อ '{shownName}' | " +
            $"ghostTarget={a?.ghostTarget?.npcName} | displayData={a?.displayData?.npcName}"
        );
    }

    // รายชื่อชาวบ้านที่ตายของวันนี้ (ไม่ซ้ำ) เก็บครั้งเดียวต่อวัน
    // หายสาบสูญยังไม่ roll ตอนนี้ จึงไม่ถูกรวม (ตามที่ตกลงไว้)
    private void SnapshotDeadVillagersForReport()
    {
        if (CampaignStatsTracker.Instance == null || DeathTracker.Instance == null)
            return;

        DailyReport report =
            CampaignStatsTracker.Instance.GetOrCreateDailyReport(currentDay);

        if (report.deadSnapshotTaken)
            return;

        List<string> names = new();

        foreach (DeathRecord r in DeathTracker.Instance.GetRecords())
        {
            if (r == null || r.npcData == null)
                continue;

            if (r.day != currentDay)
                continue;

            if (!names.Contains(r.npcData.npcName))
                names.Add(r.npcData.npcName);
        }

        report.SetDeadVillagers(names);

        Debug.Log(
            $"[DailyReport] วัน {currentDay} | " +
            $"ผีที่ผ่าน: [{string.Join(", ", report.ghostsPassed)}] | " +
            $"ตาย: [{string.Join(", ", report.villagersDead)}]"
        );
    }
    
        // ชื่อที่ผู้เล่นเห็นตอนตรวจ: ผี → ชื่อตัวตนที่ใช้ (ghostTarget) / ชาวบ้าน → ชื่อของตัวเอง
    private string GetShownNpcName(NPC npc)
    {
        if (npc == null)
            return "?";

        TodayApplicant a = npc.applicant;

        if (a != null && a.isGhost && a.ghostTarget != null)
            return a.ghostTarget.npcName;

        if (a != null && a.displayData != null)
            return a.displayData.npcName;

        return npc.data != null ? npc.data.npcName : "?";
    }

    private void SaveChecklistToReport(GameObject npcObj, int checklistPoints)
    {
        if (npcObj == null ||
            CampaignStatsTracker.Instance == null ||
            ChecklistManager.Instance == null)
            return;

        DailyReport report =
            CampaignStatsTracker.Instance.GetOrCreateDailyReport(currentDay);

        int key = npcObj.GetInstanceID();

        bool hasItems = ChecklistManager.Instance.TryGetChecklistItems(
            npcObj, out List<ChecklistItemResult> items);

        // เรียกซ้ำหลังผล Checklist ถูกเคลียร์แล้ว → ห้ามเขียนทับแถวเดิมด้วย "ไม่ได้ส่ง"
        if (!hasItems && report.checklists.Exists(c => c.npcKey == key))
            return;

        report.SetChecklist(new NpcChecklistRecord
        {
            npcKey = key,
            displayName = GetShownNpcName(npcObj.GetComponent<NPC>()),
            submitted = hasItems,
            items = hasItems ? items : new List<ChecklistItemResult>()
        });

        if (hasItems)
        {
            int correct = 0;
            string marks = "";

            foreach (ChecklistItemResult r in items)
            {
                if (r.IsCorrect) correct++;
                marks += r.IsCorrect ? "✓" : "✗";
            }

            Debug.Log(
                $"[DailyReport] Checklist วัน {currentDay} | {GetShownNpcName(npcObj.GetComponent<NPC>())} | " +
                $"{marks} | ถูก {correct} ผิด {items.Count - correct} " +
                $"(คะแนนเช็คลิสต์จริง {checklistPoints})"
            );
        }
        else
        {
            Debug.Log(
                $"[DailyReport] Checklist วัน {currentDay} | " +
                $"{GetShownNpcName(npcObj.GetComponent<NPC>())} | ไม่ได้ส่ง Checklist"
            );
        }
    }
    
    public void TestCampCall()
    {
        if (currentNPC == null)
            return;

        NPC npc = currentNPC.GetComponent<NPC>();

        if (npc == null || npc.applicant == null)
            return;

        List<CampResident> campmates =
            CampManager.Instance.GetRoommates(npc.data);

        Debug.Log("=== คนใน Camp เดียวกัน ===");

        foreach (CampResident mate in campmates)
        {
            Debug.Log(mate.npcData.npcName);
        }
    }
}