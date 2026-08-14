using UnityEngine;

public class IDCardPopup : MonoBehaviour
{
    public static IDCardPopup Instance;

    public GameObject popupPanel;
    public Transform spawnPointDisplay;

    [Header("Blocker (กันคลิกทะลุ)")]
    public GameObject blocker;   // ลาก IDCardBlocker มาใส่

    private GameObject currentDisplayObj;

    void Awake()
    {
        Instance = this;

        popupPanel.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);
    }

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

        popupPanel.SetActive(true);

        if (blocker != null)
            blocker.SetActive(true);
    }

    public void Hide()
    {
        popupPanel.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        if (currentDisplayObj != null)
        {
            Destroy(currentDisplayObj);
            currentDisplayObj = null;
        }
    }
}