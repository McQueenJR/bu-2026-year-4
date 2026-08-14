using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// ทำให้จอตัวเลขนาฬิกากระพริบแบบนาฬิกาดิจิตอลเก่าๆ ไม่เสถียร
/// - กระพริบสุ่มทุกๆ 2-5 วิ นาน 0.1-0.5 วิ
/// - กระพริบตอนเวลาเปลี่ยน (เรียกผ่าน TriggerFlicker() จาก ClockManager)
/// ใส่สคริปต์นี้ไว้บน GameObject เดียวกับ TextMeshPro (3D) ของตัวเลข
/// </summary>
public class ClockFlicker : MonoBehaviour
{
    [Header("Reference")]
    public TMP_Text timeText;

    [Header("Random Flicker (Idle)")]
    [Tooltip("ช่วงเวลาสุ่มระหว่างการกระพริบแต่ละครั้ง (วินาที)")]
    public float minInterval = 2f;
    public float maxInterval = 5f;

    [Tooltip("ระยะเวลาที่กระพริบแต่ละครั้ง (วินาที)")]
    public float minFlickerDuration = 0.1f;
    public float maxFlickerDuration = 0.5f;

    [Tooltip("ความเร็วในการติด/ดับระหว่างกระพริบ (วินาทีต่อ 1 toggle) ยิ่งน้อยยิ่งกระพริบถี่")]
    public float toggleSpeed = 0.04f;

    [Header("Glitch Look (Optional)")]
    [Tooltip("ถ้าเปิด จะโชว์ตัวเลขสุ่ม/ขีดๆ แทนเวลาจริงระหว่างกระพริบ ก่อนกลับมาโชว์เวลาถูกต้อง")]
    public bool showGlitchDigits = true;

    [Tooltip("ตัวอักษรที่จะสุ่มมาโชว์ตอน glitch")]
    public string glitchChars = "0123456789:-. ";

    private Coroutine idleFlickerRoutine;
    private Coroutine activeFlickerRoutine;
    private string realText;

    void Awake()
    {
        if (timeText == null)
            timeText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        idleFlickerRoutine = StartCoroutine(IdleFlickerLoop());
    }

    void OnDisable()
    {
        if (idleFlickerRoutine != null)
            StopCoroutine(idleFlickerRoutine);

        if (activeFlickerRoutine != null)
            StopCoroutine(activeFlickerRoutine);

        // กันเหนียว: กลับมาเปิดข้อความให้เห็นเสมอตอนปิดสคริปต์
        if (timeText != null)
        {
            timeText.enabled = true;
            if (!string.IsNullOrEmpty(realText))
                timeText.text = realText;
        }
    }

    void Update()
    {
        // เก็บ text ล่าสุดไว้ เผื่อ ClockManager เปลี่ยนค่าระหว่างที่ยังไม่ได้กระพริบ
        if (timeText != null && !IsFlickering())
            realText = timeText.text;
    }

    // =========================
    // สุ่มกระพริบเป็นระยะๆ (เหมือนจอเก่าเดี๋ยวๆ ก็สะดุด)
    // =========================
    private IEnumerator IdleFlickerLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            yield return DoFlicker();
        }
    }

    // =========================
    // เรียกจากภายนอก (เช่น ClockManager ตอนเวลาเปลี่ยน)
    // =========================
    public void TriggerFlicker()
    {
        if (activeFlickerRoutine != null)
            StopCoroutine(activeFlickerRoutine);

        activeFlickerRoutine = StartCoroutine(DoFlicker());
    }

    private bool IsFlickering()
    {
        return activeFlickerRoutine != null;
    }

    private IEnumerator DoFlicker()
    {
        activeFlickerRoutine = null; // เผื่อถูกเรียกจาก IdleFlickerLoop เอง ไม่ต้องเก็บ handle ซ้ำ

        if (timeText == null)
            yield break;

        realText = timeText.text;

        float duration = Random.Range(minFlickerDuration, maxFlickerDuration);
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            visible = !visible;
            timeText.enabled = visible;

            if (visible && showGlitchDigits)
            {
                // โชว์ตัวเลขมั่วๆ แว้บนึงระหว่างกระพริบ ให้ดูเหมือนจอรวน
                timeText.text = RandomGlitchString(realText.Length);
            }

            yield return new WaitForSeconds(toggleSpeed);
            elapsed += toggleSpeed;
        }

        // จบกระพริบ กลับมาปกติเสมอ
        timeText.enabled = true;
        timeText.text = realText;
    }

    private string RandomGlitchString(int length)
    {
        if (length <= 0) length = 5;

        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = glitchChars[Random.Range(0, glitchChars.Length)];

        return new string(chars);
    }
}