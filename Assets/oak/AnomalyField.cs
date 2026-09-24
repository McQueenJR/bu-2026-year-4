using System.Collections.Generic;
using UnityEngine;

// ผลลัพธ์ของฟิลด์หลังสุ่มแล้ว (เก็บใน TempleDocumentRuntime)
// value  = ค่าที่จะแสดงบนเอกสาร
// isFake = ค่านี้เป็นค่าปลอมหรือไม่ (ไว้ต่อยอดกับ Checklist / เฉลย)
[System.Serializable]
public struct FieldValue<T>
{
    public T value;
    public bool isFake;

    public FieldValue(T value, bool isFake)
    {
        this.value = value;
        this.isFake = isFake;
    }
}

// ฟิลด์ 1 ช่องที่รองรับทั้ง "ค่าจริง" และ "ค่าผิดปกติ"
// ใช้ได้กับ string (ชื่อ, สกุล, เลข ฯลฯ) และ Sprite (รูป, โลโก้)
[System.Serializable]
public class AnomalyField<T>
{
    [Tooltip("ค่าจริง (ปกติ)")]
    public T normal;

    [Tooltip("ติ๊ก = NPC ตัวนี้จะใช้ค่าปลอมแทนค่าจริง")]
    public bool useFake;

    [Tooltip("รายการค่าปลอม ระบบจะสุ่มเลือก 1 ค่า ตอนสร้างเอกสารของวันนั้น")]
    public List<T> fakeValues = new List<T>();

    // สุ่มได้ครั้งเดียวต่อเอกสาร (เรียกจาก TempleDocumentGenerator เท่านั้น)
    // ห้ามเรียกตอนเปิดเอกสาร ไม่งั้นค่าจะเปลี่ยนทุกครั้ง
    public FieldValue<T> Roll(string fieldName = null, UnityEngine.Object context = null)
    {
        if (!useFake)
            return new FieldValue<T>(normal, false);

        if (fakeValues == null || fakeValues.Count == 0)
        {
            Debug.LogWarning(
                $"ฟิลด์ '{fieldName}' ติ๊ก Use Fake แต่ Fake Values ว่าง → ใช้ค่า Normal แทน",
                context
            );
            return new FieldValue<T>(normal, false);
        }

        int index = Random.Range(0, fakeValues.Count);
        return new FieldValue<T>(fakeValues[index], true);
    }
}