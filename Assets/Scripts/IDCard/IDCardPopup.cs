using UnityEngine;

public class IDCardPopup : MonoBehaviour
{
    public static IDCardPopup Instance;

    public GameObject popupPanel;
    public Transform spawnPointDisplay;

    private GameObject currentDisplayObj;
    private bool isShowing = false;

    void Awake()
    {
        Instance = this;
        popupPanel.SetActive(false);
    }

    void Update()
    {
        if (!isShowing) return;

        if (Input.GetMouseButtonDown(0))
        {
            // เช็คว่าคลิกโดน object ที่ spawn ไว้ไหม
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            bool clickedOnCard = hit.collider != null &&
                                 currentDisplayObj != null &&
                                 hit.collider.transform.IsChildOf(currentDisplayObj.transform);

            if (!clickedOnCard)
            {
                Hide();
            }
        }
    }

    public void Show(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ไม่ได้ใส่ Display Prefab");
            return;
        }

        if (currentDisplayObj != null)
        {
            Destroy(currentDisplayObj);
        }

        currentDisplayObj = Instantiate(prefab, spawnPointDisplay.position, Quaternion.identity);

        popupPanel.SetActive(true);
        isShowing = true;
    }

    public void Hide()
    {
        popupPanel.SetActive(false);
        isShowing = false;

        if (currentDisplayObj != null)
        {
            Destroy(currentDisplayObj);
            currentDisplayObj = null;
        }
    }
}