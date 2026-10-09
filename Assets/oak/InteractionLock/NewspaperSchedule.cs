using System;
using UnityEngine;

[Serializable]
public class NewspaperDayEntry
{
    [Min(1)] public int day = 1;

    public NewspaperData newspaper;

    [Tooltip("ว่าง = ใช้ Default Dialogue ที่ตั้งไว้ใน NewspaperDeliveryManager")]
    public NewspaperDialogueData dialogueOverride;
}

// ตารางว่าวันไหนใช้หนังสือพิมพ์ฉบับไหน
[CreateAssetMenu(fileName = "NewspaperSchedule", menuName = "Game/Newspaper/Newspaper Schedule")]
public class NewspaperSchedule : ScriptableObject
{
    public NewspaperDayEntry[] days;

    public NewspaperDayEntry GetEntry(int day)
    {
        if (days == null)
            return null;

        foreach (NewspaperDayEntry entry in days)
        {
            if (entry != null && entry.day == day)
                return entry;
        }

        return null;
    }
}