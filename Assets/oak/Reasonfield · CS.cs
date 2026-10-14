using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
 
// ช่องเหตุผลของใบคำขอ
// - เหตุผลจริง 2 ช่อง (Normal / Normal 2): สุ่ม 1 ใน 2 (ช่องว่างจะถูกข้าม) → isFake = false
// - ติ๊ก Use Fake: สุ่มจาก Fake Values แทน → isFake = true
// - ติ๊ก Use Fake แต่ Fake Values ว่าง → เตือนใน Console แล้วใช้เหตุผลจริงแทน
[System.Serializable]
public class ReasonField
{
    [Tooltip("เหตุผลจริงช่องที่ 1")]
    [FormerlySerializedAs("reason1")]
    public string normal;
 
    [Tooltip("เหตุผลจริงช่องที่ 2 (ว่าง = ใช้ช่องที่ 1 เสมอ | กรอก = สุ่ม 1 ใน 2)")]
    public string normal2;
 
    [Tooltip("ติ๊ก = NPC ตัวนี้ใช้เหตุผลปลอม (สุ่มจาก Fake Values) แทนเหตุผลจริง")]
    public bool useFake;
 
    [Tooltip("รายการเหตุผลปลอม ระบบสุ่มเลือก 1 ค่า ตอนสร้างเอกสารของวันนั้น")]
    public List<string> fakeValues = new List<string>();
 
    // สุ่มได้ครั้งเดียวต่อเอกสาร (เรียกจาก TempleDocumentGenerator เท่านั้น)
    public FieldValue<string> Roll(string fieldName = null, Object context = null)
    {
        if (useFake)
        {
            List<string> fakes = new List<string>();
            if (fakeValues != null)
            {
                foreach (string f in fakeValues)
                    if (!string.IsNullOrWhiteSpace(f))
                        fakes.Add(f);
            }
 
            if (fakes.Count > 0)
                return new FieldValue<string>(fakes[Random.Range(0, fakes.Count)], true);
 
            Debug.LogWarning(
                $"ฟิลด์ '{fieldName}' ติ๊ก Use Fake แต่ Fake Values ว่าง → ใช้เหตุผลจริงแทน",
                context);
        }
 
        List<string> reals = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(normal)) reals.Add(normal);
        if (!string.IsNullOrWhiteSpace(normal2)) reals.Add(normal2);
 
        if (reals.Count == 0)
        {
            Debug.LogWarning($"ฟิลด์ '{fieldName}' ไม่มีเหตุผลจริงทั้ง 2 ช่อง", context);
            return new FieldValue<string>(string.Empty, false);
        }
 
        return new FieldValue<string>(reals[Random.Range(0, reals.Count)], false);
    }
}
 