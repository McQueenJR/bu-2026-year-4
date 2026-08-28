using UnityEngine;

public class Document1NavButton : MonoBehaviour
{
    public Document1Manager manager;
    public bool isNext = true;

    private void OnMouseDown()
    {
        if (manager == null)
        {
            Debug.LogWarning("Document1NavButton: ยังไม่ได้ลาก manager มาใส่");
            return;
        }

        if (isNext)
            manager.NextPage();
        else
            manager.PrevPage();
    }
}