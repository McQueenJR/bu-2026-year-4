using System;

// ศูนย์กลางสั่ง "ปิด display ทั้งหมดที่บังหน้าจอ"
public static class DisplayEvents
{
    public static event Action OnCloseAll;

    public static void CloseAll()
    {
        OnCloseAll?.Invoke();
    }
}