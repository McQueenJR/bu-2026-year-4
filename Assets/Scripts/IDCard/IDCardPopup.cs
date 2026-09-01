using UnityEngine;

public class IDCardPopup : MonoBehaviour
{
    public static IDCardPopup Instance;

    public GameObject popupPanel;
    public Transform spawnPointDisplay;
    
    [Header("กันการ์ดลากออกนอกจอ")]
    public BoxCollider2D cardDragBoundary;

    private GameObject currentDisplayObj;

    void Awake()
    {
        Instance = this;

        popupPanel.SetActive(false);
    }

    // เปิด Display
    public void Show(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ไม่ได้ใส่ Display Prefab");
            return;
        }

        if (currentDisplayObj != null)
            Destroy(currentDisplayObj);

        currentDisplayObj = Instantiate(
            prefab,
            spawnPointDisplay.position,
            Quaternion.identity
        );
        
        IDCardDisplayClick dragScript = currentDisplayObj.GetComponent<IDCardDisplayClick>();
        if (dragScript != null && cardDragBoundary != null)
        {
            dragScript.SetDragBoundary(cardDragBoundary);
        }

        popupPanel.SetActive(true);
        
        DraggableSortOrder.NotifyOpened();
    }

    // ปิด Display
    public void Hide()
    {
        popupPanel.SetActive(false);

        if (currentDisplayObj != null)
        {
            Destroy(currentDisplayObj);
            currentDisplayObj = null;
        }
        DraggableSortOrder.NotifyClosed(); 
    }
}