using UnityEngine;

public class Document1RowButton : Document1ChildBase
{
    public Document1Manager manager;

    [Tooltip("ต้องตรงกับลำดับ (index) ของ rows[] ใน Document1Manager เช่น A1=0, A2=1, A3=2, A4=3")]
    public int rowIndex;

    protected override void OnClick()
    {
        if (manager == null)
        {
            Debug.LogWarning("Document1RowButton: ยังไม่ได้ลาก manager มาใส่");
            return;
        }

        manager.GoToRow(rowIndex);
    }
}