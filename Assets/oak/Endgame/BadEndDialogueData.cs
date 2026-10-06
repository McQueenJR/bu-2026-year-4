using UnityEngine;

// Dialogue ของ NPC ฉากจบ BadEnd — แยกจาก NPCData โดยเฉพาะ
// ★ เรียงพูดตามลำดับ element ตั้งแต่บนลงล่าง (ไม่สุ่ม)
// ★ ถ้าข้อความมีช่องว่าง → พัก spacePauseDuration วิ แล้วพูดต่อ
[CreateAssetMenu(fileName = "BadEndDialogue", menuName = "Temple/Ending/Bad End Dialogue")]
public class BadEndDialogueData : ScriptableObject
{
    [Header("ชื่อผู้พูด")]
    public string speakerName = "???";

    [Header("บทพูด (เรียงจากบนลงล่าง)")]
    [TextArea(2, 5)]
    public string[] lines;
}