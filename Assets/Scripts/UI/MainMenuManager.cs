using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // ใส่ชื่อซีนเกมของคุณตรงนี้ (ต้องตรงกับชื่อไฟล์ scene เป๊ะๆ)
    public string gameSceneName = "Game";
 
    [Header("UI")]
    public GameObject mainMenuPanel; // Panel หลักที่มีปุ่ม Play / Settings / Quit
    public GameObject settingsPanel; // Panel ตั้งค่า (มี Slider เสียงของคุณอยู่แล้ว)
 
    void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
 
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
 
    // เรียกฟังก์ชันนี้จากปุ่ม Play (On Click ())
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
 
    // เรียกจากปุ่ม Settings ในหน้า Main Menu
    public void OpenSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
 
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
 
    // เรียกจากปุ่ม Back ในหน้า Settings (ของ MainMenu เท่านั้น ไม่ใช่ตัวเดียวกับ PauseManager)
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
 
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
 
    // เผื่อมีปุ่ม Quit
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // ใช้ทดสอบตอนรันใน Editor เพราะ Quit จริงจะไม่ทำงานใน Editor
    }
}