using UnityEngine;
using System.Collections.Generic;

public class PhoneManager : MonoBehaviour
{
    public GameObject phonePanel;
    public GameObject telaphone;
    public PhoneDialer phoneDialer;

    [Header("Camp Phone Data")]
    public NPCData currentTarget;
    public NPCData answeringNPC;

    public string targetName;
    public string answeringName;
    public string roomCode;
    public string phoneNumber;

    public bool targetAlive;
    public bool targetHomeToday;
    public bool targetHasDocument;

    public int peopleAtHomeCount;
    public CampRoom calledRoom;
    public List<NPCData> peopleAtHome =
        new List<NPCData>();
    [Header("Phone Question UI")]
    public GameObject questionPanel;
    private bool phoneGreetingActive = false;
    private bool phoneQuestionAnswerActive = false;
    
    
    public static PhoneManager Instance;


    // ==================================================
    // เปิดโทรศัพท์
    // ==================================================
    private void Awake()
    {
        Instance = this;
    }
    
    public void OpenPhone()
    {
        // กันกดระหว่างถือแว่นขยาย / ไม้ขีดไฟ
        if (MagnifyingGlass.Instance != null &&
            MagnifyingGlass.Instance.IsHolding)
            return;

        if (Matchbox.Instance != null &&
            Matchbox.Instance.IsHolding)
            return;

        // กันกดระหว่างมี dialog เปิดอยู่ / กำลังเรียกตำรวจ
        // / NPC ยังไม่ถึงจุดตรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log(
                    "กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้"
                );

                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log(
                    "มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้"
                );

                return;
            }

            if (GameManager.Instance.currentState !=
                GameManager.NPCState.Inspecting)
            {
                Debug.Log(
                    "NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ " +
                    "กดไม่ได้ตอนนี้"
                );

                return;
            }
        }

        telaphone.SetActive(false);
        phonePanel.SetActive(true);
    }


    // ==================================================
    // เริ่มตรวจสอบข้อมูล Camp ของ NPC
    // ==================================================

    public void StartPhoneCheck(NPCData target)
    {
        ClearPhoneData();

        if (target == null)
        {
            Debug.LogWarning(
                "📞 ไม่มี NPC เป้าหมาย"
            );

            return;
        }

        if (CampManager.Instance == null)
        {
            Debug.LogError(
                "📞 ไม่มี CampManager"
            );

            return;
        }

        currentTarget = target;

        // ------------------------------------------
        // ข้อมูล Target
        // ------------------------------------------

        targetName = target.npcName;

        targetAlive =
            CampManager.Instance.IsAlive(target);

        targetHomeToday =
            CampManager.Instance.IsHomeToday(target);


// ------------------------------------------
// เอกสารของ NPC
// ------------------------------------------

        targetHasDocument = true;

        if (GameManager.Instance != null &&
            GameManager.Instance.currentNPC != null)
        {
            NPC npc =
                GameManager.Instance.currentNPC.GetComponent<NPC>();

            if (npc != null &&
                npc.applicant != null)
            {
                targetHasDocument =
                    npc.applicant.hasTempleDocument;
            }
        }


        // ------------------------------------------
        // หา Room
        // ------------------------------------------

        CampRoom room =
            CampManager.Instance.GetRoomOfNPC(target);

        if (room == null)
        {
            Debug.LogWarning(
                $"📞 หา Room ของ {targetName} ไม่เจอ"
            );

            return;
        }

        roomCode = room.roomCode;
        phoneNumber = room.phoneNumber;


        // ------------------------------------------
        // คนที่อยู่ในห้องวันนี้
        // ------------------------------------------

        List<CampResident> residents =
            CampManager.Instance.GetResidentsAtHome(
                phoneNumber
            );

        foreach (CampResident resident in residents)
        {
            if (resident == null)
                continue;

            if (resident.npcData == null)
                continue;

            peopleAtHome.Add(
                resident.npcData
            );
        }

        peopleAtHomeCount =
            peopleAtHome.Count;


        // ------------------------------------------
        // คนรับสาย
        // ------------------------------------------

        CampResident answeringResident =
            CampManager.Instance.GetAnsweringResident(
                phoneNumber
            );

        if (answeringResident != null &&
            answeringResident.npcData != null)
        {
            answeringNPC =
                answeringResident.npcData;

            answeringName =
                answeringNPC.npcName;
        }


        // ------------------------------------------
        // Debug
        // ------------------------------------------

        Debug.Log(
            $"📞 ===== PHONE CHECK =====\n" +
            $"Target       : {targetName}\n" +
            $"Room         : {roomCode}\n" +
            $"Phone        : {phoneNumber}\n" +
            $"Alive        : {targetAlive}\n" +
            $"Home Today   : {targetHomeToday}\n" +
            $"Document     : {targetHasDocument}\n" +
            $"People Home  : {peopleAtHomeCount}\n" +
            $"Answering    : {answeringName}"
        );

        foreach (NPCData npc in peopleAtHome)
        {
            Debug.Log(
                $"📞 อยู่ในห้องวันนี้: {npc.npcName}"
            );
        }
    }

    
// ==================================================
// รีเฟรชข้อมูล Camp สำหรับโทรศัพท์
// ==================================================
    public void RefreshPhoneCampData()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("📞 ไม่มี Target สำหรับรีเฟรชข้อมูล");
            return;
        }

        if (CampManager.Instance == null)
        {
            Debug.LogError("📞 ไม่มี CampManager");
            return;
        }

        // ดึงสถานะล่าสุดของ Target
        targetAlive = CampManager.Instance.IsAlive(currentTarget);
        targetHomeToday = CampManager.Instance.IsHomeToday(currentTarget);

        // ดึงข้อมูลห้องล่าสุด
        CampRoom room = CampManager.Instance.GetRoomOfNPC(currentTarget);

        if (room == null)
        {
            Debug.LogWarning($"📞 ไม่พบห้องของ {targetName}");
            return;
        }

        roomCode = room.roomCode;
        phoneNumber = room.phoneNumber;

        // ล้างรายชื่อเก่าก่อนดึงข้อมูลใหม่
        peopleAtHome.Clear();
        answeringNPC = null;
        answeringName = "";

        // ดึงคนที่อยู่บ้านในเวลาปัจจุบัน
        List<CampResident> residents =
            CampManager.Instance.GetResidentsAtHome(phoneNumber);

        foreach (CampResident resident in residents)
        {
            if (resident == null || resident.npcData == null)
                continue;

            peopleAtHome.Add(resident.npcData);
        }

        peopleAtHomeCount = peopleAtHome.Count;

        // เลือกคนรับสายจากสถานะล่าสุด
        CampResident answeringResident =
            CampManager.Instance.GetAnsweringResident(phoneNumber);

        if (answeringResident != null &&
            answeringResident.npcData != null)
        {
            answeringNPC = answeringResident.npcData;
            answeringName = answeringNPC.npcName;
        }

        Debug.Log(
            $"📞 REFRESH CAMP | ห้อง: {roomCode} | " +
            $"คนอยู่บ้าน: {peopleAtHomeCount} | " +
            $"คนรับสาย: {answeringName}"
        );
    }
    
    // ==================================================
    // ล้างข้อมูลโทรศัพท์
    // ==================================================

    
    public void ClearPhoneData()
    {
        currentTarget = null;
        answeringNPC = null;
        calledRoom = null;

        targetName = "";
        answeringName = "";
        roomCode = "";
        phoneNumber = "";

        targetAlive = false;
        targetHomeToday = false;
        targetHasDocument = false;

        peopleAtHomeCount = 0;
        peopleAtHome.Clear();

        phoneGreetingActive = false;
        phoneQuestionAnswerActive = false;
    }


    
    
    public void RefreshPhoneCampDataForRoom(CampRoom room)
    {
        // ล้างข้อมูลเก่าก่อน
        peopleAtHome.Clear();
        answeringNPC = null;
        answeringName = "";
        peopleAtHomeCount = 0;

        if (room == null)
        {
            calledRoom = null;
            roomCode = "";
            phoneNumber = "";

            Debug.LogWarning("📞 CampRoom is null");
            return;
        }

        if (CampManager.Instance == null)
        {
            Debug.LogError("📞 ไม่มี CampManager");
            return;
        }

        // ใช้ห้องที่ผู้เล่นโทรหา
        calledRoom = room;
        roomCode = room.roomCode;
        phoneNumber = room.phoneNumber;

        List<CampResident> residents =
            CampManager.Instance.GetResidentsAtHome(phoneNumber);

        if (residents != null)
        {
            foreach (CampResident resident in residents)
            {
                if (resident == null || resident.npcData == null)
                    continue;

                peopleAtHome.Add(resident.npcData);
            }
        }

        peopleAtHomeCount = peopleAtHome.Count;

        CampResident answer =
            CampManager.Instance.GetAnsweringResident(phoneNumber);

        if (answer != null && answer.npcData != null)
        {
            answeringNPC = answer.npcData;
            answeringName = answeringNPC.npcName;
        }

        Debug.Log(
            $"📞 Room={roomCode}, Phone={phoneNumber}, " +
            $"People={peopleAtHomeCount}, Answering={answeringName}"
        );
    }




    // ==================================================
    // ปิดโทรศัพท์
    // ==================================================

    public void ClosePhone()
    {
        if (phoneDialer != null &&
            phoneDialer.IsCalling)
        {
            Debug.Log(
                "กำลังโทรอยู่ ผู้เล่นปิดโทรศัพท์เองไม่ได้ตอนนี้"
            );

            return;
        }

        ForceClosePhone();
    }


    // ==================================================
    // ปิดแบบบังคับจากระบบ
    // ==================================================

    public void ForceClosePhone()
    {
        if (phonePanel != null)
            phonePanel.SetActive(false);

        if (telaphone != null)
            telaphone.SetActive(true);

        if (questionPanel != null)
            questionPanel.SetActive(false);

        phoneGreetingActive = false;
        phoneQuestionAnswerActive = false;

        Debug.Log("📞 ปิดโทรศัพท์เรียบร้อย");
    }

    // ==================================================
    // Helper สำหรับ Dialog
    // ==================================================

    public string GetTargetName()
    {
        return targetName;
    }

    public string GetAnsweringName()
    {
        return answeringName;
    }

    public int GetPeopleAtHomeCount()
    {
        return peopleAtHomeCount;
    }

    public bool IsTargetHome()
    {
        return targetHomeToday;
    }

    public bool HasAnsweringNPC()
    {
        return answeringNPC != null;
    }
    // ==================================================
// เริ่ม Phone Dialog
// ==================================================

    public void StartPhoneDialog(
        string speaker,
        string[] messages
    )
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.dialogManager == null)
        {
            Debug.LogError(
                "📞 ไม่มี GameManager หรือ DialogManager → จบการโทร"
            );

            if (phoneDialer != null)
            {
                phoneDialer.FinishCalling();
            }
            else
            {
                ForceClosePhone();
            }

            return;
        }

        GameManager.Instance.dialogManager.StartPhoneDialog(
            speaker,
            messages
        );
    }
// ==================================================
// Phone Greeting
// ==================================================


   
   
    public void StartPhoneGreeting()
    {
        if (calledRoom == null)
        {
            ForceClosePhone();
            return;
        }

        RefreshPhoneCampDataForRoom(calledRoom);

        if (answeringNPC == null)
        {
            if (phoneDialer != null)
                phoneDialer.FinishCalling();
            else
                ForceClosePhone();

            return;
        }

        phoneGreetingActive = true;
        phoneQuestionAnswerActive = false;

        StartPhoneDialog(
            answeringName,
            new string[]
            {
                $"สวัสดี นี่ {answeringName} "
            }
        );
    }


    

// ==================================================
// Phone Dialog จบ
// ==================================================

    public void PhoneDialogFinished()
    {
        if (phoneGreetingActive)
        {
            phoneGreetingActive = false;
            ShowPhoneQuestions();
            return;
        }

        if (phoneQuestionAnswerActive)
        {
            phoneQuestionAnswerActive = false;
            ShowPhoneQuestions();
            return;
        }

        if (phoneDialer != null)
            phoneDialer.FinishCalling();
    }
    
    public void ShowPhoneQuestions()
    {
        if (questionPanel == null)
        {
            Debug.LogError(
                "📞 ไม่ได้ใส่ QuestionPanel ใน PhoneManager"
            );

            return;
        }

        questionPanel.SetActive(true);

        Debug.Log(
            "📞 QuestionPanel เปิดแล้ว"
        );
    }
    
    
    
    public void AskPeopleCount()
    {
        if (answeringNPC == null)
            return;

        questionPanel.SetActive(false);

        phoneQuestionAnswerActive = true;

        Debug.Log(
            $"📞 ถาม: ในห้องมีคนกี่คน? → {peopleAtHomeCount} คน"
        );

        StartPhoneDialog(
            answeringName,
            new string[]
            {
                $"ตอนนี้ในห้องมี {peopleAtHomeCount} คน"
            }
        );
    }
    
        public void AskDocument()
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);

        phoneQuestionAnswerActive = true;

        bool hasDocument = IsCurrentNPCDocumentInCalledRoom();

        string answer = hasDocument
            ? "มีเอกสารอยู่ในห้อง"
            : "ไม่มีเอกสารอยู่ในห้อง";

        StartPhoneDialog(
            answeringName,
            new string[] { answer }
        );
    }

    // เอกสารของ NPC ที่ยืนหน้าโต๊ะ อยู่ในห้องที่โทรไปหรือไม่
    private bool IsCurrentNPCDocumentInCalledRoom()
    {
        NPC npc = null;

        if (GameManager.Instance != null &&
            GameManager.Instance.currentNPC != null)
        {
            npc = GameManager.Instance.currentNPC.GetComponent<NPC>();
        }

        // หา NPC หน้าโต๊ะไม่เจอ → ใช้ตรรกะเดิมทุกอย่าง
        if (npc == null || npc.data == null || npc.applicant == null)
            return HasAnyDocumentInCalledRoom_Legacy();

        TodayApplicant a = npc.applicant;

        // ถือเอกสารมาด้วย = เอกสารไม่ได้อยู่ในห้อง
        if (a.hasTempleDocument)
        {
            Debug.Log("📞 NPC ถือเอกสารมาด้วย → ไม่มีเอกสารในห้อง");
            return false;
        }

        if (calledRoom == null)
            return false;

        // Ghost: เอกสารเป็นของตัวจริงที่ปลอมเป็น (ghostTarget)
        NPCData owner = (a.isGhost && a.ghostTarget != null)
            ? a.ghostTarget
            : npc.data;

        foreach (CampResident resident in calledRoom.residents)
        {
            if (resident == null || resident.npcData == null)
                continue;

            if (resident.npcData == owner ||
                resident.npcData.npcName == owner.npcName)
            {
                Debug.Log("📞 เอกสารของ " + owner.npcName + " อยู่ในห้อง " + roomCode);
                return true;
            }
        }

        Debug.Log("📞 ห้อง " + roomCode + " ไม่ใช่ห้องของ " + owner.npcName);
        return false;
    }

    // ตรรกะเดิมของ AskDocument (ใช้เป็น fallback เท่านั้น)
    private bool HasAnyDocumentInCalledRoom_Legacy()
    {
        if (calledRoom == null)
            return false;

        foreach (CampResident resident in calledRoom.residents)
        {
            if (resident == null || resident.npcData == null)
                continue;

            if (CampManager.Instance != null &&
                CampManager.Instance.HasDocumentInCamp(resident.npcData))
            {
                return true;
            }
        }

        return false;
    }
    
    public void CloseQuestionPanel()
    {
        Debug.LogError("TEST 1: CloseQuestionPanel ถูกเรียกแล้ว");

        if (questionPanel != null)
            questionPanel.SetActive(false);

        if (phoneDialer != null)
        {
            Debug.LogError(
                "TEST 2: PhoneDialer พบแล้ว | IsCalling = "
                + phoneDialer.IsCalling
            );

            phoneDialer.FinishCalling();
        }
        else
        {
            Debug.LogError("TEST 2: phoneDialer เป็น NULL");
        }
    }
    
    
}