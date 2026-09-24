using UnityEngine;

// ผลสุ่มของเอกสาร 1 ใบ ของ NPC 1 คน ในวันนั้น
// - สร้างครั้งเดียวโดย TempleDocumentGenerator แล้วเก็บใน TodayApplicant.templeDocument
// - เปิดเอกสารกี่ครั้งก็อ่านจากตัวนี้ ค่าจึงไม่เปลี่ยน
// - เป็น plain class (ไม่ใช่ ScriptableObject) จึงไม่ไปเขียนทับ asset
[System.Serializable]
public class TempleDocumentRuntime
{
    public int generatedDay;

    [Header("Paper / Images")]
    public Sprite paper;                       // null = ใช้กระดาษใน Template
    public FieldValue<Sprite> logo;
    public FieldValue<Sprite> photo;

    [Header("Text")]
    public FieldValue<string> idNumber;
    public FieldValue<string> firstName;
    public FieldValue<string> lastName;
    public FieldValue<string> tentNumber;
    public FieldValue<string> reason;

    [Header("Signature")]
    public Sprite signature;
    public bool showSignature;                 // snapshot ตอนสร้างเอกสาร

    // มีฟิลด์ไหนเป็นค่าปลอมบ้างไหม (ไว้ต่อยอดกับ Checklist / เฉลย)
    public bool HasAnyAnomaly =>
        logo.isFake || photo.isFake ||
        idNumber.isFake || firstName.isFake || lastName.isFake ||
        tentNumber.isFake || reason.isFake;

    public override string ToString()
    {
        return $"วัน {generatedDay} | ID:{Show(idNumber)} | ชื่อ:{Show(firstName)} | สกุล:{Show(lastName)}" +
               $" | เต็นท์:{Show(tentNumber)} | เหตุผล:{Show(reason)}" +
               $" | รูป:{Show(photo)} | โลโก้:{Show(logo)}" +
               $" | ลายเซ็น:{(showSignature ? "มี" : "ไม่มี")}";
    }

    private static string Show(FieldValue<string> f)
    {
        return f.isFake ? $"{f.value} [FAKE]" : f.value;
    }

    private static string Show(FieldValue<Sprite> f)
    {
        string n = f.value != null ? f.value.name : "null";
        return f.isFake ? $"{n} [FAKE]" : n;
    }
}