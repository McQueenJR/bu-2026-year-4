using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    [Header("Display")]
    public TMP_Text timeText;      // เวลา เช่น 20:00
    public TMP_Text dayText;       // DAY 1 - DAY 7

    [Header("Flicker")]
    public ClockFlicker clockFlicker;

    [Tooltip("ให้กระพริบตอนเปลี่ยนวันใหม่")]
    public bool flickerOnNewDay = true;

    private int lastHour = -1;
    private int lastDay = -1;

    void Awake()
    {
        // ถ้าไม่ได้ลาก ClockFlicker มา จะหาเองจาก TimeText
        if (clockFlicker == null && timeText != null)
            clockFlicker = timeText.GetComponent<ClockFlicker>();
    }

    // =========================
    // แสดงเวลา
    // =========================
    public void SetHour(int hour)
    {
        bool hourChanged = hour != lastHour;

        if (timeText != null)
            timeText.text = hour.ToString("00") + ":00";

        lastHour = hour;

        // กระพริบเมื่อเวลาเปลี่ยน
        if (hourChanged && clockFlicker != null)
            clockFlicker.TriggerFlicker();
    }

    // =========================
    // แสดงวัน
    // =========================
    public void SetDay(int day)
    {
        bool dayChanged = day != lastDay;

        if (dayText != null)
            dayText.text = "DAY " + day;

        lastDay = day;

        // กระพริบเมื่อเปลี่ยนวัน
        if (dayChanged && flickerOnNewDay && clockFlicker != null)
            clockFlicker.TriggerFlicker();
    }

    // =========================
    // เริ่มวันใหม่ (ตั้งทั้งวันและเวลา)
    // =========================
    public void SetNewDay(int day, int startHour)
    {
        SetDay(day);
        SetHour(startHour);
    }

    // =========================
    // รีเฟรชหน้าจอ (ใช้กรณีโหลดเซฟ หรือเริ่มเกม)
    // =========================
    public void RefreshClock(int day, int hour)
    {
        lastDay = -1;
        lastHour = -1;

        SetDay(day);
        SetHour(hour);
    }
}