
using UnityEngine;
using UnityEngine.EventSystems;

public class MagnifyingGlass : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    private bool isHolding;
    private Vector3 tablePosition;

    public bool IsHolding
    {
        get { return isHolding; }
    }

    private XRaySystem xraySystem;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        tablePosition = transform.position;

        // หา XRaySystem ใน Scene
        xraySystem =
            FindFirstObjectByType<XRaySystem>();
    }

    private void Update()
    {
        if (!isHolding)
            return;

        FollowMouse();

        // คลิกขวา = วางแว่น
        if (Input.GetMouseButtonDown(1))
        {
            PutDown();
        }
    }

    private void OnMouseDown()
    {
        // ถ้าเมาส์อยู่บน UI ไม่ต้องหยิบ
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isHolding)
            return;

        PickUp();
    }

    private void PickUp()
    {
        isHolding = true;

        Debug.Log("หยิบแว่นขยายแล้ว");
    }

    private void FollowMouse()
    {
        if (mainCamera == null)
            return;

        Vector3 mousePosition =
            Input.mousePosition;

        mousePosition.z =
            Mathf.Abs(
                mainCamera.transform.position.z
            );

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                mousePosition
            );

        transform.position =
            new Vector3(
                worldPosition.x,
                worldPosition.y,
                tablePosition.z
            );
    }

    private void PutDown()
    {
        isHolding = false;

        // คืนตำแหน่งแว่น
        transform.position =
            tablePosition;

        // รีเซ็ต X-Ray กลับ Default
        if (xraySystem != null)
        {
            xraySystem.ResetXRay();
        }

        Debug.Log("วางแว่นขยายแล้ว + รีเซ็ต X-Ray");
    }
}

