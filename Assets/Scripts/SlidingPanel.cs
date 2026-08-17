using UnityEngine;
using System.Collections;

public class SlidingPanel : MonoBehaviour
{
    public Transform panel;       // เปลี่ยนจาก RectTransform → Transform
    public Vector3 originalPos;   // เปลี่ยนจาก Vector2 → Vector3
    public Vector3 offScreenOffset = new Vector3(-8f, 0f, 0f); // ระยะเลื่อนไปทางซ้าย (หน่วย world unit ไม่ใช่ pixel)
    public float slideSpeed = 5f;

    [Header("Sound")]
    public AudioSource slideSound;

    private Coroutine slideRoutine;

    void Start()
    {
        originalPos = panel.position;
    }

    public void SlideOut(System.Action onComplete = null)
    {
        if (slideSound != null) slideSound.Play();

        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideTo(originalPos + offScreenOffset, onComplete));
    }

    public void SlideBack(System.Action onComplete = null)
    {
        if (slideSound != null) slideSound.Play();

        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideTo(originalPos, onComplete));
    }

    private IEnumerator SlideTo(Vector3 target, System.Action onComplete)
    {
        while (Vector3.Distance(panel.position, target) > 0.05f)
        {
            panel.position = Vector3.Lerp(panel.position, target, Time.deltaTime * slideSpeed);
            yield return null;
        }

        panel.position = target;
        onComplete?.Invoke();
    }
}