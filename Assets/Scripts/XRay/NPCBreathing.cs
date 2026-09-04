using UnityEngine;

public class NPCBreathing : MonoBehaviour

{

    [Header("Breathing Settings")]

    [Tooltip("ขนาดที่ NPC ขยายตอนหายใจเข้า")]

    [SerializeField] private float breathingAmount = 0.015f;

    [Tooltip("ความเร็วในการหายใจ")]

    [SerializeField] private float breathingSpeed = 1.5f;

    [Tooltip("ดีเลย์/ความเหลื่อมของจังหวะหายใจ (ตั้งค่าต่างกันเพื่อให้แต่ละตัวหายใจไม่พร้อมกัน)")]

    [SerializeField] private float breathingDelay = 0f; // <--- เพิ่มดีเลย์ตรงนี้ครับ

    [Tooltip("ให้หายใจแบบนุ่มนวล")]

    [SerializeField] private bool smoothBreathing = true;

    private Vector3 originalScale;

    // Event function

    private void Start()

    {

        // ค่า Scale เดิมของ NPC

        originalScale = transform.localScale;

    }

    // Event function

    private void Update()

    {

        Breathe();

    }

    // Frequently called

    private void Breathe()

    {

        float breath;

        // คำนวณเวลาโดยบวก breathingDelay เข้าไป

        float timeWithDelay = Time.time + breathingDelay;

        if (smoothBreathing)

        {

            // หายใจแบบนุ่มนวล

            breath = (Mathf.Sin(timeWithDelay * breathingSpeed) + 1f) / 2f;

        }

        else

        {

            // หายใจแบบธรรมดา

            breath = Mathf.Sin(timeWithDelay * breathingSpeed);

        }

        float scaleAmount = breath * breathingAmount;

        transform.localScale = originalScale + new Vector3(

            scaleAmount,

            scaleAmount,

            0f

        );

    }

}