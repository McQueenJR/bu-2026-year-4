using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MagnifyingGlass : MonoBehaviour
{
    [Header("Magic Sound")]
    [SerializeField] private AudioSource magicSound;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Visual")]
    [SerializeField] private GameObject storedVisual;
    [SerializeField] private GameObject heldVisual;

    [Header("Held Visual Sprites")]
    [Tooltip("ภาพแว่นปกติ")]
    [SerializeField] private Sprite normalSprite;

    [Tooltip("ภาพแว่นเรืองแสง Level 2")]
    [SerializeField] private Sprite xray2Sprite;

    [Tooltip("ภาพแว่นเรืองแสง Level 3")]
    [SerializeField] private Sprite xray3Sprite;

    [Header("Magic Glow")]
    [Tooltip("ลาก SpriteRenderer ของ GlowSprite มาใส่ตรงนี้")]
    [SerializeField] private SpriteRenderer glowSpriteRenderer;

    [Tooltip("เวลาที่ใช้ในการ Fade Glow")]
    [SerializeField] private float glowFadeDuration = 0.5f;

    [Header("Guide UI")]
    [SerializeField] private GameObject guideUI;

    private SpriteRenderer heldSpriteRenderer;

    private Coroutine glowCoroutine;

    private bool isHolding;

    private Vector3 tablePosition;

    private XRaySystem xraySystem;

    public bool IsHolding
    {
        get { return isHolding; }
    }

    // ให้ XRaySystem เข้าถึง HeldVisual
    public GameObject HeldVisual
    {
        get { return heldVisual; }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // จำตำแหน่งตอนวางอยู่บนโต๊ะ
        tablePosition = transform.position;

        // หา XRaySystem
        xraySystem = FindFirstObjectByType<XRaySystem>();

        isHolding = false;


        // =====================================================
        // Stored Visual
        // =====================================================

        if (storedVisual != null)
        {
            storedVisual.SetActive(true);
        }


        // =====================================================
        // Held Visual
        // =====================================================

        if (heldVisual != null)
        {
            heldVisual.SetActive(false);

            heldSpriteRenderer =
                heldVisual.GetComponent<SpriteRenderer>();

            if (heldSpriteRenderer == null)
            {
                Debug.LogError(
                    "❌ MagnifyingGlass: ไม่พบ SpriteRenderer ใน HeldVisual"
                );
            }
        }


        // =====================================================
        // Glow Sprite
        // =====================================================

        if (glowSpriteRenderer != null)
        {
            Color color = glowSpriteRenderer.color;

            // เริ่มต้นให้ Glow โปร่งใส
            color.a = 0f;

            glowSpriteRenderer.color = color;

            // ยังไม่ใส่ Sprite
            glowSpriteRenderer.sprite = null;
        }
        else
        {
            Debug.LogError(
                "❌ MagnifyingGlass: ยังไม่ได้ใส่ Glow Sprite Renderer!"
            );
        }


        // =====================================================
        // Guide UI
        // =====================================================

        if (guideUI != null)
        {
            guideUI.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isHolding)
            return;


        // ให้แว่นตามเมาส์
        FollowMouse();


        // คลิกขวา = วางแว่น
        if (Input.GetMouseButtonDown(1))
        {
            PutDown();
        }
    }


    // =========================================================
    // CLICK MAGNIFYING GLASS
    // =========================================================

    private void OnMouseDown()
    {
        // กันกดระหว่างมี dialog เปิดอยู่ หรือกำลังเรียกตำรวจ
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isPoliceSequenceActive)
                return;

            if (GameManager.Instance.dialogManager != null &&
                GameManager.Instance.dialogManager.IsDialogOpen())
                return;
        }
        
        // ถ้าคลิก UI อยู่ ไม่ให้หยิบแว่น
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }


        // ถ้าถืออยู่แล้ว ไม่ต้องหยิบซ้ำ
        if (isHolding)
            return;


        PickUp();
    }


    // =========================================================
    // PICK UP
    // =========================================================

    private void PickUp()
    {
        isHolding = true;


        // ซ่อนแว่นบนโต๊ะ
        if (storedVisual != null)
        {
            storedVisual.SetActive(false);
        }


        // แสดงแว่นที่ถือ
        if (heldVisual != null)
        {
            heldVisual.SetActive(true);
        }


        // เริ่มต้นที่ Level 1
        SetHeldSprite(0);


        // แสดง Guide UI
        if (guideUI != null)
        {
            guideUI.SetActive(true);
        }


        Debug.Log("🔍 หยิบแว่นขยายแล้ว");
    }


    // =========================================================
    // FOLLOW MOUSE
    // =========================================================

    private void FollowMouse()
    {
        if (mainCamera == null)
            return;


        Vector3 mousePosition =
            Input.mousePosition;


        mousePosition.z =
            Mathf.Abs(
                mainCamera.transform.position.z
            );


        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                mousePosition
            );


        transform.position =
            new Vector3(
                worldPosition.x,
                worldPosition.y,
                tablePosition.z
            );
    }


    // =========================================================
    // PUT DOWN
    // =========================================================

    private void PutDown()
    {
        isHolding = false;


        // กลับตำแหน่งโต๊ะ
        transform.position = tablePosition;


        // ซ่อน Held Visual
        if (heldVisual != null)
        {
            heldVisual.SetActive(false);
        }


        // แสดง Stored Visual
        if (storedVisual != null)
        {
            storedVisual.SetActive(true);
        }


        // ซ่อน Guide UI
        if (guideUI != null)
        {
            guideUI.SetActive(false);
        }


        // Reset X-Ray
        if (xraySystem != null)
        {
            xraySystem.ResetXRay();
        }


        Debug.Log(
            "🔍 วางแว่นแล้ว + Reset X-Ray"
        );
    }


    // =========================================================
    // SET HELD SPRITE
    // =========================================================

    public void SetHeldSprite(int level)
    {
        if (heldSpriteRenderer == null)
            return;


        switch (level)
        {
            // =================================================
            // LEVEL 1
            // =================================================

            case 0:

                // ให้แว่นปกติแสดงอยู่ตลอด
                heldSpriteRenderer.sprite =
                    normalSprite;


                // Fade Glow ออก
                FadeGlow(
                    null,
                    0f
                );


                // หยุดเสียง
                StopMagicSound();

                break;


            // =================================================
            // LEVEL 2
            // =================================================

            case 1:

                // แว่นปกติยังอยู่ด้านล่าง
                heldSpriteRenderer.sprite =
                    normalSprite;


                // แสดง Glow Level 2
                FadeGlow(
                    xray2Sprite,
                    1f
                );


                // เริ่มเสียง
                StartMagicSound();

                break;


            // =================================================
            // LEVEL 3
            // =================================================

            case 2:

                // แว่นปกติยังอยู่ด้านล่าง
                heldSpriteRenderer.sprite =
                    normalSprite;


                // แสดง Glow Level 3
                FadeGlow(
                    xray3Sprite,
                    1f
                );


                // เริ่มเสียง
                StartMagicSound();

                break;
        }
    }


    // =========================================================
    // FADE GLOW
    // =========================================================

    private void FadeGlow(
        Sprite newSprite,
        float targetAlpha
    )
    {
        if (glowSpriteRenderer == null)
            return;


        // หยุด Coroutine เก่าก่อน
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }


        // เริ่ม Fade ใหม่
        glowCoroutine =
            StartCoroutine(
                FadeGlowCoroutine(
                    newSprite,
                    targetAlpha
                )
            );
    }


    // =========================================================
    // FADE GLOW COROUTINE
    // =========================================================

    private IEnumerator FadeGlowCoroutine(
        Sprite newSprite,
        float targetAlpha
    )
    {
        // ถ้ามี Sprite ใหม่
        // ให้เปลี่ยน Sprite ก่อนเริ่ม Fade
        if (newSprite != null)
        {
            glowSpriteRenderer.sprite =
                newSprite;
        }


        // Alpha ปัจจุบัน
        float startAlpha =
            glowSpriteRenderer.color.a;


        float time = 0f;


        // ป้องกันกรณี Duration = 0
        if (glowFadeDuration <= 0f)
        {
            Color instantColor =
                glowSpriteRenderer.color;

            instantColor.a =
                targetAlpha;

            glowSpriteRenderer.color =
                instantColor;

            if (targetAlpha <= 0f)
            {
                glowSpriteRenderer.sprite = null;
            }

            glowCoroutine = null;

            yield break;
        }


        // =====================================================
        // FADE
        // =====================================================

        while (time < glowFadeDuration)
        {
            time += Time.deltaTime;


            float t =
                time /
                glowFadeDuration;


            t = Mathf.Clamp01(t);


            // ทำให้ Fade นุ่มนวล
            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            // คำนวณ Alpha
            float alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );


            Color color =
                glowSpriteRenderer.color;


            color.a =
                alpha;


            glowSpriteRenderer.color =
                color;


            yield return null;
        }


        // =====================================================
        // FINAL VALUE
        // =====================================================

        Color finalColor =
            glowSpriteRenderer.color;


        finalColor.a =
            targetAlpha;


        glowSpriteRenderer.color =
            finalColor;


        // ถ้า Fade จนหาย
        if (targetAlpha <= 0f)
        {
            glowSpriteRenderer.sprite = null;
        }


        glowCoroutine = null;
    }


    // =========================================================
    // MAGIC SOUND
    // =========================================================

    private void StartMagicSound()
    {
        if (magicSound == null)
            return;


        // ถ้าเสียงกำลังเล่นอยู่
        // ไม่ต้องเริ่มใหม่
        if (!magicSound.isPlaying)
        {
            magicSound.Play();
        }
    }


    // =========================================================
    // STOP MAGIC SOUND
    // =========================================================

    private void StopMagicSound()
    {
        if (magicSound == null)
            return;


        if (magicSound.isPlaying)
        {
            magicSound.Stop();
        }
    }
}