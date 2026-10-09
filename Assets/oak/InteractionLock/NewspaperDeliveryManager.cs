using System;
using System.Collections;
using UnityEngine;

public class NewspaperDeliveryManager : MonoBehaviour
{
    public static NewspaperDeliveryManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private NewspaperSchedule schedule;
    [SerializeField] private NewspaperDialogueData defaultDialogue;

    [Header("Delivery Character")]
    [Tooltip("Prefab ตัวละครคนส่ง ต้องมี NPCMovement (ไม่ต้องมี NPC.cs)")]
    [SerializeField] private GameObject deliveryNpcPrefab;

    [Header("Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform stopPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform deskPoint;

    [Header("Newspaper Object")]
    [SerializeField] private NewspaperItem newspaperPrefab;
    [SerializeField] private NewspaperReaderPopup readerPopup;

    [Header("References")]
    [Tooltip("ว่าง = ใช้ของ GameManager")]
    [SerializeField] private DialogManager dialogManager;

    [Header("Interaction Lock")]
    [Tooltip("BoxCollider2D ใหญ่เต็มจอ วางหน้าวัตถุทุกชิ้นบนโต๊ะ (เริ่มต้นปิดไว้)")]
    [SerializeField] private GameObject worldBlocker;
    [Tooltip("Image โปร่งใสเต็มจอ ต้องอยู่ใต้ Dialog (เริ่มต้นปิดไว้)")]
    [SerializeField] private GameObject uiBlocker;

    [Header("Timing")]
    [SerializeField] private float placeDelay = 0.3f;    // หลังพูดจบ → วางหนังสือพิมพ์
    [SerializeField] private float pickupDelay = 0.5f;   // หลังปิดอ่าน → เก็บ
    [SerializeField] private float leaveDelay = 0.3f;    // หลังเก็บ → เดินออก

    [Header("Sound (ไม่ใส่ก็ได้)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip pickupClip;

    private bool isRunning = false;
    private bool dialogFinished = false;
    private bool readingClosed = false;
    private bool canOpenNewspaper = false;

    private NewspaperDayEntry currentEntry;
    private NewspaperItem currentItem;

    public bool IsRunning => isRunning;

    private void Awake()
    {
        Instance = this;
        SetLock(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SetLock(false);
        }
    }

    // =====================================================
    // เรียกจาก GameManager ตอนเริ่มวัน
    // onComplete จะถูกเรียกเมื่อ "ส่งเสร็จและปลดล็อกแล้ว"
    // หรือถูกเรียกทันทีถ้าวันนั้นข้ามระบบนี้ (ข้อมูลไม่ครบ)
    // =====================================================
    public void BeginMorningDelivery(int day, Action onComplete)
    {
        if (isRunning)
        {
            Debug.LogWarning("[Newspaper] กำลังส่งหนังสือพิมพ์อยู่แล้ว → ไม่เริ่มซ้ำ");
            return;
        }

        if (dialogManager == null && GameManager.Instance != null)
            dialogManager = GameManager.Instance.dialogManager;

        if (!TryPrepare(day, out NewspaperDayEntry entry, out NewspaperDialogueData dialogue))
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DeliverySequence(entry, dialogue, onComplete));
    }

    // ตรวจข้อมูลก่อนเริ่ม — ถ้าขาดอะไรให้ข้ามพร้อม Warning (ไม่ล็อก ไม่ค้าง)
    private bool TryPrepare(
        int day,
        out NewspaperDayEntry entry,
        out NewspaperDialogueData dialogue)
    {
        entry = null;
        dialogue = null;

        if (schedule == null)
        {
            Debug.LogWarning("[Newspaper] ไม่ได้ใส่ Schedule → ข้ามระบบหนังสือพิมพ์");
            return false;
        }

        entry = schedule.GetEntry(day);

        if (entry == null)
        {
            Debug.LogWarning($"[Newspaper] ไม่มีข้อมูลหนังสือพิมพ์ของ Day {day} → ข้ามระบบหนังสือพิมพ์");
            return false;
        }

        if (entry.newspaper == null || entry.newspaper.readingSprite == null)
        {
            Debug.LogWarning($"[Newspaper] Day {day} ไม่มี NewspaperData หรือ Reading Sprite → ข้าม");
            return false;
        }

        if (deliveryNpcPrefab == null || deliveryNpcPrefab.GetComponent<NPCMovement>() == null)
        {
            Debug.LogWarning("[Newspaper] Delivery NPC Prefab ว่าง หรือไม่มี NPCMovement → ข้าม");
            return false;
        }

        if (spawnPoint == null || stopPoint == null || exitPoint == null || deskPoint == null)
        {
            Debug.LogWarning("[Newspaper] ใส่ Spawn/Stop/Exit/Desk Point ไม่ครบ → ข้าม");
            return false;
        }

        if (newspaperPrefab == null || readerPopup == null)
        {
            Debug.LogWarning("[Newspaper] ไม่ได้ใส่ Newspaper Prefab หรือ Reader Popup → ข้าม");
            return false;
        }

        dialogue = entry.dialogueOverride != null ? entry.dialogueOverride : defaultDialogue;

        return true;
    }

    // =====================================================
    // SEQUENCE
    // =====================================================
    private IEnumerator DeliverySequence(
        NewspaperDayEntry entry,
        NewspaperDialogueData dialogue,
        Action onComplete)
    {
        isRunning = true;
        currentEntry = entry;
        SetLock(true);

        // 1) Spawn คนส่ง + เดินเข้า
        GameObject boy = Instantiate(
            deliveryNpcPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        NPCMovement move = boy.GetComponent<NPCMovement>();
        move.notifyCheckpointOnArrival = false;   // ห้ามเข้าระบบ NPC ตรวจ
        move.MoveTo(stopPoint.position);

        while (move.IsMoving())
            yield return null;

        // 2) Dialog ทักทาย
        if (dialogue != null && dialogue.HasLines && dialogManager != null)
        {
            dialogFinished = false;

            dialogManager.SetTalkingNPC(boy.GetComponentInChildren<NPCMouthAnimation>());

            if (dialogManager.StartNewspaperDialog(dialogue))
            {
                yield return new WaitUntil(() => dialogFinished);
            }
            else
            {
                dialogManager.SetTalkingNPC(null);
            }
        }

        // 3) วางหนังสือพิมพ์บนโต๊ะ
        yield return new WaitForSeconds(placeDelay);

        currentItem = Instantiate(newspaperPrefab, deskPoint.position, deskPoint.rotation);
        currentItem.Setup(this, entry.newspaper);
        PlaySfx(placeClip);

        readingClosed = false;
        canOpenNewspaper = true;
        currentItem.SetInteractable(true);

        // 4) รอผู้เล่นคลิกเปิด → อ่าน → คลิกขวาปิด
        yield return new WaitUntil(() => readingClosed);

        // 5) คนส่งเก็บหนังสือพิมพ์
        yield return new WaitForSeconds(pickupDelay);

        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
            currentItem = null;
        }

        PlaySfx(pickupClip);

        yield return new WaitForSeconds(leaveDelay);

        // 6) เดินออก
        move.MoveTo(exitPoint.position);

        while (move.IsMoving())
            yield return null;

        Destroy(boy);

        // 7) ปลดล็อก แล้วค่อยให้ SpawnManager เดิมเริ่ม
        isRunning = false;
        currentEntry = null;
        SetLock(false);

        onComplete?.Invoke();
    }

    // =====================================================
    // เรียกจาก NewspaperItem (ผู้เล่นคลิกหนังสือพิมพ์)
    // =====================================================
    public void RequestOpenReading()
    {
        if (!isRunning || !canOpenNewspaper || currentEntry == null)
            return;

        if (readerPopup.IsOpen)
            return;

        if (dialogManager != null && dialogManager.IsDialogOpen())
            return;

        canOpenNewspaper = false;

        if (currentItem != null)
            currentItem.SetInteractable(false);

        readerPopup.Open(currentEntry.newspaper.readingSprite, OnReadingClosed);

        if (NPCSoundManager.Instance != null)
            NPCSoundManager.Instance.PlayDocumentOpen();
    }

    private void OnReadingClosed()
    {
        if (NPCSoundManager.Instance != null)
            NPCSoundManager.Instance.PlayDocumentClose();

        readingClosed = true;
    }

    // =====================================================
    // เรียกจาก DialogManager ตอน Dialog ของคนส่งจบ
    // =====================================================
    public void OnDeliveryDialogFinished()
    {
        if (!isRunning) return;
        dialogFinished = true;
    }

    // =====================================================
    // LOCK
    // =====================================================
    private void SetLock(bool locked)
    {
        InteractionLock.SetLocked(locked);

        if (worldBlocker != null) worldBlocker.SetActive(locked);
        if (uiBlocker != null) uiBlocker.SetActive(locked);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}