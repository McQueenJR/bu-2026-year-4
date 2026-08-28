using UnityEngine;

// ติดกับ GameObject โปร่งใสเต็มจอ อยู่หลังสุด (ต้องมี Collider2D ครอบเต็มจอ)
public class Document1Blocker : MonoBehaviour
{
    public Document1Manager manager;

    private void OnMouseDown()
    {
        if (manager == null)
        {
            Debug.LogWarning("Document1Blocker: ยังไม่ได้ลาก manager มาใส่");
            return;
        }

        manager.CloseDocument();
    }
}