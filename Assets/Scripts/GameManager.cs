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
    //public int npcPerDay = 8;

    public int score = 0;
    public int villagerPassed = 0;
    public int villagerArrested = 0;
    public int robberPassed = 0;
    public int robberArrested = 0;

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
        spawner.SpawnNextNPC();
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
        currentState = NPCState.Inspecting;   // ← ตั้งเป็น Inspecting ทันที ไม่รอ
        greenDialogTriggered = false;
    
        StartNPCDialog();                      // ← เปิด dialog ปกติเลย ไม่เช็คสีปุ่มแล้ว
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

     // ★ เช็คว่า NPC ตัวปัจจุบันมีหน้า Display เอกสารของตัวเองมั้ย
     if (currentNPC == null)
         return;

     NPC npc = currentNPC.GetComponent<NPC>();
     if (npc == null || npc.data == null)
         return;

     if (!npc.data.HasTempleDocument)
     {
         Debug.Log($"NPC '{npc.data.npcName}' ไม่มีเอกสารติดตัว → ไม่ spawn ไอคอนเอกสาร");
         return;
     }

     // ผ่านเงื่อนไขแล้ว → spawn ไอคอนเอกสารตัวเดิม (รูปเดียวกันทุกคน) ตามปกติ
     currentDocument = Instantiate(
         templeDocumentPrefab,
         spawnPointDocument.position,
         Quaternion.identity
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

        // ✅ ตัวนี้ไม่เข้าวัด
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
                CampManager.Instance.EnterCampToday(npcScript.applicant.displayData);
                CampManager.Instance.PrintAllCamps();
            }
        }

        Destroy(npc);

        currentNPC = null;

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

        // จบวันเมื่อครบจำนวน NPC ของวันนี้
        if (npcProcessedCount >= npcPerDay)
        {
            EndGame();
            return;
        }

        // Spawn คนต่อไป
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
        Debug.Log("จบวัน คะแนนรวม: " + score);

        if (endDayUI != null)
        {
            endDayUI.Show(score, villagerPassed, villagerArrested, robberPassed, robberArrested);
        }
    }

    // =========================
    // NEXT DAY
    // =========================

    public void StartNextDay()
    {
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
            spawner.SpawnNextNPC();
        }

        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.HideDecisionButtons();

        Debug.Log("===== DAY " + currentDay + " =====");
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

        // 9. กลับเข้าสู่เกมปกติ
        AdvanceHour();

        isPoliceSequenceActive = false;
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

        bool isRobber = npc.npcType == NPCType.Special;

        if (!wasArrested)
        {
            // ปล่อยเข้าไปในหมู่บ้าน
            if (isRobber)
            {
                robberPassed++;
                score = 0;
            }
            else
            {
                villagerPassed++;
                score += 1;
            }
        }
        else
        {
            // เรียกตำรวจจับ
            if (isRobber)
            {
                robberArrested++;
                score += 1;
            }
            else
            {
                villagerArrested++;
                score = 0;
            }
        }
        // ★ บวกคะแนน checklist ล่าสุดที่ส่งของ NPC คนนี้เข้า score รวม
        if (ChecklistManager.Instance != null)
        {
            int checklistPoints = ChecklistManager.Instance.GetChecklistScore(npcObj);
            Debug.Log("ดึงคะแนน checklist ของ npcObj (key = " + npcObj.GetInstanceID() + ") ได้ = " + checklistPoints);
            score += checklistPoints;
            ChecklistManager.Instance.ConsumeChecklistScore(npcObj);
        }
        else
        {
            Debug.LogWarning("ChecklistManager.Instance เป็น null!");
        }

        npcProcessedCount++;
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