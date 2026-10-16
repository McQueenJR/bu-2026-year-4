using UnityEngine;

// คิดเปอร์เซ็นต์และเกรดแรงค์ (ใช้ทั้งรายวันและแคมเปญ)
// คิดจาก คะแนนที่ได้ ÷ คะแนนเต็ม × 100 — ห้ามเอา % รายวันมาบวกกัน
public static class ScoreRankUtility
{
    public const int MaxPerNPC = 8;          // คะแนนเต็มต่อ NPC 1 ตัว
    public const int NightDeathPenalty = 2;  // หักต่อชาวบ้าน 1 คนที่ตาย (ผีฆ่า / หายสาบสูญ)

    public static float GetPercent(int earned, int max)
    {
        if (max <= 0)
            return 0f;

        return Mathf.Clamp((float)earned / max * 100f, 0f, 100f);
    }

    // S=100 | A=95–99 | B=85–94 | C=70–84 | D=45–69 | F<45
    // ไม่ปัดเศษ (99.6% = A, 94.9% = B) — เทียบด้วยเลขจำนวนเต็มกัน float เพี้ยน
    // ไม่มี NPC ที่ตัดสินเลย = F
    public static string GetRank(int earned, int max)
    {
        if (max <= 0)
            return "F";

        earned = Mathf.Clamp(earned, 0, max);

        if (earned >= max) return "S";

        long e = (long)earned * 100;

        if (e >= 95L * max) return "A";
        if (e >= 85L * max) return "B";
        if (e >= 70L * max) return "C";
        if (e >= 45L * max) return "D";
        return "F";
    }
}