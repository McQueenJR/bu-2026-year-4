using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingSummaryUI : MonoBehaviour
{
    [Header("Panel (กระดาษรายงาน ปิดไว้ตอนเริ่ม)")]
    public GameObject panel;

    [Header("Texts")]
    public TMP_Text dateText;
    public TMP_Text rankText;
    public TMP_Text titleText;
    public TMP_Text ghostsEliminatedText;
    public TMP_Text ghostsEnteredText;
    public TMP_Text ghostsRejectedText;
    public TMP_Text villagersWrongText;
    public TMP_Text villagersRejectedText;
    public TMP_Text villagersKilledText;

    [Header("Signature")]
    public Image signatureImage;
    public Sprite goodSignature;
    public Sprite badSignature;

    [Header("Date (รูปแบบเดียวกับหนังสือพิมพ์)")]
    public string dateFormat = "วันที่ {date} ตุลาคม 2558";
    public int firstDateNumber = 1;

    [Header("หัวข้อ แยกตามประเภทฉากจบ")]
    public string goodTitle = "สรุปแคมเปญ";
    public string badTitle = "สรุปแคมเปญ";

    [Header("Button")]
    public Button menuButton;
    public string menuSceneName = "MainMenu";

    [Header("ทดสอบ (เปิดซีนนี้ตรง ๆ โดยไม่ผ่านเกม)")]
    public bool useTestDataIfEmpty = true;
    public EndingType testType = EndingType.Bad;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    public void Show()
    {
        if (!EndingSummaryData.HasData)
        {
            if (useTestDataIfEmpty)
            {
                EndingSummaryData.HasData = true;
                EndingSummaryData.type = testType;
                EndingSummaryData.endDay = 7;
                EndingSummaryData.ghostsEliminated = 5;
                EndingSummaryData.ghostsEntered = 2;
                EndingSummaryData.ghostsRejected = 3;
                EndingSummaryData.villagersWronglyJudged = 1;
                EndingSummaryData.villagersRejected = 4;
                EndingSummaryData.villagersKilledByGhost = 2;
            }
            else
            {
                Debug.LogWarning("EndingSummaryUI: ไม่มี snapshot → แสดงค่า 0");
            }
        }

        bool bad = EndingSummaryData.type == EndingType.Bad;

        if (dateText != null)
            dateText.text = FormatDate(EndingSummaryData.endDay);

        if (rankText != null)
            rankText.text =  EndingSummaryData.rankText;

        if (titleText != null)
            titleText.text = bad ? badTitle : goodTitle;

        Set(ghostsEliminatedText, "ผีที่ถูกกำจัด", EndingSummaryData.ghostsEliminated);
        Set(ghostsEnteredText, "ผีที่เข้ามาในเต็นท์ได้", EndingSummaryData.ghostsEntered);
        Set(ghostsRejectedText, "ผีที่ถูกปฏิเสธไม่ให้เข้า", EndingSummaryData.ghostsRejected);
        Set(villagersWrongText, "ชาวบ้านที่ถูกตัดสินผิด", EndingSummaryData.villagersWronglyJudged);
        Set(villagersRejectedText, "ชาวบ้านที่ถูกปฏิเสธไม่ให้เข้า", EndingSummaryData.villagersRejected);
        Set(villagersKilledText, "ชาวบ้านที่ถูกผีฆ่า", EndingSummaryData.villagersKilledByGhost);

        if (signatureImage != null)
        {
            Sprite s = bad ? badSignature : goodSignature;
            if (s != null) signatureImage.sprite = s;
        }

        if (panel != null)
            panel.SetActive(true);
    }

    private void Set(TMP_Text text, string label, int value)
    {
        if (text != null)
            text.text = label + " : " + value + " คน";
    }

    private string FormatDate(int day)
    {
        int date = firstDateNumber + Mathf.Max(day, 1) - 1;

        return (dateFormat ?? "{date}")
            .Replace("{date}", date.ToString())
            .Replace("{day}", day.ToString());
    }

    private void OnMenuClicked()
    {
        EndingSummaryData.Clear();
        SceneManager.LoadScene(menuSceneName);
    }
}