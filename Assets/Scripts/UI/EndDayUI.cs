using UnityEngine;
using TMPro;
using UnityEngine.UI;   // เพิ่มบรรทัดนี้
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Text;

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
    public TMP_Text villagerSurvivedText;

    public TMP_Text rankText;
    
    [Header("Date (ใช้รูปแบบเดียวกับหนังสือพิมพ์/EndingSummaryUI)")]
    public TMP_Text dateText;
    [Tooltip("{date} = เลขวันที่ในปฏิทิน, {day} = วันที่ของเกม (1-7)")]
    public string dateFormat = "วันที่ {date} ตุลาคม 2558";
    [Tooltip("เลขวันที่ในปฏิทินของ Day 1")]
    public int firstDateNumber = 1;
    [Header("Button")]
    public Button nextDayButton;   // เพิ่มบรรทัดนี้

    
    [Header("Right Panel - Tabs (แผงข้อมูลด้านขวา)")]
    public Button ghostTabButton;
    public Button deadTabButton;
    public Button checklistTabButton;

    [Tooltip("Image ของแท็บ (ไม่ใส่ก็ได้ ใช้เปลี่ยนสีแท็บที่เลือก)")]
    public Image ghostTabImage;
    public Image deadTabImage;
    public Image checklistTabImage;

    public Color selectedTabColor = Color.white;
    public Color unselectedTabColor = new Color(0.78f, 0.78f, 0.78f, 1f);

    [Header("Right Panel - Pages")]
    public GameObject ghostPage;
    public GameObject deadPage;
    public GameObject checklistPage;

    public TMP_Text ghostListText;
    public TMP_Text deadListText;

    public string emptyGhostText = "ไม่มีผีเข้ามาในวันนี้";
    public string emptyDeadText = "ไม่มีชาวบ้านเสียชีวิตในวันนี้";
    
    
    [Header("Right Panel - Checklist Page")]
    [Tooltip("บรรทัดหัว เช่น เช็คลิส คนที่ 1 ชื่อ วรพล ทองดี")]
    public TMP_Text checklistTitleText;
    [Tooltip("GameObject แม่ของตาราง (หัวคอลัมน์ + 5 แถว) จะถูกซ่อนเมื่อไม่มีข้อมูล")]
    public GameObject checklistTable;
    [Tooltip("ข้อความกลางหน้า ใช้ตอนไม่มี NPC / NPC ไม่ได้ส่ง Checklist")]
    public TMP_Text checklistMessageText;
    public TMP_Text checklistPageText;
    public Button checklistPrevButton;
    public Button checklistNextButton;

    [Tooltip("ต้องมี 5 ช่อง เรียงตามแถว: ชื่อรายการ / ช่องปกติ / ช่องผิดปกติ")]
    public TMP_Text[] checklistItemNameTexts = new TMP_Text[5];

    [Tooltip("ต้องมี 5 ช่อง เรียงตามแถว")]
    public Image[] checklistNormalMarkImages = new Image[5];
    public Image[] checklistAbnormalMarkImages = new Image[5];

    public string[] checklistItemLabels =
    {
        "รูปลักษณ์ภายนอก",
        "แว่นขยาย",
        "ไม้ขีดไฟ",
        "เอกสารขอเข้า",
        "รายชื่อคนขอเข้าวันนี้"
    };

    [Tooltip("{n} = ลำดับ NPC, {name} = ชื่อ")]
    public string checklistTitleFormat = "เช็คลิส คนที่ {n} ชื่อ {name}";
    public string checklistPageFormat = "{n}/{total}";
    public string emptyChecklistText = "วันนี้ไม่มีการตรวจ NPC";
    public string notSubmittedText = "ไม่ได้ส่ง Checklist";
    
    [Header("Checklist Sprites")]
    public Sprite emptyBoxSprite;   // กรอบเปล่า
    public Sprite greenTickSprite;  // กรอบ + ติ๊กเขียว
    public Sprite redTickSprite;    // กรอบ + ติ๊กแดง

    // รายการ Checklist ของวันที่กำลังแสดง (อ่านอย่างเดียว) และ NPC ที่เปิดดูอยู่
    private List<NpcChecklistRecord> checklistRecords = new List<NpcChecklistRecord>();
    private int checklistIndex = 0;
    
    void Awake()
    {
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(OnNextDayClicked);

        if (ghostTabButton != null)
            ghostTabButton.onClick.AddListener(() => SelectTab(0));

        if (deadTabButton != null)
            deadTabButton.onClick.AddListener(() => SelectTab(1));

        if (checklistTabButton != null)
            checklistTabButton.onClick.AddListener(() => SelectTab(2));
        
        if (checklistPrevButton != null)
            checklistPrevButton.onClick.AddListener(OnChecklistPrev);

        if (checklistNextButton != null)
            checklistNextButton.onClick.AddListener(OnChecklistNext);
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

        // ชาวบ้านที่ยังมีชีวิตอยู่ในแคมป์จริง (ไม่ใช่ villagersKilled)
        if (villagerSurvivedText != null)
        {
            int alive = CampManager.Instance != null
                ? CampManager.Instance.GetAliveVillagerCount()
                : 0;

            villagerSurvivedText.text = "ชาวบ้านเหลือรอด : " + alive + " คน";
        }

        // วันที่ของวันที่เพิ่งจบ (currentDay ยังไม่ถูกเพิ่มจนกว่าจะกด Next Day)
        if (dateText != null)
        {
            int day = GameManager.Instance != null
                ? GameManager.Instance.currentDay
                : 1;

            dateText.text = FormatDate(day);
        }

        rankText.text =
            ScoreRankUtility.GetRank(scoreEarned, scoreMax);
        
        RefreshRightPanel();
    }
    

    public void Hide()
    {
        panel.SetActive(false);
    }
    
        // =========================
    // RIGHT PANEL
    // =========================

    // อ่านจาก DailyReport ของวันที่เพิ่งจบ (snapshot ที่ GameManager เก็บไว้แล้ว)
    private void RefreshRightPanel()
    {
        int day = GameManager.Instance != null
            ? GameManager.Instance.currentDay
            : 1;

        DailyReport report = CampaignStatsTracker.Instance != null
            ? CampaignStatsTracker.Instance.GetDailyReport(day)
            : null;

        if (ghostListText != null)
            ghostListText.text = BuildNameList(
                report?.ghostsPassed, false, emptyGhostText);

        // ชาวบ้านที่ตาย → ขีดฆ่าชื่อ
        if (deadListText != null)
            deadListText.text = BuildNameList(
                report?.villagersDead, true, emptyDeadText);
        // Checklist ของ NPC แต่ละคนในวันนั้น เริ่มดูคนแรก
        checklistRecords = report != null
            ? report.checklists
            : new List<NpcChecklistRecord>();

        checklistIndex = 0;
        ShowChecklistPage();
        SelectTab(0);
    }

    private string BuildNameList(List<string> names, bool strikethrough, string emptyText)
    {
        if (names == null || names.Count == 0)
            return emptyText;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < names.Count; i++)
        {
            if (i > 0)
                sb.Append('\n');

            if (strikethrough)
                sb.Append("<s>").Append(names[i]).Append("</s>");
            else
                sb.Append(names[i]);
        }

        return sb.ToString();
    }

    // 0 = ผีที่เข้ามาได้ | 1 = ชาวบ้านที่ตาย | 2 = ลิส (Checklist)
    // สลับแท็บแค่เปิด/ปิดหน้า ไม่แตะข้อมูลหรือคะแนนใด ๆ
    public void SelectTab(int index)
    {
        if (ghostPage != null) ghostPage.SetActive(index == 0);
        if (deadPage != null) deadPage.SetActive(index == 1);
        if (checklistPage != null) checklistPage.SetActive(index == 2);

        if (ghostTabImage != null)
            ghostTabImage.color = index == 0 ? selectedTabColor : unselectedTabColor;

        if (deadTabImage != null)
            deadTabImage.color = index == 1 ? selectedTabColor : unselectedTabColor;

        if (checklistTabImage != null)
            checklistTabImage.color = index == 2 ? selectedTabColor : unselectedTabColor;
    }
    
        // =========================
    // CHECKLIST PAGE
    // =========================

    private void OnChecklistPrev()
    {
        if (checklistIndex <= 0)
            return;

        checklistIndex--;
        ShowChecklistPage();
    }

    private void OnChecklistNext()
    {
        if (checklistIndex >= checklistRecords.Count - 1)
            return;

        checklistIndex++;
        ShowChecklistPage();
    }

    private void ShowChecklistPage()
    {
        int total = checklistRecords != null ? checklistRecords.Count : 0;

        // วันนี้ไม่มี NPC ที่ตรวจ
        if (total == 0)
        {
            if (checklistTitleText != null)
                checklistTitleText.text = "";

            ShowChecklistMessage(emptyChecklistText);
            UpdateChecklistNav(0, 0);
            return;
        }

        checklistIndex = Mathf.Clamp(checklistIndex, 0, total - 1);
        NpcChecklistRecord rec = checklistRecords[checklistIndex];

        if (checklistTitleText != null)
        {
            checklistTitleText.text = (checklistTitleFormat ?? "{name}")
                .Replace("{n}", (checklistIndex + 1).ToString())
                .Replace("{name}", rec.displayName);
        }

        if (!rec.submitted)
        {
            // เช่นถูกจับผ่านตำรวจโดยไม่ได้ส่ง Checklist
            ShowChecklistMessage(notSubmittedText);
        }
        else
        {
            if (checklistTable != null) checklistTable.SetActive(true);
            if (checklistMessageText != null) checklistMessageText.text = "";

            FillChecklistRows(rec);
        }

        UpdateChecklistNav(checklistIndex, total);
    }

    private void ShowChecklistMessage(string message)
    {
        if (checklistTable != null)
            checklistTable.SetActive(false);

        if (checklistMessageText != null)
            checklistMessageText.text = message;
    }

    private void FillChecklistRows(NpcChecklistRecord rec)
    {
        for (int i = 0; i < 5; i++)
        {
            TMP_Text nameText = GetTextAt(checklistItemNameTexts, i);
            Image normalImg = GetImageAt(checklistNormalMarkImages, i);
            Image abnormalImg = GetImageAt(checklistAbnormalMarkImages, i);

            if (nameText != null &&
                checklistItemLabels != null &&
                i < checklistItemLabels.Length)
                nameText.text = checklistItemLabels[i];

            // รีเซ็ตเป็นกรอบเปล่าก่อนเสมอ กันของ NPC คนก่อนค้าง
            SetMark(normalImg, emptyBoxSprite);
            SetMark(abnormalImg, emptyBoxSprite);

            ChecklistItemResult item = rec.items.Find(r => r.itemIndex == i);
            if (item == null)
                continue;

            bool playerAbnormal = item.playerAnswerAbnormal;
            bool correctAbnormal = item.correctAnswerAbnormal;

            // ช่องที่ผู้เล่นเลือก: ถูก = เขียว, ผิด = แดง
            Sprite playerSprite = (playerAbnormal == correctAbnormal)
                ? greenTickSprite
                : redTickSprite;

            SetMark(playerAbnormal ? abnormalImg : normalImg, playerSprite);

            // ตอบผิด -> โชว์ช่องเฉลยเป็นติ๊กเขียวด้วย
            if (playerAbnormal != correctAbnormal)
                SetMark(correctAbnormal ? abnormalImg : normalImg, greenTickSprite);
        }
    }

    private static void SetMark(Image img, Sprite sprite)
    {
        if (img == null) return;
        img.sprite = sprite;
        img.enabled = sprite != null;
    }

    private static Image GetImageAt(Image[] array, int index)
    {
        return array != null && index < array.Length ? array[index] : null;
    }

    private void UpdateChecklistNav(int index, int total)
    {
        if (checklistPageText != null)
        {
            checklistPageText.text = total == 0
                ? ""
                : (checklistPageFormat ?? "{n}/{total}")
                    .Replace("{n}", (index + 1).ToString())
                    .Replace("{total}", total.ToString());
        }

        if (checklistPrevButton != null)
            checklistPrevButton.interactable = total > 0 && index > 0;

        if (checklistNextButton != null)
            checklistNextButton.interactable = total > 0 && index < total - 1;
    }

    private static TMP_Text GetTextAt(TMP_Text[] array, int index)
    {
        return array != null && index < array.Length ? array[index] : null;
    }
    
    private string FormatDate(int day)
    {
        int date = firstDateNumber + Mathf.Max(day, 1) - 1;

        return (dateFormat ?? "{date}")
            .Replace("{date}", date.ToString())
            .Replace("{day}", day.ToString());
    }

    private void OnNextDayClicked()
    {
        GameManager.Instance.StartNextDay();
    }


}