using UnityEngine;

// ข้อมูลตั้งต้นของเอกสารขอเข้าวัด (Template) ของ NPC 1 ตัว/1 variant
// ลากไปใส่ที่ NPCData.templeDocumentData
// ★ asset นี้ห้ามเก็บผลสุ่มของวัน — ผลสุ่มไปอยู่ที่ TempleDocumentRuntime
[CreateAssetMenu(fileName = "New Temple Document Data", menuName = "Game/Temple Document Data")]
public class TempleDocumentData : ScriptableObject
{
    [Header("Paper")]
    [Tooltip("ว่าง = ใช้กระดาษที่อยู่ใน Template prefab")]
    public Sprite paperBackground;

    [Header("Images")]
    public AnomalyField<Sprite> logo = new AnomalyField<Sprite>();
    public AnomalyField<Sprite> photo = new AnomalyField<Sprite>();

    [Header("Text")]
    [Tooltip("เลขประจำตัว (string เผื่อมีตัวอักษร / เลข 0 นำหน้า)")]
    public AnomalyField<string> idNumber = new AnomalyField<string>();
    public AnomalyField<string> firstName = new AnomalyField<string>();
    public AnomalyField<string> lastName = new AnomalyField<string>();
    [Tooltip("เต็นท์ประจำตัวของ NPC (คงที่) เช่น A3-01 — ค่า Fake = ห้องผิด")]
    public AnomalyField<string> tentNumber = new AnomalyField<string>();
    public AnomalyField<string> reason = new AnomalyField<string>();

    [Header("Signature")]
    [Tooltip("ลายเซ็นเจ้าอาวาส — จะแสดงหรือไม่ ขึ้นกับสถานะเจ้าอาวาสตอนสร้างเอกสาร")]
    public Sprite signature;

    // ---- ไว้ทดสอบขั้นที่ 1 ----
    // คลิกขวาที่ชื่อ asset ใน Inspector → "Test Generate (x5)"
    [ContextMenu("Test Generate (x5)")]
    private void TestGenerate()
    {
        for (int i = 0; i < 5; i++)
        {
            TempleDocumentRuntime doc = TempleDocumentGenerator.Generate(this, 1, true);
            Debug.Log($"[{name}] สุ่มครั้งที่ {i + 1}: {doc}", this);
        }
    }
}