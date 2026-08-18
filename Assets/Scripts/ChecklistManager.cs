using UnityEngine;

public class ChecklistManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject checklistPanel;

    [Header("Dialog")]
    public DialogManager dialogManager;

    [Header("Score")]
    public int score = 0;

    private NPCData currentNPC;

    // 0 = Bag
    // 1 = Appearance
    // 2 = ID Card
    // 3 = Temple Document


    // =========================
    // เปิด Checklist
    // =========================

    public void OpenChecklist(NPCData npcData)
    {
        if (npcData == null)
        {
            Debug.LogWarning("ไม่มี NPCData");
            return;
        }

        currentNPC = npcData;

        checklistPanel.SetActive(true);

        Debug.Log("เปิด Checklist ของ NPC : " + npcData.npcName);
    }


    // =========================
    // กด Ask
    // =========================

    public void AskQuestion(int index)
    {
        if (currentNPC == null)
        {
            Debug.LogWarning("ยังไม่มี NPC");
            return;
        }

        if (index < 0 || index >= currentNPC.checkQuestions.Length)
        {
            Debug.LogWarning("Index ของคำถามไม่ถูกต้อง");
            return;
        }

        string question = currentNPC.checkQuestions[index];

        // ไม่มีคำถาม
        if (string.IsNullOrEmpty(question))
        {
            Debug.Log("ช่องนี้ไม่มีคำถาม");
            return;
        }

        Debug.Log("ถาม NPC : " + question);

        // ตอนนี้ยังไม่เปิด Dialog
        // เดี๋ยวเราค่อยเชื่อม DialogManager ทีหลัง
    }


    // =========================
    // ตอบคำถาม
    // =========================
    //
    // true  = Normal
    // false = Abnormal
    //

    public void Answer(int index, bool answer)
    {
        if (currentNPC == null)
        {
            Debug.LogWarning("ยังไม่มี NPC");
            return;
        }

        if (index < 0 || index >= currentNPC.correctAnswers.Length)
        {
            Debug.LogWarning("Index ของคำตอบไม่ถูกต้อง");
            return;
        }

        bool correctAnswer = currentNPC.correctAnswers[index];

        if (answer == correctAnswer)
        {
            score++;

            Debug.Log(
                "ตอบถูก! +1 คะแนน | คะแนนปัจจุบัน = " + score
            );
        }
        else
        {
            Debug.Log(
                "ตอบผิด! | คะแนนปัจจุบัน = " + score
            );
        }
    }


    // =========================
    // ปิด Checklist
    // =========================

    public void CloseChecklist()
    {
        checklistPanel.SetActive(false);

        currentNPC = null;
    }
}