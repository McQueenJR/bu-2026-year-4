// สร้าง TempleDocumentRuntime จาก TempleDocumentData (สุ่มครั้งเดียว)
// ไม่พึ่ง MonoBehaviour / Manager ใด ๆ → ทดสอบง่าย
// ผู้เรียก (SpawnManager) เป็นคนตัดสินว่าวันนี้ลายเซ็นเปิดหรือไม่ แล้วส่งเข้ามาเป็น signatureActive
public static class TempleDocumentGenerator
{
    public static TempleDocumentRuntime Generate(
        TempleDocumentData data,
        int currentDay,
        bool signatureActive = true)
    {
        // NPC ที่ยังไม่ย้ายมาใช้ระบบใหม่ → คืน null แล้วให้ใช้ applicantPhotoPrefab เดิม
        if (data == null)
            return null;

        return new TempleDocumentRuntime
        {
            generatedDay = currentDay,

            paper = data.paperBackground,
            logo = RollField(data.logo, "Logo", data),
            photo = RollField(data.photo, "Photo", data),

            idNumber = RollField(data.idNumber, "ID Number", data),
            firstName = RollField(data.firstName, "First Name", data),
            lastName = RollField(data.lastName, "Last Name", data),
            tentNumber = RollField(data.tentNumber, "Tent Number", data),
            reason = RollField(data.reason, "Reason", data),

            signature = data.signature,
            showSignature = signatureActive && data.signature != null
        };
    }

    private static FieldValue<T> RollField<T>(
        AnomalyField<T> field,
        string fieldName,
        TempleDocumentData owner)
    {
        if (field == null)
            return default;

        return field.Roll(fieldName, owner);
    }
}