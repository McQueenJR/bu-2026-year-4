using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private bool greenDialogTriggered = false;  
    public static GameManager Instance;

    public GameObject currentNPC;
    public SpawnManager spawner;

    public Transform enterPoint;
    public Transform exitPoint;

    [Header("Day System")]
    public int currentHour;
    public int startHour = 20;
    public int endHour = 6;

    public ClockManager clockManager;

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

    [Header("Bag")]
    public GameObject bagPrefab;
    public Transform spawnPointBag;
    public Vector3 bagScale = Vector3.one;
    public Vector3 bagRotation = Vector3.zero;   // ใส่เป็นองศา (Euler angles)
    public SlidingPanel windowPanel;

    private GameObject currentBag;

    [Header("ID Card")]
    public GameObject idCardPrefab;        // บัตรคนทั่วไป
    public GameObject monkIdCardPrefab;    // บัตรพระ
    public Transform spawnPointIDCard;
    public Vector3 idCardScale = Vector3.one;
    public Vector3 idCardRotation = Vector3.zero;

    private GameObject currentIDCard;

    // =========================
    // SPAWN SOUNDS
    // =========================
    [Header("Spawn Sounds")]
    public AudioSource spawnAudioSource;
    public AudioClip bagAndCardSpawnSound;

    [Header("Police Call")]
    public GameObject policePrefab;
    public Transform spawnPolice;
    public Transform exitPolicePoint;
    public AudioSource policeSound;
    public bool isPoliceSequenceActive = false;

    private GameObject currentPolice;

    [Header("Day Stats")]
    public int npcProcessedCount = 0;
    public int npcPerDay = 8;

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
        currentHour = startHour;

        clockManager.SetHour(currentHour);

        spawner.SpawnNPC();
    }

    // =========================
    // BUTTON
    // =========================

  

    public void GreenDialogFinished()
    {
        if (currentNPC == null)
            return;
        if (currentState != NPCState.Inspecting) return;   // ← เพิ่มบรรทัดนี้

        Debug.Log("Green Dialog จบ → ปล่อย NPC");

        ReleaseCurrentNPC();
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

        windowPanel.SlideOut(() =>
        {
            SpawnBag();
        });
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

    private void SetCurrentNPCMouthTalking()
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

    private void SpawnBag()
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

    private void DestroyBagAndSlideBack()
    {
        if (currentBag != null)
        {
            Destroy(currentBag);
            currentBag = null;
        }

        if (currentIDCard != null)
        {
            Destroy(currentIDCard);
            currentIDCard = null;
        }

        if (windowPanel != null)
        {
            windowPanel.SlideBack();
        }
    }

    // =========================
    // RELEASE NPC
    // =========================

    public   void ReleaseCurrentNPC()
    {
        if (currentNPC == null)
            return;
        if (currentState == NPCState.Leaving) return;  
        RecordDecision(currentNPC, wasArrested: false);   // <-- โอ๊ตเพิ่มบรรทัดนี้
        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        NPCMovement move =
            currentNPC.GetComponent<NPCMovement>();

        move.MoveTo(enterPoint.position);

        StartCoroutine(
            WaitForExitThenAdvanceHour(currentNPC));
    }

    // =========================
    // REJECT NPC
    // =========================

    public   void RejectCurrentNPC()
    {
        if (currentNPC == null)
            return;

        currentState = NPCState.Leaving;

        DestroyBagAndSlideBack();

        NPCMovement move =
            currentNPC.GetComponent<NPCMovement>();

        move.MoveTo(exitPoint.position);

        StartCoroutine(
            WaitForExitThenAdvanceHour(currentNPC));
    }

    // =========================
    // NPC DESTROY
    // =========================

    private IEnumerator WaitForExitThenAdvanceHour(GameObject npc)
    {
        NPCMovement move =
            npc.GetComponent<NPCMovement>();

        while (move.IsMoving())
            yield return null;

        Destroy(npc);

        currentNPC = null;

        // เวลา +1 ชั่วโมง
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

        /*Debug.Log(
            "เวลา : " +
            currentHour.ToString("00") +
            ":00"); */

        // จบวันเมื่อครบ 8 คน (แทนที่จะดูแค่ currentHour == endHour)
        if (npcProcessedCount >= npcPerDay)
        {
            EndGame();
            return;
        }
        /*if (currentHour == endHour)
        {
            EndGame();
            return;
        }*/


        // Spawn NPC คนใหม่
        spawner.SpawnNPC();
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
        // รีเซ็ตสถิติ
        npcProcessedCount = 0;
        score = 0;
        villagerPassed = 0;
        villagerArrested = 0;
        robberPassed = 0;
        robberArrested = 0;
        
      // ★ เคลียร์คะแนน checklist ของวันเก่าทั้งหมด ไม่ให้ปนกับวันใหม่
        if (ChecklistManager.Instance != null)
            ChecklistManager.Instance.ResetAllChecklistScores();
        
        // ปิด UI สรุปผล
        if (endDayUI != null)
            endDayUI.Hide();

        // รีเซ็ตเวลากลับไปเริ่มต้น
        currentHour = startHour;
        clockManager.SetHour(currentHour);

        // เคลียร์ NPC ค้าง (กันเหนียว เผื่อมี object หลงเหลือ)
        currentNPC = null;
        currentState = NPCState.WalkingToCheckpoint;

        spawner.ResetHistory();   // เพิ่มบรรทัดนี้
        // เริ่ม spawn คนแรกของวันใหม่
        spawner.SpawnNPC();
        
        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.HideDecisionButtons();
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


        // 1. NPC ปัจจุบันออกไปก่อน
        if (currentNPC != null)
        {
            
            NPCMovement npcMove =
                currentNPC.GetComponent<NPCMovement>();

            npcMove.MoveTo(exitPoint.position, 6f);

            while (npcMove.IsMoving())
                yield return null;

            RecordDecision(currentNPC, wasArrested: true);

            Destroy(currentNPC);
            currentNPC = null;
        }

        // 2. เล่นเสียงสัญญาณเตือน
        if (policeSound != null)
        {
            policeSound.Play();

            // รอจนเสียงจบ
            while (policeSound.isPlaying)
                yield return null;
        }

        // 3. เปิดประตูฉุกเฉิน
        if (emergencyManager != null)
            emergencyManager.ForceOpenDoor();

        // 4. Spawn ตำรวจ
        currentPolice = Instantiate(
            policePrefab,
            spawner.spawnPoint.position,
            Quaternion.identity
        );

        NPCMovement policeMove =
            currentPolice.GetComponent<NPCMovement>();

        // 5. ตำรวจเดินเข้ามากลางจอ
        policeMove.MoveTo(spawnPolice.position);

        while (policeMove.IsMoving())
            yield return null;

        // 6. ตำรวจมาถึงแล้ว → เปิด Dialog
        StartPoliceDialog();

        // รอจน Dialog จบ
        yield return new WaitUntil(() =>
            !dialogManager.IsDialogOpen()
        );
        // ตำรวจเดินออกแล้ว → ค่อยลบกระเป๋า/บัตรออกจากโต๊ะ
        DestroyBagAndSlideBack();

        // 7. ตำรวจเดินออก
        policeMove.MoveTo(exitPolicePoint.position);

        while (policeMove.IsMoving())
            yield return null;

        Destroy(currentPolice);
        currentPolice = null;
        

        // 8. กลับเข้าสู่เกมปกติ
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

        bool isRobber = npc.npcType == NPCType.Robber;

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
}