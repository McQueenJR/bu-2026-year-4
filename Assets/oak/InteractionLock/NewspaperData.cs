using UnityEngine;

// ข้อมูลหนังสือพิมพ์ของ "1 วัน" (สร้างจาก Create > Game > Newspaper > Newspaper Data)
[CreateAssetMenu(fileName = "New Newspaper", menuName = "Game/Newspaper/Newspaper Data")]
public class NewspaperData : ScriptableObject
{
    [Header("ข้อมูล (ไว้จำเฉยๆ ไม่ได้ใช้ในโค้ด)")]
    public string title;

    [Header("ภาพหนังสือพิมพ์บนโต๊ะ (ไม่ใส่ = ใช้ภาพที่อยู่ใน Prefab)")]
    public Sprite deskSprite;

    [Header("ช่อง 1 : ข่าวจากหอกระจายข่าว")]
    [Tooltip("ติ๊กเฉพาะวัน 2 และ 7 : เลือกข้อความตามว่ามีชาวบ้านตัวจริงตายแล้วหรือไม่")]
    public bool useDeathHistoryForNews = false;

    [Tooltip("ข้อความปกติ (ใช้เมื่อไม่ติ๊กด้านบน)")]
    public NewspaperStoryBlock news = new NewspaperStoryBlock();

    public NewspaperStoryBlock newsIfNoDeath = new NewspaperStoryBlock();
    public NewspaperStoryBlock newsIfAnyDeath = new NewspaperStoryBlock();

    [Header("ช่อง 2 : หมวดคนสูญหาย")]
    [Tooltip("ข้อความเมื่อเมื่อวานไม่มีคนสูญหาย")]
    [TextArea(2, 5)]
    public string noMissingText = "ยังไม่มีคนสูญหายถือเป็นข่าวดี";

    [Header("ช่อง 3 : รายงานจากชาวบ้าน")]
    [Tooltip("ข้อความปกติ (ใช้เมื่อไม่ติ๊กสุ่มเหตุการณ์)")]
    public NewspaperStoryBlock report = new NewspaperStoryBlock();

    [Tooltip("ติ๊กเฉพาะวัน 5 : สุ่ม 1 เหตุการณ์จากชาวบ้านที่ยังมีชีวิต")]
    public bool useRandomAliveVillagerEvent = false;

    public NewspaperVillagerEvent[] villagerEvents;

    [Tooltip("ใช้เมื่อชาวบ้านในรายการตายหมดทุกคน")]
    public NewspaperStoryBlock reportIfAllDead = new NewspaperStoryBlock();
}