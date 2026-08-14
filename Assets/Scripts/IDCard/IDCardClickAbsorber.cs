using UnityEngine;

public class IDCardClickAbsorber : MonoBehaviour
{
    private void OnMouseDown()
    {
        // ไม่ต้องทำอะไร แค่มี method นี้เพื่อให้การ์ด "รับ" คลิกไว้เอง
        // ป้องกันไม่ให้คลิกไหลไปโดน Blocker หรือของด้านหลัง
    }
}