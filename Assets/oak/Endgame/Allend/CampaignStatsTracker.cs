using UnityEngine;
using System.Collections.Generic;

// สถิติสะสมทั้งแคมเปญ ไม่รีเซ็ตเมื่อขึ้นวันใหม่
// โหลดซีน ART ใหม่ = component ถูกสร้างใหม่ = แคมเปญใหม่อัตโนมัติ
public class CampaignStatsTracker : MonoBehaviour
{
    public static CampaignStatsTracker Instance;

    public enum Decision { Passed, Arrested, Rejected }

    public int GhostsEliminated { get; private set; }
    public int GhostsEntered { get; private set; }
    public int GhostsRejected { get; private set; }
    public int VillagersWronglyJudged { get; private set; }
    public int VillagersRejected { get; private set; }

    // กันนับซ้ำ: NPC 1 ตัวนับได้ครั้งเดียว
    private readonly HashSet<int> decidedIds = new();

    void Awake()
    {
        Instance = this;
    }

    public void Record(GameObject npcObj, bool isGhost, Decision decision)
    {
        if (npcObj == null)
            return;

        if (!decidedIds.Add(npcObj.GetInstanceID()))
        {
            Debug.LogWarning($"[CampaignStats] {npcObj.name} ถูกนับไปแล้ว → ข้าม");
            return;
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
    }
}