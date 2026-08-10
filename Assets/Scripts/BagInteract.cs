using UnityEngine;

public class BagInteract : MonoBehaviour
{
    public BagManager bagManager;

    private void OnMouseDown()
    {
        if (bagManager == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ BagManager");
            return;
        }

        bagManager.OpenBag();
    }
}