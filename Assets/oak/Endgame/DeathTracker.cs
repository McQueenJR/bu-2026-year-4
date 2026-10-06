using UnityEngine;
using System.Collections.Generic;

// =========================
// สาเหตุการตาย
// =========================
public enum DeathCause
{
    GhostTentKill,        // โดนผีฆ่าในเต็นท์ (ResolveGhosts)
    VanishedAfterReject,  // ถูกปุ่มแดงไล่ออกแล้วหายสาบสูญระหว่างคืน
    EmergencyKill         // โดนหมอผีกำจัดจากการกด emergency
}

// =========================
// รายการตาย 1 แถว
// =========================
[System.Serializable]
public class DeathRecord
{
    public NPCData npcData;
    public DeathCause cause;
    public int day;
    public string detail;

    public DeathRecord(NPCData npc, DeathCause cause, int day, string detail)
    {
        this.npcData = npc;
        this.cause = cause;
        this.day = day;
        this.detail = detail;
    }
}

// =========================
// สมุดบันทึกการตาย — ใครตายจากอะไร วันไหน
// ทุกสาเหตุต้องมาบันทึกที่นี่ที่เดียว
// =========================
public class DeathTracker : MonoBehaviour
{
    public static DeathTracker Instance;

    [Header("หายสาบสูญ (ปุ่มแดง)")]
    [Range(0f, 100f)]
    [Tooltip("โอกาส % ที่ชาวบ้านที่ถูกปฏิเสธจะหายสาบสูญระหว่างคืน")]
    public float vanishChance = 50f;

    // สมุดบันทึกการตาย
    private readonly List<DeathRecord> records = new();

    // ชาวบ้านที่ถูกปุ่มแดงไล่ออก รอ roll หายสาบสูญตอนเริ่มวันใหม่
    private readonly List<NPCData> pendingRejected = new();

    void Awake()
    {
        Instance = this;
    }

    // =====================================================
    // ★ ทางเข้าหลัก — บันทึกการตาย (เรียกจากที่นี่ที่เดียวเท่านั้น)
    // =====================================================
    public void RecordDeath(NPCData npc, DeathCause cause, int day, string detail = "")
    {
        if (npc == null)
            return;

        // กันตายซ้ำ (เช็กจากชื่อ เพราะ NPCData ของคนเดียวกันอาจเป็นคนละ instance)
        if (IsDead(npc))
        {
            Debug.LogWarning(
                $"⚠️ {npc.npcName} ถูกบันทึกว่าตายไปแล้ว → ข้ามการบันทึกซ้ำ ({cause})"
            );
            return;
        }

        records.Add(new DeathRecord(npc, cause, day, detail));

        Debug.Log(
            $"💀 [DeathTracker] {npc.npcName} | {cause} | " +
            $"วันที่ {day} | {detail}"
        );
    }

    // =====================================================
    // ปุ่มแดง — แจ้งว่าคนนี้ถูกปฏิเสธ (ยังไม่ตาย รอ roll ตอนกลางคืน)
    // =====================================================
    public void NotifyRejected(NPCData npc)
    {
        if (npc == null)
            return;

        // ตายไปแล้ว → ไม่ต้อง pending
        if (IsDead(npc))
            return;

        if (!pendingRejected.Exists(n => n.npcName == npc.npcName))
            pendingRejected.Add(npc);
    }

    // =====================================================
    // เรียกตอนเริ่มวันใหม่ — roll หายสาบสูญของคนที่ถูกไล่ออกเมื่อคืน
    // =====================================================
    public void RollPendingVanish(int currentDay)
    {
        if (pendingRejected.Count == 0)
            return;

        Debug.Log("===== ROLL หายสาบสูญ (คนถูกปฏิเสธเมื่อคืน) =====");

        foreach (NPCData npc in pendingRejected)
        {
            // เผื่อตายจากสาเหตุอื่นไปแล้วระหว่างคืน
            if (IsDead(npc))
                continue;

            float roll = Random.Range(0f, 100f);

            if (roll < vanishChance)
            {
                RecordDeath(
                    npc,
                    DeathCause.VanishedAfterReject,
                    currentDay,
                    "ถูกปฏิเสธแล้วหายสาบสูญระหว่างคืน"
                );
            }
            else
            {
                Debug.Log(
                    $"🌙 {npc.npcName} รอดจากการถูกไล่ออก " +
                    $"(roll {roll:F1} ≥ {vanishChance})"
                );
            }
        }

        pendingRejected.Clear();
    }

    // =====================================================
    // QUERY — คำนวณจาก list ตอนเรียก ไม่ถือ counter แยก
    // =====================================================

    public int GetCount(DeathCause cause)
    {
        int count = 0;

        foreach (DeathRecord r in records)
        {
            if (r.cause == cause)
                count++;
        }

        return count;
    }

    // ตายจากผีฆ่าในเต็นท์ + หายสาบสูญจากปุ่มแดง (เงื่อนไข BadEnd)
    public int GetGhostPlusVanishCount()
    {
        return GetCount(DeathCause.GhostTentKill) +
               GetCount(DeathCause.VanishedAfterReject);
    }

    public int GetTotalDeaths()
    {
        return records.Count;
    }

    public bool IsDead(NPCData npc)
    {
        if (npc == null)
            return false;

        return records.Exists(
            r => r.npcData != null &&
                 r.npcData.npcName == npc.npcName
        );
    }

    public IReadOnlyList<DeathRecord> GetRecords()
    {
        return records;
    }

    // =====================================================
    // DEBUG ตัวเลขการตายมุมจอ
    // =====================================================
    [Header("Debug")]
    public bool showDebugGUI = true;

    void OnGUI()
    {
        if (!showDebugGUI)
            return;

        GUI.Box(new Rect(10, 720, 300, 110), "DEATH TRACKER");

        GUI.Label(new Rect(20, 745, 280, 22),
            $"ผีฆ่าในเต็นท์ : {GetCount(DeathCause.GhostTentKill)}");
        GUI.Label(new Rect(20, 767, 280, 22),
            $"หายสาบสูญ (แดง) : {GetCount(DeathCause.VanishedAfterReject)}");
        GUI.Label(new Rect(20, 789, 280, 22),
            $"โดน Emergency : {GetCount(DeathCause.EmergencyKill)}");
        GUI.Label(new Rect(20, 811, 280, 22),
            $"รวม : {GetTotalDeaths()}");
    }
}