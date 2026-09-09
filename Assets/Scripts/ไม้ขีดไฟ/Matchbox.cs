
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Matchbox : MonoBehaviour
{
    [Header("ไม้ขีดไฟ")]
    [SerializeField] private GameObject matchstick;

    [Header("ตำแหน่งไม้ขีดตอนถือ")]
    [SerializeField] private Transform matchstickHoldPosition;

    [Header("ระบบความมืด")]
    [SerializeField] private GameObject darknessOverlay;
    
    [Header("เสียงไม้ขีดไฟ")]
    [SerializeField] private AudioSource igniteAudioSource;   // เสียงไฟพุ่งขึ้น ตอนหยิบ (เล่นครั้งเดียว)
    [SerializeField] private AudioSource loopFireAudioSource; // เสียงไฟลุกวนๆ (เล่นวนหลังไฟพุ่งจบ)

    private bool holdingMatch = false;

    private SpriteRenderer boxSprite;
    
    private Coroutine fireSoundRoutine;
    
    public static Matchbox Instance { get; private set; }

    public bool IsHolding
    {
        get { return holdingMatch; }
    }
    
    
    private void Start()
    {
        Instance = this;
        
        boxSprite = GetComponent<SpriteRenderer>();

        // ซ่อนไม้ขีดตอนเริ่ม
        if (matchstick != null)
        {
            matchstick.SetActive(false);
        }

        // ปิดความมืดตอนเริ่ม
        if (darknessOverlay != null)
        {
            darknessOverlay.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (MagnifyingGlass.Instance != null && MagnifyingGlass.Instance.IsHolding) 
            return;
        
        // กันกดระหว่างมี dialog เปิดอยู่ / กำลังเรียกตำรวจ / NPC ยังไม่ถึงจุดตรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
            {
                Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
            {
                Debug.Log("มี Dialog เปิดอยู่ กดไม่ได้ตอนนี้");
                return;
            }

            if (GameManager.Instance.currentState != GameManager.NPCState.Inspecting)
            {
                Debug.Log("NPC ยังไม่ถึงจุดตรวจ หรือกำลังเดินอยู่ กดไม่ได้ตอนนี้");
                return;
            }
        }
        
        if (holdingMatch)
            return;

        TakeMatch();
    }

    private void Update()
    {
        // =====================================
        // ไม้ขีดตามเมาส์
        // =====================================

        if (holdingMatch && matchstick != null)
        {
            if (Camera.main == null)
                return;

            Vector3 mouseScreenPosition = Input.mousePosition;

            Vector3 worldPosition =
                Camera.main.ScreenToWorldPoint(
                    new Vector3(
                        mouseScreenPosition.x,
                        mouseScreenPosition.y,
                        Mathf.Abs(Camera.main.transform.position.z)
                    )
                );

            // ให้ใช้ Z เดิมของไม้ขีด
            worldPosition.z = matchstick.transform.position.z;

            matchstick.transform.position = worldPosition;
        }

        // =====================================
        // คลิกขวา = คืนไม้ขีด
        // =====================================

        if (holdingMatch && Input.GetMouseButtonDown(1))
        {
            ReturnMatch();
        }
    }

    private void TakeMatch()
    {
        // =====================================
        // 1. เปิดไม้ขีด
        // =====================================

        if (matchstick != null)
        {
            matchstick.SetActive(true);

            // =================================
            // 2. วางไม้ขีดที่จุดขวามือ
            // =================================

            if (matchstickHoldPosition != null)
            {
                matchstick.transform.position =
                    matchstickHoldPosition.position;
            }

            // =================================
            // 3. ย้ายเมาส์ไปตรงไม้ขีด
            // =================================

            if (Mouse.current != null &&
                Camera.main != null &&
                matchstickHoldPosition != null)
            {
                Vector3 screenPosition =
                    Camera.main.WorldToScreenPoint(
                        matchstickHoldPosition.position
                    );

                Mouse.current.WarpCursorPosition(
                    screenPosition
                );
            }
        }

        // =====================================
        // 4. เปิดสถานะถือ
        // =====================================

        holdingMatch = true;

        // =====================================
        // 5. ทำให้ฉากมืด
        // =====================================

        if (darknessOverlay != null)
        {
            darknessOverlay.SetActive(true);
        }

        // =====================================
        // 6. ซ่อนกล่อง
        // =====================================

        if (boxSprite != null)
        {
            boxSprite.enabled = false;
        }

        // =====================================
        // 7. แสดง NPC Anomaly
        // =====================================

        if (NPCAnomaly.CurrentNPC != null)
        {
            NPCAnomaly.CurrentNPC.ShowAnomaly();
            
        }
        
        Debug.Log(
            "ถือไม้ขีด → ไม้ขีดอยู่ขวา + เมาส์ย้ายตาม + มืด + Anomaly"
        );
        PlayMatchSound();
    }

    private void ReturnMatch()
    {
        holdingMatch = false;

        // =====================================
        // NPC กลับปกติ
        // =====================================

        if (NPCAnomaly.CurrentNPC != null)
        {
            NPCAnomaly.CurrentNPC.HideAnomaly();
        }

        // =====================================
        // ซ่อนไม้ขีด
        // =====================================

        if (matchstick != null)
        {
            matchstick.SetActive(false);
        }

        // =====================================
        // เปิดกล่องกลับ
        // =====================================

        if (boxSprite != null)
        {
            boxSprite.enabled = true;
        }

        // =====================================
        // เปิดไฟกลับ
        // =====================================

        if (darknessOverlay != null)
        {
            darknessOverlay.SetActive(false);
        }
        
        StopMatchSound();
        Debug.Log(
            "คืนไม้ขีด → กล่องกลับ + ไม้ขีดหาย + ไฟสว่าง + NPC ปกติ"
        );
    }
    private void PlayMatchSound()
    {
        if (fireSoundRoutine != null)
        {
            StopCoroutine(fireSoundRoutine);
            fireSoundRoutine = null;
        }

        if (loopFireAudioSource != null)
        {
            loopFireAudioSource.Stop();
        }

        if (igniteAudioSource != null)
        {
            igniteAudioSource.Stop();
            igniteAudioSource.Play();
        }

        fireSoundRoutine = StartCoroutine(PlayLoopFireAfterIgnite());
    }

    private IEnumerator PlayLoopFireAfterIgnite()
    {
        float waitTime = 0f;

        if (igniteAudioSource != null && igniteAudioSource.clip != null)
        {
            waitTime = igniteAudioSource.clip.length;
        }

        yield return new WaitForSeconds(waitTime);

        if (!holdingMatch)
        {
            fireSoundRoutine = null;
            yield break;
        }

        if (loopFireAudioSource != null)
        {
            loopFireAudioSource.loop = true;
            loopFireAudioSource.Play();
        }

        fireSoundRoutine = null;
    }

    private void StopMatchSound()
    {
        if (fireSoundRoutine != null)
        {
            StopCoroutine(fireSoundRoutine);
            fireSoundRoutine = null;
        }

        if (igniteAudioSource != null)
        {
            igniteAudioSource.Stop();
        }

        if (loopFireAudioSource != null)
        {
            loopFireAudioSource.Stop();
        }
    }
}
