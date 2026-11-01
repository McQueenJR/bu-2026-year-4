using UnityEngine;

// สถานะล็อกการโต้ตอบกลาง ใช้ช่วงส่งหนังสือพิมพ์
// สคริปต์อื่นเช็ก InteractionLock.IsLocked แล้ว return ได้เลย
public static class InteractionLock
{
    public static bool IsLocked { get; private set; }
    public static bool IsDecisionPhase { get; private set; }

    public static void SetLocked(bool locked)
    {
        IsLocked = locked;
    }

    public static void SetDecisionPhase(bool active)
    {
        IsDecisionPhase = active;
    }

    // ส่ง allowDuringDecision เข้าไป ได้ true = ถูกบล็อก
    public static bool IsBlockedFor(bool allowDuringDecision)
    {
        return IsLocked || (IsDecisionPhase && !allowDuringDecision);
    }
    
    // กันค่าค้างเมื่อปิด Domain Reload ใน Enter Play Mode Options
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        IsLocked = false;
        IsDecisionPhase = false;
    }
}