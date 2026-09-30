using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Light2DAutoToggle : MonoBehaviour
{
    [Header("Light 2D")]
    [SerializeField] private Light2D targetLight;

    [Header("ตั้งเวลา (วินาที)")]
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private float lightOnDuration = 5f;
    [SerializeField] private float lightOffDuration = 2f;

    [Header("GameObject ที่เปิด/ปิดตาม Light")]
    [SerializeField] private GameObject[] linkedObjects;

    private void Start()
    {
        if (targetLight == null)
        {
            Debug.LogError("กรุณากำหนด Light 2D ใน Inspector");
            return;
        }

        // เริ่มต้นปิดไฟ
        targetLight.enabled = false;

        // ปิด GameObject ที่ผูกไว้
        SetLinkedObjects(false);

        StartCoroutine(ToggleLightRoutine());
    }

    private IEnumerator ToggleLightRoutine()
    {
        // รอก่อนเปิดครั้งแรก
        yield return new WaitForSeconds(
            Mathf.Max(0f, startDelay)
        );

        while (true)
        {
            // =========================
            // เปิด Light
            // =========================

            targetLight.enabled = true;

            // เปิด GameObject
            SetLinkedObjects(true);

            yield return new WaitForSeconds(
                Mathf.Max(0f, lightOnDuration)
            );

            // =========================
            // ปิด Light
            // =========================

            targetLight.enabled = false;

            // ปิด GameObject
            SetLinkedObjects(false);

            yield return new WaitForSeconds(
                Mathf.Max(0f, lightOffDuration)
            );
        }
    }

    private void SetLinkedObjects(bool state)
    {
        if (linkedObjects == null)
            return;

        foreach (GameObject obj in linkedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }
}