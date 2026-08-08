using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;    // ลาก Panel เมนู Pause มาใส่ตรงนี้
    public GameObject settingsPanel; // ลาก Panel เมนู Settings มาใส่ตรงนี้

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        // กันไม่ให้ timeScale ค้างจากซีนก่อนหน้า (เผื่อเคย pause แล้วเปลี่ยนซีนไป)
        Time.timeScale = 1f;
        IsPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ถ้าอยู่หน้า Settings ให้กด ESC ครั้งแรกกลับไปหน้า Pause ก่อน ไม่ใช่ Resume เลย
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f; // หยุดเวลาทั้งเกม
        IsPaused = true;
    }

    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f; // เดินเวลาต่อ
        IsPaused = false;
    }

    // เรียกจากปุ่ม "Main Menu" ใน pause panel
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // ต้องรีเซ็ตก่อนเปลี่ยนซีนเสมอ ไม่งั้นซีนถัดไปจะค้างเวลาไปด้วย
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // เรียกจากปุ่ม "Settings" ใน pause panel
    public void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // เรียกจากปุ่ม "Back" ในหน้า settings
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
}