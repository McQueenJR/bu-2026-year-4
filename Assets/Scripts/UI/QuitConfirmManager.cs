using UnityEngine;

// ติดสคริปต์นี้ที่ Empty GameObject เช่น "QuitConfirmManager" ใน Hierarchy
public class QuitConfirmManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject confirmPanel; // ลาก Panel ที่ถามยืนยันมาใส่ตรงนี้

    void Awake()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    // เรียกจากปุ่ม Quit (แทนที่การ Quit ตรงๆ)
    public void OpenConfirmPanel()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    // เรียกจากปุ่ม "ใช่" ใน confirm panel
    public void ConfirmQuit()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // ใช้ทดสอบตอนรันใน Editor เพราะ Quit จริงจะไม่ทำงานใน Editor
    }

    // เรียกจากปุ่ม "ไม่" ใน confirm panel
    public void CancelQuit()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}