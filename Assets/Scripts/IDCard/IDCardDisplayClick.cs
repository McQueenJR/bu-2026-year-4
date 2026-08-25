using UnityEngine;

public class IDCardDisplayClick : MonoBehaviour
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

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

            mouseWorldPos.z = transform.position.z;

            // ทำให้ตอนเริ่มลาก Display ไม่กระโดด
            offset = transform.position - mouseWorldPos;
        }
    }

    private void OnMouseDrag()
    {
        // กำลังลากอยู่
        if (isDragging)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

            mouseWorldPos.z = transform.position.z;

            transform.position = mouseWorldPos + offset;
        }
    }

    private void OnMouseUp()
    {
        // ปล่อยเมาส์ซ้าย → หยุดลาก
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void OnMouseOver()
    {
        // คลิกขวา → ปิด Display
        if (Input.GetMouseButtonDown(1))
        {
            IDCardPopup.Instance.Hide();
        }
    }
}