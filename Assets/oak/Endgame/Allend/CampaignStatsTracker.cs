using UnityEngine;
using System.Collections.Generic;

// สถิติและคะแนนสะสมทั้งแคมเปญ ไม่รีเซ็ตเมื่อขึ้นวันใหม่
// โหลดซีน ART ใหม่ = component ถูกสร้างใหม่ = แคมเปญใหม่อัตโนมัติ
public class CampaignStatsTracker : MonoBehaviour
{
    public static CampaignStatsTracker Instance;

    public enum Decision { Passed, Arrested, Rejected }

    // ---------- สถิติ 6 รายการ (ชาวบ้านถูกผีฆ่าอ่านจาก DeathTracker) ----------
    public int GhostsEliminated { get; private set; }
    public int GhostsEntered { get; private set; }
    public int GhostsRejected { get; private set; }
    public int VillagersWronglyJudged { get; private set; }
    public int VillagersRejected { get; private set; }

    // ---------- คะแนนแคมเปญ (รวมเฉพาะวันที่ปิดบัญชีแล้ว) ----------
    public int CampaignEarned { get; private set; }
    public int CampaignMax { get; private set; }

    // กันนับซ้ำ: NPC 1 ตัวนับได้ครั้งเดียว
    private readonly HashSet<int> decidedIds = new();

    // คะแนน NPC และจำนวน NPC ที่ตัดสินเสร็จ แยกตามวัน
    private readonly Dictionary<int, int> dayNpcScore = new();
    private readonly Dictionary<int, int> dayJudged = new();

    private readonly HashSet<int> finalizedDays = new();

    // กันหักคนตายซ้ำ (คนเดียวกันหักได้ครั้งเดียว)
    private readonly HashSet<string> deductedNames = new();

    void Awake()
    {
        Instance = this;
    }

    private static int Get(Dictionary<int, int> d, int key)
    {
        return d.TryGetValue(key, out int v) ? v : 0;
    }

    // =====================================================
    // บันทึกผลตัดสิน NPC 1 ตัว
    // คืน false = NPC ตัวนี้ถูกนับไปแล้ว (หรือข้อมูลไม่ครบ) → ผู้เรียกไม่ต้องทำซ้ำ
    // npcPoints = คะแนน NPC ตัวนี้ที่ GameManager คิดแล้ว (0–8)
    // =====================================================
    public bool Record(
        GameObject npcObj,
        bool isGhost,
        Decision decision,
        int day,
        int npcPoints)
    {
        if (npcObj == null)
            return false;

        if (!decidedIds.Add(npcObj.GetInstanceID()))
        {
            Debug.LogWarning($"[CampaignStats] {npcObj.name} ถูกนับไปแล้ว → ข้าม");
            return false;
        }

        switch (decision)
        {
            case Decision.Passed:
                if (isGhost) GhostsEntered++;
                break;   // ชาวบ้านที่ผ่านไม่อยู่ใน 6 สถิติ

            case Decision.Arrested:
                if (isGhost) GhostsEliminated++;
                else VillagersWronglyJudged++;
                break;

            case Decision.Rejected:
                if (isGhost) GhostsRejected++;
                else VillagersRejected++;
                break;
        }

        dayNpcScore[day] = Get(dayNpcScore, day) + npcPoints;
        dayJudged[day] = Get(dayJudged, day) + 1;

        Debug.Log(
            $"[Score] วัน {day} | {npcObj.name} | {decision} " +
            $"{(isGhost ? "(ผี)" : "(ชาวบ้าน)")} → +{npcPoints} " +
            $"| NPC วันนี้ {Get(dayNpcScore, day)}/{Get(dayJudged, day) * ScoreRankUtility.MaxPerNPC}"
        );

        return true;
    }

    // =====================================================
    // ดูคะแนนวันนี้ "ชั่วคราว" (ใช้กับ EndDayUI) — ไม่ปิดบัญชี
    // หักเฉพาะคนตายที่เกิดขึ้นแล้วของวันนั้น
    // =====================================================
    public void GetDayPreview(int day, out int earned, out int max)
    {
        int deaths = CountNightDeaths(day, false);

        earned = Mathf.Max(
            0,
            Get(dayNpcScore, day) - ScoreRankUtility.NightDeathPenalty * deaths);

        max = Get(dayJudged, day) * ScoreRankUtility.MaxPerNPC;
    }

    // =====================================================
    // ปิดบัญชีของวัน: max(0, คะแนน NPC − 2 × คนตายของวันนั้น) แล้วบวกเข้าแคมเปญ
    // เรียกซ้ำกี่ครั้งก็ได้ วันเดียวกันนับครั้งเดียว
    // =====================================================
    public void FinalizeDay(int day)
    {
        if (!finalizedDays.Add(day))
            return;

        int npcScore = Get(dayNpcScore, day);
        int judged = Get(dayJudged, day);
        int deaths = CountNightDeaths(day, true);

        int dayScore = Mathf.Max(
            0,
            npcScore - ScoreRankUtility.NightDeathPenalty * deaths);

        int dayMax = judged * ScoreRankUtility.MaxPerNPC;

        CampaignEarned += dayScore;
        CampaignMax += dayMax;

        Debug.Log(
            $"[Score] ปิดบัญชีวัน {day} | NPC {judged} คน | " +
            $"คะแนน NPC {npcScore}/{dayMax} | คนตาย {deaths} (หัก {deaths * ScoreRankUtility.NightDeathPenalty}) " +
            $"→ ได้ {dayScore} | สะสม {CampaignEarned}/{CampaignMax} " +
            $"= แรงค์ {ScoreRankUtility.GetRank(CampaignEarned, CampaignMax)}"
        );
    }

    // นับชาวบ้านที่ตายของวันนั้นจาก DeathTracker
    // GhostTentKill + VanishedAfterReject เท่านั้น (EmergencyKill ไม่หักซ้ำ เพราะมี -3 อยู่แล้ว)
    private int CountNightDeaths(int day, bool markDeducted)
    {
        if (DeathTracker.Instance == null)
            return 0;

        HashSet<string> seen = new();

        foreach (DeathRecord r in DeathTracker.Instance.GetRecords())
        {
            if (r == null || r.npcData == null)
                continue;

            if (r.day != day)
                continue;

            if (r.cause != DeathCause.GhostTentKill &&
                r.cause != DeathCause.VanishedAfterReject)
                continue;

            string name = r.npcData.npcName;

            if (deductedNames.Contains(name))
                continue;

            seen.Add(name);
        }

        if (markDeducted)
            deductedNames.UnionWith(seen);

        return seen.Count;
    }
}