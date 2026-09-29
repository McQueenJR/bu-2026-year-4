using System.Collections.Generic;
using UnityEngine;

// ต่างจาก AnomalyField: ไม่มีแนวคิด "จริง/ปลอม"
// - ไม่ติ๊ก หรือ ติ๊กแต่ list ว่าง → คืน null (View จะใช้กระดาษ default ของ Template prefab)
// - ติ๊ก + มี 1 รูป → ใช้รูปนั้นเสมอ
// - ติ๊ก + มีหลายรูป → สุ่ม 1 รูป ตอนสร้างเอกสาร (ครั้งเดียว เหมือนฟิลด์อื่น)
[System.Serializable]
public class PaperVariantField
{
    [Tooltip("ติ๊ก = ใช้กระดาษจากรายการด้านล่างแทนกระดาษ default ของ Template")]
    public bool useCustomPaper;

    [Tooltip("ใส่ได้หลายแบบ ถ้ามีมากกว่า 1 จะสุ่มเลือก 1 แบบต่อเอกสาร")]
    public List<Sprite> variants = new List<Sprite>();

    public Sprite Roll()
    {
        if (!useCustomPaper || variants == null || variants.Count == 0)
            return null;

        if (variants.Count == 1)
            return variants[0];

        return variants[Random.Range(0, variants.Count)];
    }
}