// ประเภทเอกสารขอเข้าวัด — กำหนดไว้ที่ TempleDocumentData.documentType
// Villager ต้องเป็น 0 เพื่อให้ asset เดิมที่ไม่มีค่านี้ถูกตีเป็นชาวบ้านอัตโนมัติ
// ห้ามสลับลำดับค่าที่มีอยู่ (ค่าถูกเก็บเป็นตัวเลขใน asset) เพิ่มประเภทใหม่ให้ต่อท้ายเท่านั้น
public enum TempleDocumentType
{
    Villager = 0,
    Monk = 1
}