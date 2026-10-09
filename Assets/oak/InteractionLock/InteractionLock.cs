using UnityEngine;

// สถานะล็อกการโต้ตอบกลาง ใช้ช่วงส่งหนังสือพิมพ์
// สคริปต์อื่นเช็ก InteractionLock.IsLocked แล้ว return ได้เลย
public static class InteractionLock
{
    public static bool IsLocked { get; private set; }

    public static void SetLocked(bool locked)
    {
        IsLocked = locked;
    }

    // กันค่าค้างเมื่อปิด Domain Reload ใน Enter Play Mode Options
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        IsLocked = false;
    }
}