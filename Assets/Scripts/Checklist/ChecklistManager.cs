using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;

    [Header("Checklist Panel")]
    public GameObject checklistPanel;
    
    [Header("Blocker")]
    public GameObject blocker;

    [Header("Checklist Sounds")]
    public AudioSource checklistAudioSource;
    public AudioClip openChecklistSound;
    public AudioClip submitSound;
    
    [Header("Answer Toggles")]
    public Toggle bagAbnormal;
    public Toggle bagNormal;

    public Toggle appearanceAbnormal;
    public Toggle appearanceNormal;

    public Toggle idAbnormal;
    public Toggle idNormal;

    public Toggle entryDocAbnormal;
    public Toggle entryDocNormal;
    
    [Header("Score")]
    public int checklistScore = 0; // ใช้แสดงผลคะแนนรอบล่าสุดที่ตอบ (ไม่ใช่ตัวสะสมจริงแล้ว)
    
    private bool[] playerAnswers = new bool[4];

    // เก็บว่าผู้เล่นตอบข้อไหนแล้ว
    private bool[] answered = new bool[4];

// เก็บคะแนน checklist ล่าสุดที่ "กดส่ง" ของ NPC แต่ละคน
    private Dictionary<GameObject, int> npcChecklistScores = new Dictionary<GameObject, int>();

    // NPC ที่กำลังตรวจ
    private NPC currentNPC;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false);
        
        if (blocker != null)                    // ← เพิ่มใหม่
            blocker.SetActive(false);

        
        bagAbnormal.onValueChanged.AddListener(
            value => SetAnswer(0, true, value)
        );
        bagNormal.onValueChanged.AddListener(
            value => SetAnswer(0, false, value)
        );

        appearanceAbnormal.onValueChanged.AddListener(
            value => SetAnswer(1, true, value)
        );
        appearanceNormal.onValueChanged.AddListener(
            value => SetAnswer(1, false, value)
        );

        idAbnormal.onValueChanged.AddListener(
            value => SetAnswer(2, true, value)
        );
        idNormal.onValueChanged.AddListener(
            value => SetAnswer(2, false, value)
        );

        entryDocAbnormal.onValueChanged.AddListener(
            value => SetAnswer(3, true, value)
        );
        entryDocNormal.onValueChanged.AddListener(
            value => SetAnswer(3, false, value)
        );
        
    }
    // =====================================================
// รับคำตอบจากผู้เล่น (หน้า 2)
// =====================================================

    private void SetAnswer(int index, bool answer, bool isOn)
    {
        if (!isOn)
            return;

        if (index < 0 || index >= 4)
            return;

        playerAnswers[index] = answer;
        answered[index] = true;

        Debug.Log(
            "ข้อ " + index +
            " ผู้เล่นตอบ = " +
            (answer ? "Abnormal" : "Normal")
        );
    }
    
    


    // =====================================================
    // OPEN
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
    
    public void OpenChecklist()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ไม่มี GameManager");
            return;
        }

        GameObject npcObject =
            GameManager.Instance.currentNPC;

        if (npcObject == null)
        {
            Debug.LogWarning(
                "GameManager ไม่มี currentNPC"
            );

            return;
        }

        currentNPC =
            npcObject.GetComponent<NPC>();

        if (currentNPC == null)
        {
            Debug.LogWarning(
                "NPC ไม่มี NPC.cs"
            );

            return;
        }

        if (currentNPC.data == null)
        {
            Debug.LogWarning(
                "NPC ไม่มี NPCData"
            );

            return;
        }

        Debug.Log(
            "เปิด Checklist NPC : " +
            currentNPC.data.npcName
        );

        checklistPanel.SetActive(true);

        if (blocker != null)              // ← เพิ่ม
            blocker.SetActive(true);

        PlaySound(openChecklistSound);    // ← เพิ่ม

        // ไม่มี ShowPage1() แล้ว เพราะไม่มีระบบหน้า 1/2 อีกต่อไป
        ResetChecklist();
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
                return;                // ← หยุดทั้งฟังก์ชันทันที ไม่บันทึกคะแนนเลย
            }

            bool correctAnswer = currentNPC.data.correctAnswers[i];
            bool playerAnswer = playerAnswers[i];
            if (playerAnswer == correctAnswer)
                scoreThisNPC++;
        }

        // เก็บ "คะแนนล่าสุดที่ส่ง" ของ NPC ตัวนี้ (ส่งซ้ำ = เขียนทับของเดิม)
        npcChecklistScores[currentNPC.gameObject] = scoreThisNPC;
        Debug.Log("บันทึกคะแนน checklist ของ " + currentNPC.data.npcName + " = " + scoreThisNPC + " | key = " + currentNPC.gameObject.GetInstanceID());
        checklistScore = scoreThisNPC; // ไว้โชว์ผล/debug

        Debug.Log(currentNPC.data.npcName + " ส่ง checklist ได้ " + scoreThisNPC + " คะแนน (ล่าสุด)");
        
        PlaySound(submitSound);     
        CloseChecklist();
        
        if (GreenRedButtonManager.Instance != null)  
            GreenRedButtonManager.Instance.ShowDecisionButtons();
    }


    // =====================================================
    // RESET
    // =====================================================

    private void ResetChecklist()
    {
        for (int i = 0; i < 4; i++)
        {

            // ★ เพิ่ม 2 บรรทัดนี้ - เคลียร์คำตอบรอบก่อนหน้า
            playerAnswers[i] = false;
            answered[i] = false;
        }
        
        // ★ เพิ่มเคลียร์ toggle หน้า 2
        bagAbnormal.SetIsOnWithoutNotify(false);
        bagNormal.SetIsOnWithoutNotify(false);
        appearanceAbnormal.SetIsOnWithoutNotify(false);
        appearanceNormal.SetIsOnWithoutNotify(false);
        idAbnormal.SetIsOnWithoutNotify(false);
        idNormal.SetIsOnWithoutNotify(false);
        entryDocAbnormal.SetIsOnWithoutNotify(false);
        entryDocNormal.SetIsOnWithoutNotify(false);
    }


    // =====================================================
    // CLOSE
    // =====================================================

    public void CloseChecklist()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false);
        
        if (blocker != null)            
            blocker.SetActive(false);
        

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