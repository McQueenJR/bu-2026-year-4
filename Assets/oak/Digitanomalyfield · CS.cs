using UnityEngine;
 
// ฟิลด์เลขประจำตัว 5 หลัก: ผู้พัฒนาเขียนเลขต้นฉบับ + ติ๊กได้ว่าหลักไหนให้สุ่ม
// หลักที่ติ๊ก → สุ่ม 0-9 โดยไม่ซ้ำเลขเดิมของหลักนั้น | ไม่ติ๊กเลย → ใช้เลขต้นฉบับ (ไม่ Fake)
[System.Serializable]
public class DigitAnomalyField
{
    public const int DigitCount = 5;
 
    [Tooltip("เลขต้นฉบับ 5 หลัก")]
    public string original = "00000";
 
    [Tooltip("ติ๊กหลักที่ต้องการให้สุ่ม (index 0 = หลักที่ 1)")]
    public bool[] randomize = new bool[DigitCount];
 
    public FieldValue<string> Roll(string fieldName, Object owner)
    {
        char[] digits = Normalize(fieldName, owner);
        bool anyFake = false;
 
        for (int i = 0; i < DigitCount; i++)
        {
            bool flagged = randomize != null && i < randomize.Length && randomize[i];
            if (!flagged)
                continue;
 
            int orig = digits[i] - '0';
            int r = Random.Range(0, 9); // 0-8
            if (r >= orig) r++;         // ข้ามเลขเดิม → ได้ 0-9 ที่ไม่ใช่ orig
            digits[i] = (char)('0' + r);
            anyFake = true;
        }
 
        return new FieldValue<string>(new string(digits), anyFake);
    }
 
    // แปลงเลขต้นฉบับให้เป็น 5 หลักเสมอ (เตือนถ้าผิดรูปแบบ)
    private char[] Normalize(string fieldName, Object owner)
    {
        string src = original ?? string.Empty;
        char[] result = new char[DigitCount];
        bool warned = false;
 
        for (int i = 0; i < DigitCount; i++)
        {
            if (i < src.Length && char.IsDigit(src[i]))
            {
                result[i] = src[i];
            }
            else
            {
                result[i] = '0';
                warned = true;
            }
        }
 
        if (warned || src.Length != DigitCount)
            Debug.LogWarning($"[{fieldName}] เลขต้นฉบับ \"{src}\" ต้องเป็นตัวเลข {DigitCount} หลัก", owner);
 
        return result;
    }
}