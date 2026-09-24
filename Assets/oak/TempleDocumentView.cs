using TMPro;
using UnityEngine;

// อยู่บน TempleDocumentTemplate prefab (World Space: SpriteRenderer + TextMeshPro 3D)
// หน้าที่เดียว: รับ TempleDocumentRuntime แล้วเอาค่าไปใส่ช่องต่าง ๆ
// ไม่สุ่ม ไม่ตัดสินใจอะไรเอง → เปิดกี่ครั้งก็ได้ค่าเดิม
public class TempleDocumentView : MonoBehaviour
{
    [Header("Images (SpriteRenderer)")]
    [SerializeField] private SpriteRenderer paper;
    [SerializeField] private SpriteRenderer logo;
    [SerializeField] private SpriteRenderer photo;
    [SerializeField] private SpriteRenderer signature;

    [Header("Texts (TextMeshPro 3D — ไม่ใช่ TextMeshPro UI)")]
    [SerializeField] private TextMeshPro idNumberText;
    [SerializeField] private TextMeshPro firstNameText;
    [SerializeField] private TextMeshPro lastNameText;
    [SerializeField] private TextMeshPro tentNumberText;
    [SerializeField] private TextMeshPro reasonText;

    public void Bind(TempleDocumentRuntime data)
    {
        if (data == null)
        {
            Debug.LogWarning("TempleDocumentView.Bind: data เป็น null", this);
            return;
        }

        // กระดาษ: ถ้า data ไม่ระบุ ใช้กระดาษที่อยู่ใน prefab
        if (paper != null && data.paper != null)
            paper.sprite = data.paper;

        SetSprite(logo, data.logo.value);
        SetSprite(photo, data.photo.value);

        SetText(idNumberText, data.idNumber.value);
        SetText(firstNameText, data.firstName.value);
        SetText(lastNameText, data.lastName.value);
        SetText(tentNumberText, data.tentNumber.value);
        SetText(reasonText, data.reason.value);

        // ลายเซ็น: เปิดตาม snapshot ที่ Generator ตัดสินไว้แล้ว
        if (signature != null)
        {
            bool show = data.showSignature && data.signature != null;
            signature.sprite = data.signature;
            signature.gameObject.SetActive(show);
        }
    }

    // sprite เป็น null (เช่น ค่าปลอมที่ตั้งใจให้ "ไม่มีรูป") → ซ่อนช่องนั้น
    private static void SetSprite(SpriteRenderer target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = sprite != null;
    }

    private static void SetText(TextMeshPro target, string value)
    {
        if (target == null)
            return;

        target.text = value ?? string.Empty;
    }
}