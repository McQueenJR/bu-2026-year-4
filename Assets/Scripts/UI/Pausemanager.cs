
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;    // ลาก Panel เมนู Pause มาใส่ตรงนี้
    public GameObject settingsPanel; // ลาก Panel เมนู Settings มาใส่ตรงนี้

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Clickable Text")]
    public TMP_Text resumeText;   // ข้อความ Resume
    public TMP_Text settingsText; // ข้อความ Settings ในหน้า Pause
    public TMP_Text mainMenuText; // ข้อความ Main Menu

    void Awake()
    {
        // กันไม่ให้ timeScale ค้างจากซีนก่อนหน้า (เผื่อเคย pause แล้วเปลี่ยนซีนไป)
        Time.timeScale = 1f;
        IsPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Start()
    {
        // เพิ่มตัวตรวจจับคลิกให้ข้อความโดยอัตโนมัติ
        RegisterClick(resumeText, Resume);
        RegisterClick(settingsText, OpenSettings);
        RegisterClick(mainMenuText, GoToMainMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    // เพิ่ม Event Trigger ให้ข้อความคลิกได้
    void RegisterClick(TMP_Text text, UnityEngine.Events.UnityAction action)
    {
        if (text == null)
            return;

        // ข้อความต้องรับ Raycast จากเมาส์ได้
        text.raycastTarget = true;

        // เพิ่ม Event Trigger หากยังไม่มี
        EventTrigger trigger = text.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = text.gameObject.AddComponent<EventTrigger>();

        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();

        // ป้องกันการเพิ่ม Pointer Click ซ้ำ
        trigger.triggers.RemoveAll(
            entry => entry.eventID == EventTriggerType.PointerClick
        );

        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((eventData) => action.Invoke());

        trigger.triggers.Add(clickEntry);
    }

    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // ปิด Settings เมื่อเปิด Pause ใหม่
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 0f; // หยุดเวลาทั้งเกม
        IsPaused = true;
    }

    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f; // เดินเวลาต่อ
        IsPaused = false;
    }

    // เรียกจากข้อความ "Main Menu" ใน pause panel
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // ต้องรีเซ็ตก่อนเปลี่ยนซีนเสมอ ไม่งั้นซีนถัดไปจะค้างเวลาไปด้วย
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // เรียกจากข้อความ "Settings" ใน pause panel
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        // ให้เมนู Pause อยู่ด้านหน้าหน้า Settings
        if (pausePanel != null)
            pausePanel.transform.SetAsLastSibling();
    }
}
