using System;
using UnityEngine;

// ตารางข่าวคนสูญหายของชาวบ้านแต่ละคน (ข้อความ + รูปประจำตัว)
// สร้างจาก Create > Game > Newspaper > Missing Database
[CreateAssetMenu(fileName = "NewspaperMissingDatabase", menuName = "Game/Newspaper/Missing Database")]
public class NewspaperMissingDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public NPCData villager;

        [TextArea(2, 6)]
        public string missingText;

        public Sprite photo;
    }

    public Entry[] entries;

    [Header("ใช้เมื่อหาชาวบ้านคนนั้นในตารางไม่เจอ ({name} = ชื่อ NPC)")]
    [TextArea(2, 5)]
    public string fallbackText = "{name} หายตัวไปอย่างไร้ร่องรอย";

    public Sprite fallbackPhoto;

    public NewspaperStoryBlock Get(NPCData npc)
    {
        if (npc == null)
            return null;

        if (entries != null)
        {
            foreach (Entry e in entries)
            {
                if (e == null || e.villager == null)
                    continue;

                // เทียบ reference หรือชื่อ (เหมือนที่ DeathTracker ทำ)
                if (e.villager == npc || e.villager.npcName == npc.npcName)
                    return NewspaperStoryBlock.Of(e.missingText, e.photo);
            }
        }

        Debug.LogWarning(
            $"[Newspaper] ไม่พบ {npc.npcName} ใน Missing Database → ใช้ข้อความสำรอง"
        );

        string text = (fallbackText ?? string.Empty).Replace("{name}", npc.npcName);
        return NewspaperStoryBlock.Of(text, fallbackPhoto);
    }
}