
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TMPTextHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text")]
    public TMP_Text targetText;

    [Header("Hover Color")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    [Header("Glow")]
    public bool useGlow = true;
    public Color glowColor = Color.yellow;
    [Range(0f, 1f)]
    public float glowOuter = 0.3f;

    [Header("Hover Sound")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    private Material textMaterial;
    private Color originalColor;
    private Color originalGlowColor;
    private float originalGlowOuter;
    private bool hasGlowColor;
    private bool hasGlowOuter;

    void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        if (targetText == null)
        {
            Debug.LogWarning("TMPTextHoverEffect: ไม่พบ TMP_Text", this);
            enabled = false;
            return;
        }

        targetText.raycastTarget = true;
        originalColor = targetText.color;

        // สร้าง Material Instance สำหรับข้อความนี้
        // เพื่อไม่ให้เปลี่ยน Material ของข้อความอื่นพร้อมกัน
        textMaterial = targetText.fontMaterial;

        if (textMaterial != null)
        {
            hasGlowColor = textMaterial.HasProperty("_GlowColor");
            hasGlowOuter = textMaterial.HasProperty("_GlowOuter");

            if (hasGlowColor)
                originalGlowColor = textMaterial.GetColor("_GlowColor");

            if (hasGlowOuter)
                originalGlowOuter = textMaterial.GetFloat("_GlowOuter");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // เปลี่ยนสีข้อความเมื่อเมาส์ชี้
        targetText.color = hoverColor;

        // เพิ่ม Glow หาก Shader รองรับ
        if (useGlow && textMaterial != null)
        {
            if (hasGlowColor)
                textMaterial.SetColor("_GlowColor", glowColor);

            if (hasGlowOuter)
                textMaterial.SetFloat("_GlowOuter", glowOuter);
        }

        // เล่นเสียงหนึ่งครั้งเมื่อเมาส์เข้ามาชี้
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, soundVolume);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // คืนค่าสีข้อความเดิม
        targetText.color = originalColor;

        // คืนค่า Glow เดิม
        if (textMaterial != null)
        {
            if (hasGlowColor)
                textMaterial.SetColor("_GlowColor", originalGlowColor);

            if (hasGlowOuter)
                textMaterial.SetFloat("_GlowOuter", originalGlowOuter);
        }
    }
}
