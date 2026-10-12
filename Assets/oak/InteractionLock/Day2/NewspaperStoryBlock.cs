using System;
using UnityEngine;

// ข้อมูล 1 ก้อนของหนังสือพิมพ์ = ข้อความ + รูป (รูปว่างได้ ช่องรูปจะถูกซ่อน)
// ข้อความรองรับแท็ก TextMeshPro เช่น <b>ตัวหนา</b>
[Serializable]
public class NewspaperStoryBlock
{
    [TextArea(3, 8)]
    public string text;

    public Sprite image;

    public bool IsEmpty => string.IsNullOrWhiteSpace(text) && image == null;

    public static NewspaperStoryBlock Of(string text, Sprite image = null)
    {
        return new NewspaperStoryBlock { text = text, image = image };
    }
}