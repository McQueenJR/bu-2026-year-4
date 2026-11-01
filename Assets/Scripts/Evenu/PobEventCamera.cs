
using System.Collections;
using UnityEngine;

public class PobEventCamera : MonoBehaviour
{
    [Header("Mouse Look")]
    public bool enableMouseLook = true;
    public float mouseSensitivity = 2f;

    [Tooltip("ขอบเขตการหมุนแนวนอน")]
    public float minYaw = -60f;
    public float maxYaw = 60f;

    [Tooltip("ขอบเขตการหมุนแนวตั้ง")]
    public float minPitch = -35f;
    public float maxPitch = 35f;

    [Header("Pob Face Target")]
    public Transform faceTarget;
    public float zoomDuration = 2f;
    public float zoomDistance = 0.5f;
    public float zoomRotationSpeed = 3f;

    [Header("Camera Shake")]
    public float startShakeStrength = 0.01f;
    public float maxShakeStrength = 0.12f;
    public float startShakeSpeed = 10f;
    public float maxShakeSpeed = 35f;

    [Header("Event Audio")]
    public AudioSource eventAudioSource;
    public AudioClip eventSound;

    private float yaw;
    private float pitch;

    private Vector3 normalPosition;
    private Quaternion normalRotation;
    private Vector3 shakeOffset;

    private bool eventRunning;
    private bool eventFinished;
    private Coroutine eventCoroutine;

    public bool EventRunning => eventRunning;
    public bool EventFinished => eventFinished;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = NormalizeAngle(angles.x);

        normalPosition = transform.position;
        normalRotation = transform.rotation;
    }

    private void Update()
    {
        if (!eventRunning && !eventFinished)
        {
            HandleMouseLook();
        }
        else if (eventRunning)
        {
            // ยังให้ผู้เล่นขยับเมาส์ได้ระหว่างอีเวนต์
            HandleMouseLook();
        }
    }

    private void LateUpdate()
    {
        if (eventRunning)
        {
            transform.position += shakeOffset;
        }
    }

    private void HandleMouseLook()
    {
        if (!enableMouseLook)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw = Mathf.Clamp(yaw + mouseX, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        if (!eventRunning)
        {
            transform.rotation = targetRotation;
            normalRotation = targetRotation;
        }
        else
        {
            // ขณะซูม เมาส์ยังปรับมุมได้ แต่ไม่เกินขอบเขต
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * zoomRotationSpeed
            );
        }
    }

    public void StartPobEvent()
    {
        if (eventRunning || eventFinished)
            return;

        if (faceTarget == null)
        {
            Debug.LogError(
                "PobEventCamera: กรุณากำหนด Face Target ใน Inspector"
            );
            return;
        }

        eventCoroutine = StartCoroutine(PobEventSequence());
    }

    private IEnumerator PobEventSequence()
    {
        eventRunning = true;

        normalPosition = transform.position;
        normalRotation = transform.rotation;

        if (eventAudioSource != null && eventSound != null)
        {
            eventAudioSource.clip = eventSound;
            eventAudioSource.loop = true;
            eventAudioSource.Play();
        }

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 direction = (faceTarget.position - startPosition).normalized;
        Vector3 targetPosition =
            faceTarget.position - direction * zoomDistance;

        Quaternion targetRotation = Quaternion.LookRotation(
            faceTarget.position - targetPosition
        );

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / zoomDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // ซูมเข้าใบหน้า
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                smoothT
            );

            // หันเข้าหาใบหน้า
            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                smoothT
            );

            // การสั่นค่อย ๆ แรงและเร็วขึ้น
            float strength = Mathf.Lerp(
                startShakeStrength,
                maxShakeStrength,
                smoothT
            );

            float shakeSpeed = Mathf.Lerp(
                startShakeSpeed,
                maxShakeSpeed,
                smoothT
            );

            shakeOffset = new Vector3(
                (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f)
                    * 2f * strength,
                (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f)
                    * 2f * strength,
                0f
            );

            yield return null;
        }

        // จบการซูมและหยุดสั่น
        shakeOffset = Vector3.zero;

        if (eventAudioSource != null)
            eventAudioSource.Stop();

        eventRunning = false;
        eventFinished = true;
        eventCoroutine = null;

        Debug.Log("Pob camera event finished.");
    }

    public void ResetCameraEvent()
    {
        if (eventCoroutine != null)
            StopCoroutine(eventCoroutine);

        if (eventAudioSource != null)
            eventAudioSource.Stop();

        transform.position = normalPosition;
        transform.rotation = normalRotation;

        shakeOffset = Vector3.zero;
        eventRunning = false;
        eventFinished = false;
        eventCoroutine = null;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
