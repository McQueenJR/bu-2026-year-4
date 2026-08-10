using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;   // ความเร็วปกติ

    private Vector3 target;
    private bool moving = false;
    private float currentSpeed;

    public void MoveTo(Vector3 destination, float? speedOverride = null)
    {
        target = destination;
        moving = true;
        currentSpeed = speedOverride ?? speed;   // ใช้ค่าที่ส่งมา ถ้าไม่ส่งมาใช้ speed ปกติ
    }

    void Update()
    {
        if (!moving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            moving = false;

            if (GameManager.Instance.currentState == GameManager.NPCState.WalkingToCheckpoint)
            {
                GameManager.Instance.NPCReachedCheckpoint(gameObject);
            }
        }
    }

    public bool IsMoving()
    {
        return moving;
    }
}