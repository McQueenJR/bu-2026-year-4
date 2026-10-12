using System;
using UnityEngine;

// เหตุการณ์ของชาวบ้าน 1 คน (ใช้กับรายงานจากชาวบ้านวันที่ 5)
[Serializable]
public class NewspaperVillagerEvent
{
    [Tooltip("ลาก NPCData ของชาวบ้านคนนี้ (ตัวเดียวกับที่ใช้ใน CampDatabase)")]
    public NPCData villager;

    public NewspaperStoryBlock report = new NewspaperStoryBlock();
}