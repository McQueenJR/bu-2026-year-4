using UnityEngine;

// ดูสถานะเกม พอ "เข้า" สถานะที่ห้ามมี display บังจอ ก็สั่งปิดทั้งหมดหนึ่งครั้ง
public class DisplayAutoCloser : MonoBehaviour
{
    private bool wasPolice;
    private bool wasNotInspecting;

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        bool police = gm.isPoliceSequenceActive;
        bool notInspecting = gm.currentState != GameManager.NPCState.Inspecting;

        // จับเฉพาะ "จังหวะที่เพิ่งเข้าสถานะ" ไม่ใช่ทุกเฟรมที่อยู่ในสถานะ
        bool policeStarted = police && !wasPolice;
        bool leftInspecting = notInspecting && !wasNotInspecting;

        wasPolice = police;
        wasNotInspecting = notInspecting;

        if (!policeStarted && !leftInspecting)
            return;

        DisplayEvents.CloseAll();

        // โทรศัพท์: ปิดตอนออกจากสถานะตรวจ แต่ไม่ปิดตอนเรียกตำรวจ (ต้องค้าง "Calling...")
        // ClosePhone() มี guard "กำลังโทรอยู่ปิดไม่ได้" ในตัวอยู่แล้ว
        if (!police && PhoneManager.IsPhoneOpen)
            PhoneManager.Instance.ClosePhone();
    }
}