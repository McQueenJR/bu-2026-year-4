using System;
using TMPro;
using UnityEngine;

// อยู่บน Prefab แม่แบบหนังสือพิมพ์ (World Space: SpriteRenderer + TextMeshPro 3D)
// หน้าที่เดียว: รับ NewspaperRuntime แล้วใส่ข้อความ/รูปลงแต่ละช่อง
// ทำงานแบบเดียวกับ TempleDocumentView
public class NewspaperView : MonoBehaviour
{
    [Serializable]
    public class ImageSlot
    {
        public SpriteRenderer target;

        [Tooltip("ขนาดกรอบสูงสุดของรูป (หน่วยเดียวกับพื้นที่ของ parent) รูปจะย่อ/ขยายให้พอดีกรอบโดยไม่เบี้ยว")]
        public Vector2 maxSize = new Vector2(4f, 2f);

        public void Apply(Sprite sprite)
        {
            if (target == null)
                return;

            // ไม่มีรูป → ซ่อนช่องนั้น
            if (sprite == null)
            {
                target.sprite = null;
                target.gameObject.SetActive(false);
                return;
            }

            target.gameObject.SetActive(true);
            target.sprite = sprite;

            Vector3 size = sprite.bounds.size;

            if (size.x < 0.0001f || size.y < 0.0001f)
                return;

            float scale = Mathf.Min(maxSize.x / size.x, maxSize.y / size.y);
            target.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    [Header("Texts (TextMeshPro 3D — ไม่ใช่ TextMeshPro UI)")]
    [SerializeField] private TextMeshPro dateText;
    [SerializeField] private TextMeshPro newsText;
    [SerializeField] private TextMeshPro missingText;
    [SerializeField] private TextMeshPro reportText;

    [Header("Images")]
    [SerializeField] private ImageSlot newsImage = new ImageSlot();
    [SerializeField] private ImageSlot missingImage = new ImageSlot();
    [SerializeField] private ImageSlot reportImage = new ImageSlot();

    public void Bind(NewspaperRuntime data)
    {
        if (data == null)
        {
            Debug.LogWarning("NewspaperView.Bind: data เป็น null", this);
            return;
        }

        SetText(dateText, data.dateText);

        SetBlock(newsText, newsImage, data.news);
        SetBlock(missingText, missingImage, data.missing);
        SetBlock(reportText, reportImage, data.report);
    }

    private static void SetBlock(TextMeshPro text, ImageSlot image, NewspaperStoryBlock block)
    {
        SetText(text, block != null ? block.text : null);
        image.Apply(block != null ? block.image : null);
    }

    private static void SetText(TextMeshPro target, string value)
    {
        if (target == null)
            return;

        target.text = value ?? string.Empty;
    }
}