using UnityEngine;
using System.Collections.Generic;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;

    [Header("Checklist Panel (World GameObject)")]
    public GameObject checklistPanel;

    [Header("Checklist Sounds")]
    public AudioSource checklistAudioSource;
    public AudioClip openChecklistSound;
    public AudioClip submitSound;

    [Header("Answer Toggles (World)")]
    public WorldToggle bagAbnormal;
    public WorldToggle bagNormal;

    public WorldToggle appearanceAbnormal;
    public WorldToggle appearanceNormal;

    public WorldToggle idAbnormal;
    public WorldToggle idNormal;

    public WorldToggle entryDocAbnormal;
    public WorldToggle entryDocNormal;

    [Header("Score")]
    public int checklistScore = 0; // ใช้แสดงผลคะแนนรอบล่าสุดที่ตอบ (ไม่ใช่ตัวสะสมจริง)

    private bool[] playerAnswers = new bool[4];
    private bool[] answered = new bool[4];

    // เก็บคะแนน checklist ล่าสุดที่ "กดส่ง" ของ NPC แต่ละคน
    private Dictionary<GameObject, int> npcChecklistScores = new Dictionary<GameObject, int>();

    // NPC ที่กำลังตรวจ
    private NPC currentNPC;
    
    private class ChecklistAnswerState
    {
        public bool[] answers = new bool[4];
        public bool[] answered = new bool[4];
    }
    private Dictionary<GameObject, ChecklistAnswerState> npcAnswerStates = new Dictionary<GameObject, ChecklistAnswerState>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        // ผูก toggle คู่ Abnormal/Normal ให้ทำงานแบบ radio (เลือกได้ข้อเดียว)
        // + ผูกเข้ากับ SetAnswer เหมือนระบบเดิม
        SetupPair(bagAbnormal, bagNormal, 0);
        SetupPair(appearanceAbnormal, appearanceNormal, 1);
        SetupPair(idAbnormal, idNormal, 2);
        SetupPair(entryDocAbnormal, entryDocNormal, 3);
    }

    private void SetupPair(WorldToggle abnormalToggle, WorldToggle normalToggle, int index)
    {
        abnormalToggle.onValueChanged += value =>
        {
            SetAnswer(index, true, value);
            if (value) normalToggle.SetIsOnWithoutNotify(false);
        };

        normalToggle.onValueChanged += value =>
        {
            SetAnswer(index, false, value);
            if (value) abnormalToggle.SetIsOnWithoutNotify(false);
        };
    }

    // =====================================================
    // รับคำตอบจากผู้เล่น
    // =====================================================
    private void SetAnswer(int index, bool answer, bool isOn)
    {
        if (index < 0 || index >= 4)
            return;

        if (isOn)
        {
            // ผู้เล่นติ๊กเลือกข้อนี้
            playerAnswers[index] = answer;
            answered[index] = true;

            Debug.Log(
                "ข้อ " + index +
                " ผู้เล่นตอบ = " +
                (answer ? "Abnormal" : "Normal")
            );
        }
        else
        {
            // ★ ผู้เล่นกดยกเลิก (คลิกซ้ำอันเดิม) -> ถือว่าข้อนี้ยังไม่ได้ตอบ
            answered[index] = false;

            Debug.Log("ข้อ " + index + " ผู้เล่นยกเลิกคำตอบ");
        }

        // บันทึกคำตอบล่าสุดไว้กับ NPC ตัวปัจจุบัน
        SaveAnswerState();
    }
    private void SaveAnswerState()
    {
        if (currentNPC == null) return;

        if (!npcAnswerStates.TryGetValue(currentNPC.gameObject, out ChecklistAnswerState state))
        {
            state = new ChecklistAnswerState();
            npcAnswerStates[currentNPC.gameObject] = state;
        }

        for (int i = 0; i < 4; i++)
        {
            state.answers[i] = playerAnswers[i];
            state.answered[i] = answered[i];
        }
    }
    

    // =====================================================
    // SCORE
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

    // =====================================================
    // OPEN
    // =====================================================
    public void OpenChecklist()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ไม่มี GameManager");
            return;
        }

        GameObject npcObject = GameManager.Instance.currentNPC;

        if (npcObject == null)
        {
            Debug.LogWarning("GameManager ไม่มี currentNPC");
            return;
        }

        currentNPC = npcObject.GetComponent<NPC>();

        if (currentNPC == null)
        {
            Debug.LogWarning("NPC ไม่มี NPC.cs");
            return;
        }

        if (currentNPC.data == null)
        {
            Debug.LogWarning("NPC ไม่มี NPCData");
            return;
        }

        Debug.Log("เปิด Checklist NPC : " + currentNPC.data.npcName);

        checklistPanel.SetActive(true);

        // ถ้าโปรเจกต์มี DraggableSortOrder อยู่แล้ว (แบบเดียวกับกระเป๋า/บัตร)
        // เรียกอันนี้เพื่อให้สมุด checklist ลอยขึ้นหน้าสุดตอนเปิด
        DraggableSortOrder.NotifyOpened();

        PlaySound(openChecklistSound);
        
        LoadChecklistState();
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
            {
                Debug.Log("ยังตอบไม่ครบ 4 ข้อ ส่งไม่ได้");
                return; // หยุดทั้งฟังก์ชันทันที ไม่บันทึกคะแนนเลย
            }

            bool correctAnswer = currentNPC.data.correctAnswers[i];
            bool playerAnswer = playerAnswers[i];
            if (playerAnswer == correctAnswer)
                scoreThisNPC++;
        }

        // เก็บ "คะแนนล่าสุดที่ส่ง" ของ NPC ตัวนี้ (ส่งซ้ำ = เขียนทับของเดิม)
        npcChecklistScores[currentNPC.gameObject] = scoreThisNPC;
        Debug.Log("บันทึกคะแนน checklist ของ " + currentNPC.data.npcName + " = " + scoreThisNPC + " | key = " + currentNPC.gameObject.GetInstanceID());
        checklistScore = scoreThisNPC;

        Debug.Log(currentNPC.data.npcName + " ส่ง checklist ได้ " + scoreThisNPC + " คะแนน (ล่าสุด)");

        PlaySound(submitSound);
        CloseChecklist();

        if (GreenRedButtonManager.Instance != null)
            GreenRedButtonManager.Instance.ShowDecisionButtons();
    }

    // =====================================================
    // Load
    // =====================================================
    private void LoadChecklistState()
    {
        ChecklistAnswerState state = null;
        if (currentNPC != null)
            npcAnswerStates.TryGetValue(currentNPC.gameObject, out state);

        for (int i = 0; i < 4; i++)
        {
            answered[i] = state != null && state.answered[i];
            playerAnswers[i] = answered[i] ? state.answers[i] : false;
        }

        ApplyToggle(bagAbnormal, bagNormal, 0);
        ApplyToggle(appearanceAbnormal, appearanceNormal, 1);
        ApplyToggle(idAbnormal, idNormal, 2);
        ApplyToggle(entryDocAbnormal, entryDocNormal, 3);
    }

    private void ApplyToggle(WorldToggle abnormalToggle, WorldToggle normalToggle, int index)
    {
        if (!answered[index])
        {
            abnormalToggle.SetIsOnWithoutNotify(false);
            normalToggle.SetIsOnWithoutNotify(false);
            return;
        }

        bool isAbnormal = playerAnswers[index];
        abnormalToggle.SetIsOnWithoutNotify(isAbnormal);
        normalToggle.SetIsOnWithoutNotify(!isAbnormal);
    }

    // =====================================================
    // CLOSE
    // =====================================================
    public void CloseChecklist()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        DraggableSortOrder.NotifyClosed();

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