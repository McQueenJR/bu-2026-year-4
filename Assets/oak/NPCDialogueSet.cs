using UnityEngine;

// ชุดบทพูดที่แยกออกมาจาก NPCData เพื่อให้หลาย NPC แชร์กันได้
// ลาก asset นี้ใส่ NPCData.dialogueSet
// ★ ถ้า NPC ตัวไหนใส่ตัวนี้ไว้ ระบบจะใช้ทั้ง 4 หมวดจาก asset นี้ทั้งหมด
//   (ไม่ผสมกับ dialogs/greenDialogs/... ที่กรอกไว้ใน NPCData เดิม)
[CreateAssetMenu(fileName = "New Dialogue Set", menuName = "Game/NPC Dialogue Set")]
public class NPCDialogueSet : ScriptableObject
{
    [Header("Normal")]
    [TextArea(2, 5)]
    public string[] dialogs;

    [Header("Green Button Dialog")]
    [TextArea(2, 5)]
    public string[] greenDialogs;

    [Header("Red Button Dialog")]
    [TextArea(2, 5)]
    public string[] redDialogs;

    [Header("Emergency Dialog")]
    [TextArea(2, 5)]
    public string[] emergencyDialogs;
}