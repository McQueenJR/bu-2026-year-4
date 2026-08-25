using UnityEngine;

public class BagDisplayClick : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        // คลิกซ้าย → เริ่มลาก
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;

            Vector3 mouseWorldPos =
                mainCamera.ScreenToWorldPoint(
                    Input.mousePosition
                );

            mouseWorldPos.z = transform.position.z;

            offset =
                transform.position - mouseWorldPos;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Vector3 mouseWorldPos =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        mouseWorldPos.z = transform.position.z;

        transform.position =
            mouseWorldPos + offset;
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void OnMouseOver()
    {
        // คลิกขวา → ปิดกระเป๋า
        if (Input.GetMouseButtonDown(1))
        {
            if (BagInventoryUI.Instance != null)
            {
                BagInventoryUI.Instance.CloseInventory();
            }
        }
    }
}