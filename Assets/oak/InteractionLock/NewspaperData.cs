using UnityEngine;

// หนังสือพิมพ์ 1 ฉบับ (สร้างได้จาก Create > Game > Newspaper > Newspaper Data)
[CreateAssetMenu(fileName = "New Newspaper", menuName = "Game/Newspaper/Newspaper Data")]
public class NewspaperData : ScriptableObject
{
    [Header("ข้อมูล (ไว้จำเฉยๆ ไม่ได้ใช้ในโค้ด)")]
    public string title;

    [Header("ภาพที่แสดงตอนเปิดอ่าน (ภาพใหญ่ คมชัด)")]
    public Sprite readingSprite;

    [Header("ภาพที่วางบนโต๊ะ (ว่าง = ใช้ภาพอ่านแทน)")]
    public Sprite deskSprite;

    public Sprite DeskSprite => deskSprite != null ? deskSprite : readingSprite;
}