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

        [Tooltip("ใช้เฉพาะเป็นค่าสำรอง กรณีช่องไม่มี Sprite placeholder ใน Template (ปกติไม่ใช้) " +
                 "ขนาดกรอบจริงมาจาก Scale ที่จัดไว้ใน Template ร่วมกับ Sprite placeholder (เช่น Square)")]
        public Vector2 maxSize = new Vector2(4f, 2f);
        // ---- ค่าที่จำจาก Template (ไม่ serialize เพื่อไม่ให้ถูกบันทึกลง Prefab/Scene) ----
        [NonSerialized] private bool captured;
        [NonSerialized] private Vector2 frameSize;          // ขนาดกรอบ (หน่วยของ parent)
        [NonSerialized] private Vector3 frameCenterLocal;   // จุดกึ่งกลางกรอบ (local space ของ parent)
        [NonSerialized] private float baseScaleZ = 1f;

        // จำกรอบครั้งเดียว ต้องเรียกก่อนแตะ sprite/scale ใดๆ
        // กรอบ = localScale x ขนาดของ Sprite ที่อยู่ใน renderer ตอนนั้น
        private void CaptureFrame()
        {
            if (captured)
                return;

            captured = true;

            Transform t = target.transform;
            Vector3 ls = t.localScale;
            baseScaleZ = ls.z;

            Sprite current = target.sprite;
            if (current != null &&
                current.bounds.size.x > 0.0001f &&
                current.bounds.size.y > 0.0001f)
            {
                Bounds b = current.bounds;
                frameSize = new Vector2(Mathf.Abs(ls.x * b.size.x), Mathf.Abs(ls.y * b.size.y));
                frameCenterLocal = t.localPosition + t.localRotation * Vector3.Scale(ls, b.center);
            }
            else
            {
                // ไม่มี placeholder ใน Template → ใช้ maxSize เป็นกรอบ และถือว่า pivot อยู่กลาง
                frameSize = maxSize;
                frameCenterLocal = t.localPosition;
                Debug.LogWarning("NewspaperView.ImageSlot: ไม่พบ Sprite placeholder ใน '" + target.name +
                                 "' ใช้ maxSize เป็นขนาดกรอบแทน", target);
            }
        }
        public void Apply(Sprite sprite)
        {
            if (target == null)
                return;
            
            CaptureFrame();

            // ไม่มีรูป → ซ่อนช่องนั้น (ไม่แตะ scale/position)
            if (sprite == null)
            {
                target.sprite = null;
                target.gameObject.SetActive(false);
                return;
            }

            target.gameObject.SetActive(true);
            target.sprite = sprite;

            Bounds bounds = sprite.bounds;
            if (bounds.size.x < 0.0001f || bounds.size.y < 0.0001f)
                return;

            // Stretch: คำนวณแกน X, Y แยกกัน ให้รูปเต็มกรอบเดิมเสมอ
            Vector3 scale = new Vector3(
                frameSize.x / bounds.size.x,
                frameSize.y / bounds.size.y,
                baseScaleZ);

            Transform t = target.transform;
            t.localScale = scale;

            // รักษาตำแหน่ง "กึ่งกลางกรอบ" ให้เท่าเดิม แม้ Sprite ใหม่มี pivot ไม่อยู่กลาง
            t.localPosition = frameCenterLocal - t.localRotation * Vector3.Scale(scale, bounds.center);
        }
    }
    
    
    [Header("Paper (ใช้วัดความสูงเพื่อ Fit เข้าจอใน NewspaperReaderPopup)")]
    [Tooltip("SpriteRenderer ของกระดาษพื้นหลังใน Template นี้ (ลาก Paper มาใส่)")]
    [SerializeField] private SpriteRenderer paperRenderer;

    public SpriteRenderer PaperRenderer => paperRenderer;
    
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

        target.text = ThaiWordBreaker.Process(value);
    }
}