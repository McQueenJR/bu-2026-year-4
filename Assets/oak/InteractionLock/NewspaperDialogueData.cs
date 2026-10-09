using UnityEngine;

// บทพูดของคนส่งหนังสือพิมพ์ (แยกจาก NPCData / NPCDialogueSet)
// พูดเรียงตามลำดับ element ทุกบรรทัด ไม่สุ่ม
[CreateAssetMenu(fileName = "NewspaperDialogue", menuName = "Game/Newspaper/Newspaper Dialogue")]
public class NewspaperDialogueData : ScriptableObject
{
    public string speakerName = "คนส่งหนังสือพิมพ์";

    [Tooltip("ใช้คลังเสียงเดิมใน DialogManager > Voice Library")]
    public NpcVoiceType voiceType = NpcVoiceType.None;

    [TextArea(2, 5)]
    public string[] lines;

    public bool HasLines
    {
        get
        {
            if (lines == null) return false;

            foreach (string line in lines)
                if (!string.IsNullOrWhiteSpace(line))
                    return true;

            return false;
        }
    }
}