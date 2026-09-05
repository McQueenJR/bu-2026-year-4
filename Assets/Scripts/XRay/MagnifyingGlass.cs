using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite xray2Sprite;
    [SerializeField] private Sprite xray3Sprite;
    
    [Header("Guide UI")]
    [SerializeField] private GameObject guideUI;
    
    private SpriteRenderer heldSpriteRenderer;

    private bool isHolding;
    private Vector3 tablePosition;

    public bool IsHolding
    {
        get { return isHolding; }
    }

    // ให้ XRaySystem เข้าถึง HeldVisual
    public GameObject HeldVisual
    {
        get { return heldVisual; }
    }

    private XRaySystem xraySystem;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        tablePosition = transform.position;

        xraySystem = FindFirstObjectByType<XRaySystem>();

        isHolding = false;

        if (storedVisual != null)
            storedVisual.SetActive(true);

        if (heldVisual != null)
        {
            heldVisual.SetActive(false);

            heldSpriteRenderer =
                heldVisual.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (!isHolding)
            return;

        FollowMouse();

        // คลิกขวา = วางแว่น
        if (Input.GetMouseButtonDown(1))
        {
            PutDown();
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isHolding)
            return;

        PickUp();
    }

    private void PickUp()
    {
        isHolding = true;

        if (storedVisual != null)
            storedVisual.SetActive(false);

        if (heldVisual != null)
            heldVisual.SetActive(true);

        SetHeldSprite(0);

        if (guideUI != null)
            guideUI.SetActive(true);

        Debug.Log("หยิบแว่นขยายแล้ว");
    }

    private void FollowMouse()
    {
        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

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

    private void PutDown()
    {
        isHolding = false;

        transform.position = tablePosition;

        if (heldVisual != null)
            heldVisual.SetActive(false);

        if (storedVisual != null)
            storedVisual.SetActive(true);

        if (guideUI != null)
            guideUI.SetActive(false);

        if (xraySystem != null)
        {
            xraySystem.ResetXRay();
        }

        Debug.Log("วางแว่นแล้ว + รีเซ็ต X-Ray");
    }
    public void SetHeldSprite(int level)
    {
        if (heldSpriteRenderer == null)
            return;

        switch (level)
        {
            case 0:
                // รูปแว่นปกติ
                heldSpriteRenderer.sprite = normalSprite;

                StopMagicSound();
                break;

            case 1:
                // X-Ray Level 2
                heldSpriteRenderer.sprite = xray2Sprite;

                StartMagicSound();
                break;

            case 2:
                // X-Ray Level 3
                heldSpriteRenderer.sprite = xray3Sprite;

                StartMagicSound();
                break;
        }
    }
    
    private void StartMagicSound()
    {
        if (magicSound == null)
            return;

        // ถ้าเสียงกำลังเล่นอยู่ ไม่ต้องเริ่มใหม่
        if (!magicSound.isPlaying)
        {
            magicSound.Play();
        }
    }

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