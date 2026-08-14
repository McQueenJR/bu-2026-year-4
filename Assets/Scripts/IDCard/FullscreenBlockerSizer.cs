using UnityEngine;

public class FullscreenBlockerSizer : MonoBehaviour
{
    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = new Vector2(width, height);

        transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            transform.position.z   // คง Z ตามที่ตั้งไว้เอง
        );
    }
}