using UnityEngine;
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
    
    // คะแนนต่อ NPC 1 ตัว (ตามกติกาที่ตกลงไว้ ห้ามเปลี่ยนค่า)
    private const int PointsCorrectDecision = 3;   // ปล่อยชาวบ้าน / กำจัดผี
    private const int PointsReject = 1;            // ปฏิเสธ (ชาวบ้านหรือผี)
    private const int PointsSevereMistake = -3;    // ฆ่าชาวบ้านผ่าน Emergency / ปล่อยผีเข้า

    [Header("End Day UI")]
    public EndDayUI endDayUI;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentDay = 1;
        currentHour = startHour;
        
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

            // คะแนน NPC ตัวนี้ = checklist + ปฏิเสธ (+1)
            int npcPoints = CalcNpcPoints(currentNPC, PointsReject);
            
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

        // คะแนนส่วนตัดสินใจ: ถูก +3 / ผิดร้ายแรง -3
        //   ปล่อยชาวบ้าน +3 | จับผี +3 | ปล่อยผี -3 | จับชาวบ้าน -3
        int decisionPoints;

        if (!wasArrested)
            decisionPoints = isRobber ? PointsSevereMistake : PointsCorrectDecision;
        else
            decisionPoints = isRobber ? PointsCorrectDecision : PointsSevereMistake;

        int npcPoints = CalcNpcPoints(npcObj, decisionPoints);

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
                robberPassed++;
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

    // คะแนน NPC 1 ตัว = checklist + ผลตัดสิน (ต่ำสุด 0 สูงสุด 8)
    // ดึงแล้วเคลียร์คะแนน checklist ของ NPC ตัวนั้นทิ้งด้วย
    private int CalcNpcPoints(GameObject npcObj, int decisionPoints)
    {
        int checklistPoints = 0;

        if (ChecklistManager.Instance != null)
        {
            checklistPoints = ChecklistManager.Instance.GetChecklistScore(npcObj);
            ChecklistManager.Instance.ConsumeChecklistScore(npcObj);
        }
        else
        {
            Debug.LogWarning("ChecklistManager.Instance เป็น null!");
        }

        int total = Mathf.Clamp(
            checklistPoints + decisionPoints,
            0,
            ScoreRankUtility.MaxPerNPC);

        Debug.Log(
            $"[Score] {npcObj.name} | checklist {checklistPoints} " +
            $"+ ตัดสิน {decisionPoints} → {total}"
        );

        return total;
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