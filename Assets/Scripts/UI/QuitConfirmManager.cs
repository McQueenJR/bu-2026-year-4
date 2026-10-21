
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class QuitConfirmManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject confirmPanel; // ลาก Panel ที่ถามยืนยันมาใส่ตรงนี้

    [Header("Clickable Text")]
    public TMP_Text quitText;        // ข้อความ QUIT ที่หน้า Main Menu
    public TMP_Text confirmYesText;  // ข้อความ "ใช่" ใน Confirm Panel
    public TMP_Text cancelNoText;    // ข้อความ "ไม่" ใน Confirm Panel

    void Awake()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    void Start()
    {
        // เพิ่มตัวตรวจจับคลิกให้ข้อความโดยอัตโนมัติ
        RegisterClick(quitText, OpenConfirmPanel);
        RegisterClick(confirmYesText, ConfirmQuit);
        RegisterClick(cancelNoText, CancelQuit);
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

    // คลิกข้อความ QUIT เพื่อเปิดหน้าต่างยืนยัน
    public void OpenConfirmPanel()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    // คลิกข้อความ "ใช่" เพื่อออกจากเกม
    public void ConfirmQuit()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    // คลิกข้อความ "ไม่" เพื่อปิดหน้าต่างยืนยัน
    public void CancelQuit()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}
