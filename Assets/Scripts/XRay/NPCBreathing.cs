using UnityEngine;

public class NPCBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]

    [Tooltip("ขนาดที่ NPC ขยายตอนหายใจเข้า")]
    [SerializeField] private float breathingAmount = 0.015f;

    [Tooltip("ความเร็วในการหายใจ")]
    [SerializeField] private float breathingSpeed = 1.5f;

    [Tooltip("ให้หายใจแบบนุ่มนวล")]
    [SerializeField] private bool smoothBreathing = true;

    private Vector3 originalScale;

    private void Start()
    {
        // จำ Scale เดิมของ NPC
        originalScale = transform.localScale;
    }

    private void Update()
    {
        Breathe();
    }

    private void Breathe()
    {
        float breath;

        if (smoothBreathing)
        {
            // หายใจแบบนุ่มนวล
            breath =
                (Mathf.Sin(Time.time * breathingSpeed) + 1f)
                / 2f;
        }
        else
        {
            // หายใจแบบธรรมดา
            breath =
                Mathf.Sin(Time.time * breathingSpeed);
        }

        float scaleAmount =
            breath * breathingAmount;

        transform.localScale =
            originalScale +
            new Vector3(
                scaleAmount,
                scaleAmount,
                0f
            );
    }
}