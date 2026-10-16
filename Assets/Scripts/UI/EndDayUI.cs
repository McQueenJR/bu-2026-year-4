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

    public TMP_Text villagerRejectedText;
    public TMP_Text robberRejectedText;
    public TMP_Text villagersKilledText;

    public TMP_Text rankText;

    [Header("Button")]
    public Button nextDayButton;   // เพิ่มบรรทัดนี้

    void Awake()
    {
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(OnNextDayClicked);
    }

    // scoreEarned / scoreMax = คะแนนที่ได้ / คะแนนเต็มของวันนี้ (แรงค์คิดจากเปอร์เซ็นต์)
    public void Show(
        int scoreEarned,
        int scoreMax,
        int villagerPassed,
        int villagerArrested,
        int robberPassed,
        int robberArrested,
        int villagerRejected,
        int robberRejected,
        int villagersKilled)
    {
        panel.SetActive(true);

        villagerPassedText.text =
            "ชาวบ้านเข้าหมู่บ้าน : " + villagerPassed + " คน";

        villagerArrestedText.text =
            "ชาวบ้านโดนจับ : " + villagerArrested + " คน";

        robberPassedText.text =
            "ผีเข้าหมู่บ้าน : " + robberPassed + " คน";

        robberArrestedText.text =
            "ผีโดนจับ : " + robberArrested + " คน";

        villagerRejectedText.text =
            "ชาวบ้านถูกปฏิเสธ : " + villagerRejected + " คน";

        robberRejectedText.text =
            "ผีถูกปฏิเสธ : " + robberRejected + " คน";

        villagersKilledText.text =
            "ชาวบ้านถูกสังหาร : " + villagersKilled + " คน";

        rankText.text =
            "แรงค์ : " + ScoreRankUtility.GetRank(scoreEarned, scoreMax);
    }
    

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void OnNextDayClicked()
    {
        GameManager.Instance.StartNextDay();
    }


}