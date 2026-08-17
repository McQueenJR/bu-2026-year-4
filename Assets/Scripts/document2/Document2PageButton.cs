using UnityEngine;

// ติดสคริปต์นี้กับปุ่มลิสต์แต่ละอัน (GameObject ที่มี Collider หรือ Collider2D)
public class Document2PageButton : MonoBehaviour
{
    public Document2Manager manager;
    public int pageIndex; // 0 = หน้า1, 1 = หน้า2, ...

    private void OnMouseDown()
    {
        if (manager == null)
        {
            Debug.LogWarning("Document2PageButton: ยังไม่ได้ลาก manager มาใส่");
            return;
        }

        manager.OpenPage(pageIndex);
    }
}