using UnityEngine;
 
// ข้อมูลตั้งต้นของเอกสารขอเข้าวัด (Template) ของ NPC 1 ตัว/1 variant
// ลากไปใส่ที่ NPCData.templeDocumentData
// ★ asset นี้ห้ามเก็บผลสุ่มของวัน — ผลสุ่มไปอยู่ที่ TempleDocumentRuntime
[CreateAssetMenu(fileName = "New Temple Document Data", menuName = "Game/Temple Document Data")]
public class TempleDocumentData : ScriptableObject
{
    [Header("Paper")]
    [Tooltip("ไม่ติ๊ก / list ว่าง = ใช้กระดาษของ Template prefab | ติ๊ก+มีหลายรูป = สุ่ม 1 รูป")]
    public PaperVariantField paperBackground = new PaperVariantField();
 
    [Header("Images")]
    public AnomalyField<Sprite> logo = new AnomalyField<Sprite>();
    public AnomalyField<Sprite> photo = new AnomalyField<Sprite>();
 
    [Header("Text")]
    [Tooltip("เลขประจำตัว 5 หลัก — ติ๊กหลักที่ต้องการให้สุ่มเป็นเลขอื่น (ไม่ติ๊ก = ใช้เลขเดิม)")]
    public DigitAnomalyField idDigits = new DigitAnomalyField();
 
    public AnomalyField<string> firstName = new AnomalyField<string>();
    public AnomalyField<string> lastName = new AnomalyField<string>();
    public AnomalyField<string> occupation = new AnomalyField<string>();
    [Tooltip("เต็นท์ประจำตัวของ NPC (คงที่) เช่น A3-01 — ค่า Fake = ห้องผิด")]
    public AnomalyField<string> tentNumber = new AnomalyField<string>();
    public AnomalyField<string> reason = new AnomalyField<string>();
 
    [Header("Signature")]
    [Tooltip("จะ 'แสดงหรือไม่' ขึ้นกับสถานะเจ้าอาวาส (AbbotStatusManager), ส่วน 'ใช้รูปไหน' ขึ้นกับ Use Fake ตรงนี้")]
    public AnomalyField<Sprite> signature = new AnomalyField<Sprite>();
 
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