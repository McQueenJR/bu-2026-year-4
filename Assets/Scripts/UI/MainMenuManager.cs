
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // ใส่ชื่อซีนเกมของคุณตรงนี้ (ต้องตรงกับชื่อไฟล์ scene เป๊ะๆ)
    public string gameSceneName = "Game";

    [Header("UI")]
    public GameObject mainMenuPanel; // Panel หลักที่มีข้อความ Play / Settings
    public GameObject settingsPanel; // Panel ตั้งค่า (มี Slider เสียงของคุณอยู่แล้ว)

    [Header("Clickable Text")]
    public TMP_Text playText;
    public TMP_Text settingsText;

    void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    void Start()
    {
        // เพิ่มตัวตรวจจับคลิกให้ข้อความโดยอัตโนมัติ
        RegisterClick(playText, PlayGame);
        RegisterClick(settingsText, OpenSettings);
    }

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
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

        // ป้องกันการเพิ่ม Pointer Click ซ้ำ
        trigger.triggers.RemoveAll(
            entry => entry.eventID == EventTriggerType.PointerClick
        );

        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((eventData) => action.Invoke());

        trigger.triggers.Add(clickEntry);
    }

    // เรียกจากข้อความ PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // เรียกจากข้อความ SETTINGS
    public void OpenSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // เรียกใช้เมื่อต้องการกลับจาก Settings ไป Main Menu
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
}
