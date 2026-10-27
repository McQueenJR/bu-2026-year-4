using UnityEngine;

// [ENDING-SCORE] คิดเปอร์เซ็นต์และเกรดแรงค์ (ใช้ทั้งรายวันและแคมเปญ)
// คิดจาก คะแนนที่ได้ ÷ คะแนนเต็ม × 100 — ห้ามเอา % รายวันมาบวกกัน
//
// ระบบคะแนนต่อ NPC 1 ตัว: ตัดสิน 80% (20) + เช็คลิสต์ 20% (0–5) = เต็ม 25
//   ปล่อยชาวบ้าน / กำจัดผี / ปฏิเสธชาวบ้านที่ห้องมีผี ......... 20
//   ปฏิเสธผี ................................................ 14
//   ปฏิเสธชาวบ้านทั่วไป .................................... 6
//   ผิดร้ายแรง (ปล่อยผี / Emergency ใส่ชาวบ้าน / ปล่อยชาวบ้านเข้าห้องที่มีผี) = NPC ตัวนั้น 0
//   เช็คลิสต์: ถูกข้อละ +1 ผิด 0 (ไม่ติดลบ) | Emergency กำจัดผีถูก = เต็ม 5 อัตโนมัติ
// หักรายวัน: ชาวบ้านตายทุกสาเหตุ −5 ต่อคน (ดู CampaignStatsTracker)
public static class ScoreRankUtility
{
    // ---------- คะแนนต่อ NPC ----------
    public const int PointsCorrectDecision = 20;
    public const int PointsRejectGhost = 14;
    public const int PointsRejectVillager = 6;
    public const int MaxChecklistPoints = 5;

    public const int MaxPerNPC = PointsCorrectDecision + MaxChecklistPoints;   // 25

    // หักต่อชาวบ้าน 1 คนที่ตาย (ผีฆ่า / หายสาบสูญ / Emergency)
    public const int NightDeathPenalty = 5;

    // ---------- เกณฑ์เกรด (เปอร์เซ็นต์ขั้นต่ำ) ----------
    public const int ThresholdS = 95;
    public const int ThresholdA = 80;
    public const int ThresholdB = 70;
    public const int ThresholdC = 55;
    public const int ThresholdD = 45;

    public static float GetPercent(int earned, int max)
    {
        if (max <= 0)
            return 0f;

        return Mathf.Clamp((float)earned / max * 100f, 0f, 100f);
    }

    // S ≥95 | A ≥80 | B ≥70 | C ≥55 | D ≥45 | F <45
    // ไม่ปัดเศษ — เทียบด้วยเลขจำนวนเต็ม กัน float เพี้ยนที่ขอบ (เช่น 80.0% พอดี = A)
    // ไม่มี NPC ที่ตัดสินเลย = F
    public static string GetRank(int earned, int max)
    {
        if (max <= 0)
            return "F";

        earned = Mathf.Clamp(earned, 0, max);

        long e = (long)earned * 100;

        if (e >= ThresholdS * (long)max) return "S";
        if (e >= ThresholdA * (long)max) return "A";
        if (e >= ThresholdB * (long)max) return "B";
        if (e >= ThresholdC * (long)max) return "C";
        if (e >= ThresholdD * (long)max) return "D";
        return "F";
    }
}