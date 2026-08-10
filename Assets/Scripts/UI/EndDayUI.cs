using UnityEngine;
using TMPro;
using UnityEngine.UI;   // เพิ่มบรรทัดนี้

public class EndDayUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Texts")]
    public TMP_Text villagerPassedText;
    public TMP_Text villagerArrestedText;
    public TMP_Text robberPassedText;
    public TMP_Text robberArrestedText;
    public TMP_Text rankText;

    [Header("Button")]
    public Button nextDayButton;   // เพิ่มบรรทัดนี้

    void Awake()
    {
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(OnNextDayClicked);
    }

    public void Show(int score, int villagerPassed, int villagerArrested, int robberPassed, int robberArrested)
    {
        panel.SetActive(true);
        
        villagerPassedText.text = "ชาวบ้านเข้าหมู่บ้าน : " + villagerPassed + " คน";
        villagerArrestedText.text = "ชาวบ้านโดนจับ : " + villagerArrested + " คน";
        robberPassedText.text = "โจรเข้าหมู่บ้าน : " + robberPassed + " คน";
        robberArrestedText.text = "โจรโดนจับ : " + robberArrested + " คน";

        rankText.text = "แรงค์ : " + GetRank(score);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void OnNextDayClicked()
    {
        GameManager.Instance.StartNextDay();
    }

    private string GetRank(int score)
    {
        if (score <= 1) return "F";
        if (score <= 3) return "D";
        if (score <= 5) return "C";
        if (score == 6) return "B";
        if (score == 7) return "A";
        if (score >= 8) return "S";

        return "F";
    }
}