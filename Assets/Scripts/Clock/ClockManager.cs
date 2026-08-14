using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    [Header("Display")]
    public TMP_Text timeText; // ลาก 3D TextMeshPro (Object ในฉาก) มาใส่ตรงนี้ได้เลย ไม่ต้องเปลี่ยน type

    [Header("Flicker")]
    public ClockFlicker clockFlicker; // ถ้าไม่ใส่ ระบบจะหาให้เองจาก GameObject เดียวกับ timeText

    [Tooltip("ให้กระพริบตอนเปลี่ยนวันใหม่ (StartNextDay) ด้วยหรือไม่")]
    public bool flickerOnNewDay = true;

    private int lastHour = -1;

    void Awake()
    {
        // ถ้าไม่ได้ลาก ClockFlicker ไว้เอง ให้ลองหาจาก GameObject ของ timeText อัตโนมัติ
        if (clockFlicker == null && timeText != null)
            clockFlicker = timeText.GetComponent<ClockFlicker>();
    }

    public void SetHour(int hour)
    {
        bool hourChanged = hour != lastHour;

        timeText.text = hour.ToString("00") + ":00";
        lastHour = hour;

        // กระพริบทุกครั้งที่ตัวเลขชั่วโมงเปลี่ยนจริงๆ (ไม่นับตอนเซ็ตค่าซ้ำเดิม)
        if (hourChanged && clockFlicker != null)
            clockFlicker.TriggerFlicker();
    }
}