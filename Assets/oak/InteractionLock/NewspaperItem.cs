using UnityEngine;

// แปะที่ root ของ Prefab หนังสือพิมพ์บนโต๊ะ
// ต้องมี SpriteRenderer + BoxCollider2D บน GameObject เดียวกัน
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class NewspaperItem : MonoBehaviour
{
    [SerializeField] private float hoverScale = 1.05f;

    private NewspaperDeliveryManager manager;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private Vector3 originalScale;
    private bool interactable = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalScale = transform.localScale;
    }

    public void Setup(NewspaperDeliveryManager owner, NewspaperData data)
    {
        manager = owner;

        if (data != null && data.DeskSprite != null)
        {
            spriteRenderer.sprite = data.DeskSprite;

            // ปรับ Collider ให้พอดีกับภาพของแต่ละวัน
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            boxCollider.offset = spriteRenderer.sprite.bounds.center;
        }
    }

    public void SetInteractable(bool value)
    {
        interactable = value;

        if (!value)
            transform.localScale = originalScale;
    }

    private void OnMouseEnter()
    {
        if (!interactable) return;
        transform.localScale = originalScale * hoverScale;
    }

    private void OnMouseExit()
    {
        transform.localScale = originalScale;
    }

    private void OnMouseUpAsButton()
    {
        if (!interactable || manager == null)
            return;

        manager.RequestOpenReading();
    }
}