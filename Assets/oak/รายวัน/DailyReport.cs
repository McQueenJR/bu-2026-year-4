using System;
using System.Collections.Generic;

// ผลตรวจ 1 ข้อของ Checklist (เก็บเป็นค่าล้วน ไม่อ้าง GameObject)
[Serializable]
public class ChecklistItemResult
{
    public int itemIndex;                // 0-4 ตามลำดับใน ChecklistManager
    public bool playerAnswerAbnormal;    // ผู้เล่นเลือก "ผิดปกติ" = true
    public bool correctAnswerAbnormal;   // เฉลยที่ระบบใช้คิดคะแนนจริง = true คือผิดปกติ

    // ถูก = ได้คะแนนข้อนั้น (ตรงกับกติกา +1 ใน ChecklistManager.SubmitChecklist)
    public bool IsCorrect => playerAnswerAbnormal == correctAnswerAbnormal;
}

// Checklist ของ NPC 1 คน
[Serializable]
public class NpcChecklistRecord
{
    public int npcKey;                   // instance id ของ NPC ใช้กันบันทึกซ้ำ
    public string displayName;           // ชื่อที่ผู้เล่นเห็นตอนตรวจ
    public bool submitted;               // false = ไม่ได้ส่ง Checklist (เช่นถูกจับผ่านตำรวจ)
    public List<ChecklistItemResult> items = new();
}

// รายงาน 1 วัน (snapshot) เก็บเฉพาะ string/bool/int
[Serializable]
public class DailyReport
{
    public int day;

    // ผีที่ได้รับอนุญาตให้ผ่าน (ชื่อที่ใช้ปลอมตัว หรือชื่อผีเองถ้าไม่ปลอม)
    public List<string> ghostsPassed = new();

    // ชาวบ้านที่ตายของวันนั้น (ไม่ซ้ำ) สร้างครั้งเดียวตอนจบวัน
    public List<string> villagersDead = new();
    public bool deadSnapshotTaken;

    // Checklist ของ NPC แต่ละคน เรียงตามลำดับที่ตัดสิน
    public List<NpcChecklistRecord> checklists = new();

    public DailyReport(int day)
    {
        this.day = day;
    }

    public void AddGhostPassed(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return;

        if (!ghostsPassed.Contains(displayName))
            ghostsPassed.Add(displayName);
    }

    // เรียกได้ครั้งเดียวต่อวัน เรียกซ้ำจะถูกข้าม
    public bool SetDeadVillagers(IEnumerable<string> names)
    {
        if (deadSnapshotTaken || names == null)
            return false;

        foreach (string n in names)
        {
            if (!string.IsNullOrEmpty(n) && !villagersDead.Contains(n))
                villagersDead.Add(n);
        }

        deadSnapshotTaken = true;
        return true;
    }

    // NPC คนเดิม (npcKey เดิม) = เขียนทับแถวเดิม ไม่เพิ่มแถวใหม่
    public void SetChecklist(NpcChecklistRecord record)
    {
        if (record == null)
            return;

        int index = checklists.FindIndex(c => c.npcKey == record.npcKey);

        if (index >= 0)
            checklists[index] = record;
        else
            checklists.Add(record);
    }
}