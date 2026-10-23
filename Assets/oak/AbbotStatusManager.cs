using UnityEngine;

// สถานะเจ้าอาวาส + กฎลายเซ็นบนเอกสาร
// กฎ: ลายเซ็นยังแสดงได้จนถึง "วันที่เจ้าอาวาสตาย" (รวมวันนั้น) วันถัด ๆ ไปไม่มีลายเซ็น
// คำนวณจาก state ไม่ต้องมี event ปิดลายเซ็น: StartNextDay เพิ่ม currentDay แล้ว Generate ใหม่ ก็ปิดเอง
public class AbbotStatusManager : MonoBehaviour
{
    public static AbbotStatusManager Instance;

    [Header("สถานะเจ้าอาวาส")]
    [Tooltip("-1 = ยังมีชีวิตอยู่ / ค่าอื่น = วันที่เจ้าอาวาสตาย")]
    [SerializeField] private int deathDay = -1;

    [Header("ตัวตนเจ้าอาวาส")]
    [Tooltip("ต้องตรงกับ npcName ใน NPCData ของเจ้าอาวาสสุจิโตเป๊ะ ๆ")]
    [SerializeField] private string abbotNpcName = "";
    public bool IsAlive => deathDay < 0;
    public int DeathDay => deathDay;

    private void Awake()
    {
        Instance = this;
    }
    public bool IsAbbot(NPCData npc)
    {
        return npc != null &&
               !string.IsNullOrEmpty(abbotNpcName) &&
               npc.npcName == abbotNpcName;
    }

    // DeathTracker เรียกทุกครั้งที่มีการบันทึกการตาย
    // ไม่ใช่เจ้าอาวาส = ไม่ทำอะไร (คนอื่นตายแล้วลายเซ็นไม่หาย)
    public void NotifyNpcDied(NPCData npc, int day)
    {
        if (IsAbbot(npc))
            MarkDead(day);
    }
    // เรียกจากที่ไหนก็ได้ในอนาคต เมื่อเจ้าอาวาสตาย เช่น AbbotStatusManager.Instance.MarkDead(GameManager.Instance.currentDay);
    public void MarkDead(int day)
    {
        if (!IsAlive)
            return;

        deathDay = day;
        Debug.Log($"เจ้าอาวาสตายในวันที่ {day} (เอกสารของวันนี้ยังมีลายเซ็น วันถัดไปจะไม่มี)");
    }

    // วันนี้เอกสารที่สร้างใหม่ควรมีลายเซ็นไหม
    public bool ShouldShowSignature(int day)
    {
        return IsAlive || day <= deathDay;
    }

    // เรียกตอนเริ่มเกมใหม่ทั้งเกม
    public void ResetForNewGame()
    {
        deathDay = -1;
    }

    // ---- ไว้ทดสอบ: คลิกขวาที่หัว component ตอนกด Play ----
    [ContextMenu("Test: Mark Dead Today")]
    private void TestMarkDeadToday()
    {
        int day = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;
        MarkDead(day);
    }

    [ContextMenu("Test: Reset (Alive)")]
    private void TestReset()
    {
        ResetForNewGame();
    }
}