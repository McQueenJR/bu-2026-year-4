

using UnityEngine;

public class MatchStickFollowMouse : MonoBehaviour
{
    [Header("ตำแหน่ง Offset จากเมาส์")]
    public Vector3 offset = Vector3.zero;

    private bool followMouse = false;

    public void StartFollowingMouse()
    {
        followMouse = true;
    }

    public void StopFollowingMouse()
    {
        followMouse = false;
    }

    private void Update()
    {
        if (!followMouse)
            return;

        if (Camera.main == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            Mathf.Abs(Camera.main.transform.position.z);

        Vector3 worldPosition =
            Camera.main.ScreenToWorldPoint(mousePosition);

        transform.position = worldPosition + offset;
    }
}

