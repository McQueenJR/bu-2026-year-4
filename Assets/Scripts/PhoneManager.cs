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
    // ล้างข้อมูลโทรศัพท์
    // ==================================================

    public void ClearPhoneData()
    {
        currentTarget = null;
        answeringNPC = null;

        targetName = "";
        answeringName = "";
        roomCode = "";
        phoneNumber = "";

        targetAlive = false;
        targetHomeToday = false;
        targetHasDocument = false;

        peopleAtHomeCount = 0;

        peopleAtHome.Clear();
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
        phonePanel.SetActive(false);
        telaphone.SetActive(true);

        if (questionPanel != null)
            questionPanel.SetActive(false);

        phoneGreetingActive = false;
        phoneQuestionAnswerActive = false;

        if (phoneDialer != null)
            phoneDialer.Clear();
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
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.dialogManager == null)
        {
            Debug.LogError(
                "📞 ไม่มี DialogManager"
            );

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
        if (answeringNPC == null)
        {
            Debug.LogWarning("📞 ไม่มี NPC รับสาย");
            return;
        }

        phoneGreetingActive = true;
        phoneQuestionAnswerActive = false;

        StartPhoneDialog(
            answeringName,
            new string[]
            {
                "สวัสดีครับ/ค่ะ"
            }
        );
    }

// ==================================================
// Phone Dialog จบ
// ==================================================

    public void PhoneDialogFinished()
    {
        Debug.Log(
            $"📞 Phone Dialog จบ | Target: {targetName}"
        );

        // =========================
        // B พูด Greeting จบ
        // =========================
        if (phoneGreetingActive)
        {
            phoneGreetingActive = false;

            Debug.Log(
                "📞 B พูดสวัสดีจบ → เปิด Question Panel"
            );

            ShowPhoneQuestions();

            return;
        }

        // =========================
        // B ตอบคำถามจบ
        // =========================
        if (phoneQuestionAnswerActive)
        {
            phoneQuestionAnswerActive = false;

            Debug.Log(
                "📞 B ตอบคำถามจบ → กลับไปเลือกคำถาม"
            );

            ShowPhoneQuestions();

            return;
        }

        // =========================
        // ไม่มี Dialog ต่อแล้ว
        // =========================
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
                $"ตอนนี้ในห้องมี {peopleAtHomeCount} คนครับ/ค่ะ"
            }
        );
    }
    
    public void AskDocument()
    {
        if (answeringNPC == null)
            return;

        if (currentTarget == null)
            return;

        questionPanel.SetActive(false);

        phoneQuestionAnswerActive = true;

        bool hasDocument =
            CampManager.Instance.HasDocumentInCamp(
                currentTarget
            );

        Debug.Log(
            $"📞 ถาม: เอกสารของ {targetName} อยู่ไหน? " +
            $"→ hasDocument = {hasDocument}"
        );

        string answer;

        if (hasDocument)
        {
            answer =
                $"เอกสารของ {targetName} อยู่ที่นี่ครับ/ค่ะ";
        }
        else
        {
            answer =
                $"ไม่มีเอกสารของ {targetName} อยู่ที่นี่ครับ/ค่ะ";
        }

        StartPhoneDialog(
            answeringName,
            new string[]
            {
                answer
            }
        );
    }
    
    public void CloseQuestionPanel()
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);

        Debug.Log("📞 ปิด QuestionPanel → วางสาย");

        if (phoneDialer != null)
        {
            phoneDialer.FinishCalling();
        }
    }
    
}