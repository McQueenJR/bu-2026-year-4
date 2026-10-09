using System;
using UnityEngine;

// ภาพหนังสือพิมพ์ใหญ่ (World Space) — เปิดโดย NewspaperDeliveryManager
// ปิดด้วยคลิกขวา ไม่มีระบบลาก
public class NewspaperReaderPopup : MonoBehaviour
{
    [Header("Renderer ของภาพกระดาษ (ลูกของ object นี้)")]
    [SerializeField] private SpriteRenderer paperRenderer;

    [SerializeField] private int sortingOrder = 500;

    [Header("ปรับขนาดอัตโนมัติ (0 = ใช้ขนาดตาม Sprite/Scale ที่ตั้งไว้)")]
    [Tooltip("ความสูงเป็นหน่วยโลก แนะนำ ≈ Camera Orthographic Size × 2 × 0.9")]
    [SerializeField] private float fitHeight = 0f;

    private Action onClosed;
    private int openedFrame = -1;

    public bool IsOpen { get; private set; }

    public void Open(Sprite sprite, Action closedCallback)
    {
        if (IsOpen || paperRenderer == null || sprite == null)
            return;

        paperRenderer.sprite = sprite;
        paperRenderer.sortingOrder = sortingOrder;

        if (fitHeight > 0f)
        {
            float spriteHeight = sprite.bounds.size.y;

            if (spriteHeight > 0.0001f)
            {
                float scale = fitHeight / spriteHeight;
                paperRenderer.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        onClosed = closedCallback;
        IsOpen = true;
        openedFrame = Time.frameCount;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Time.frameCount == openedFrame) return;

        // คลิกขวา → ปิด
        if (Input.GetMouseButtonDown(1))
            Close();
    }

    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        gameObject.SetActive(false);

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }
}