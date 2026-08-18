using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;          // ความเร็วปกติ

    [Header("Walking Bounce")]
    public float bounceHeight = 0.05f; // ความสูงที่เด้งขึ้นลง
    public float bounceSpeed = 8f;      // ความเร็วของการเด้ง

    private Vector3 target;
    private bool moving = false;
    private float currentSpeed;

    private float baseY;
    private float bounceTime;

    void Start()
    {
        baseY = transform.position.y;
    }

    public void MoveTo(Vector3 destination, float? speedOverride = null)
    {
        target = destination;
        moving = true;
        currentSpeed = speedOverride ?? speed;

        // จำระดับ Y ปัจจุบันไว้
        baseY = transform.position.y;

        // เริ่มจังหวะเดินใหม่
        bounceTime = 0f;
    }

    void Update()
    {
        if (!moving) return;

        // =========================
        // เดินไปยังเป้าหมาย
        // =========================
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            currentSpeed * Time.deltaTime
        );

        // =========================
        // ขยับขึ้นลงเหมือนกำลังเดิน
        // =========================
        bounceTime += Time.deltaTime * bounceSpeed;

        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin(bounceTime) * bounceHeight;
        transform.position = pos;

        // =========================
        // ถึงเป้าหมาย
        // =========================
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            moving = false;

            // กลับมาอยู่ระดับปกติ
            Vector3 finalPos = transform.position;
            finalPos.y = target.y;
            transform.position = finalPos;

            if (GameManager.Instance.currentState ==
                GameManager.NPCState.WalkingToCheckpoint)
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