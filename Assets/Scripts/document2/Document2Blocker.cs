using UnityEngine;

// ติดสคริปต์นี้กับ GameObject ที่ทำหน้าที่เป็น Blocker (วางอยู่หลังสุด, Collider ครอบเต็มจอ)
public class Document2Blocker : MonoBehaviour
{
    public Document2Manager manager;

    private void OnMouseDown()
    {
        if (manager == null)
        {
            Debug.LogWarning("Document2Blocker: ยังไม่ได้ลาก manager มาใส่");
            return;
        }

        manager.CloseDocument();
    }
}