using UnityEngine;

// เครื่องมือทดสอบเท่านั้น — ลบทิ้งก่อน Build เกมจริง
// ใช้ตอน Play: กดเมนู ⋮ ของ Component นี้แล้วเลือกคำสั่ง
public class NewspaperDebugTools : MonoBehaviour
{
    [Header("อ้างอิง")]
    public NewspaperSchedule schedule;
    public NewspaperMissingDatabase missingDatabase;
    public string dateFormat = "วันที่ {date} ตุลาคม 2558";
    public int firstDateNumber = 1;

    [Header("บันทึกการตายปลอม (เพื่อทดสอบ)")]
    public NPCData villager;
    public DeathCause cause = DeathCause.VanishedAfterReject;
    [Tooltip("วันที่ตาย — ถ้าจะดูข่าวเช้าวันที่ N ให้ใส่ N-1")]
    public int deathDay = 1;

    [Header("ดูตัวอย่าง")]
    public int previewDay = 2;

    [ContextMenu("1) Record Fake Death")]
    public void RecordFakeDeath()
    {
        if (DeathTracker.Instance == null || villager == null)
        {
            Debug.LogWarning("[NewspaperDebug] ต้อง Play และใส่ Villager ก่อน");
            return;
        }

        DeathTracker.Instance.RecordDeath(villager, cause, deathDay, "debug");
    }

    [ContextMenu("2) Preview Newspaper (ดูผลใน Console)")]
    public void Preview()
    {
        if (schedule == null)
        {
            Debug.LogWarning("[NewspaperDebug] ไม่ได้ใส่ Schedule");
            return;
        }

        NewspaperDayEntry entry = schedule.GetEntry(previewDay);

        if (entry == null || entry.newspaper == null)
        {
            Debug.LogWarning($"[NewspaperDebug] ไม่มีข้อมูล Day {previewDay}");
            return;
        }

        NewspaperRuntime r = NewspaperContentBuilder.Build(
            entry.newspaper, previewDay, missingDatabase, dateFormat, firstDateNumber
        );

        Debug.Log(
            $"===== PREVIEW DAY {previewDay} =====\n" +
            $"วันที่: {r.dateText}\n" +
            $"[ช่อง 1] {r.news.text}  (รูป: {(r.news.image != null ? r.news.image.name : "-")})\n" +
            $"[ช่อง 2] {r.missing.text}  (รูป: {(r.missing.image != null ? r.missing.image.name : "-")})\n" +
            $"[ช่อง 3] {r.report.text}  (รูป: {(r.report.image != null ? r.report.image.name : "-")})"
        );
    }
}