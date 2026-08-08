using UnityEngine;

public class ButtonVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // ถ้าใช้ UI Image ให้เปลี่ยนเป็น Image แทน
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite activeSprite;

    public void SetActive(bool isActive)
    {
        spriteRenderer.sprite = isActive ? activeSprite : normalSprite;
    }
}