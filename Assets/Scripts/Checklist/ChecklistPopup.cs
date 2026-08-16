using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// เขียนให้สไตล์เดียวกับ IDCardPopup ที่มีอยู่แล้ว (Singleton, Show/Hide, blocker คู่กับ panel)
///
/// โครงสร้าง Hierarchy ที่แนะนำใต้ popupPanel:
///   ChecklistPanel (มี ChecklistPopup script นี้ติดอยู่ หรือแยกไปอีก GameObject ก็ได้)
///     - Blocker            <- ChecklistBlocker.cs, Image alpha 0, raycastTarget=true, sibling แรกสุด (index 0)
///     - NotebookBackground  <- ChecklistClickAbsorber.cs ติดไว้กันคลิกทะลุ, sibling หลัง Blocker
///     - Page1_AskQuestions  <- Toggle 4 อัน (กระเป๋า/รูปร่างหน้าตา/บัตรประชาชน/เอกสารขอเข้า)
///     - Page2_Evaluate      <- Toggle คู่ ผิดปกติ/ไม่ผิดปกติ ต่อแถว (ใช้ ToggleGroup ต่อแถว)
///     - PostIt (ปุ่มบนภาพโพสต์อิท ใช้สลับหน้า)
///
/// ข้อสำคัญ: Blocker ต้องเป็น sibling แรกสุด (บนสุดของลิสต์ Hierarchy) ให้เนื้อหาอื่น
/// เรนเดอร์ทับข้างบน (sibling หลังจากนั้น) เพื่อให้ Toggle/ปุ่มรับคลิกของตัวเองได้ก่อน
/// คลิกที่ไม่โดนอะไรเลยถึงจะทะลุไปโดน Blocker แล้วปิด Panel
/// </summary>
public class ChecklistPopup : MonoBehaviour
{
    public static ChecklistPopup Instance;

    [Header("Panel + Blocker (เปิด/ปิดคู่กันเสมอ)")]
    public GameObject popupPanel;
    public GameObject blocker;

    [Header("หน้าย่อย")]
    public GameObject page1_AskQuestions;
    public GameObject page2_Evaluate;

    [Header("หน้า 1 - Toggle สอบถาม 4 ข้อ")]
    public Toggle toggleBag;
    public Toggle toggleAppearance;
    public Toggle toggleIDCard;
    public Toggle toggleEntryDoc;

    [Header("หน้า 2 - แต่ละแถวมี ToggleGroup คู่ ผิดปกติ/ไม่ผิดปกติ")]
    public Toggle bagAbnormal,        bagNormal;
    public Toggle appearanceAbnormal, appearanceNormal;
    public Toggle idAbnormal,         idNormal;
    public Toggle docAbnormal,        docNormal;

    [Header("ปุ่มบนโพสต์อิท (สลับหน้า)")]
    public Button postItGoToPage2;
    public Button postItGoToPage1;
    public Button submitButton;

    [Header("ช่องเก็บรูปภาพ (ถ้ามี)")]
    public Image photoSlot;

    private NPCData currentNpc;

    void Awake()
    {
        Instance = this;

        popupPanel.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        if (postItGoToPage2 != null) postItGoToPage2.onClick.AddListener(ShowPage2);
        if (postItGoToPage1 != null) postItGoToPage1.onClick.AddListener(ShowPage1);
        if (submitButton != null)    submitButton.onClick.AddListener(OnSubmit);
    }

    // เรียกจาก WorldButton.OnClick() ของกระดาษบนโต๊ะ (ยังไม่ผูก NPC คนไหน)
    public void OpenChecklist()
    {
        Show(null);
    }

    public void Show(NPCData npc)
    {
        currentNpc = npc;

        popupPanel.SetActive(true);

        if (blocker != null)
            blocker.SetActive(true);

        ResetToggles();
        ShowPage1();
    }

    public void Hide()
    {
        popupPanel.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        currentNpc = null;
    }

    public void ShowPage1()
    {
        if (page1_AskQuestions != null) page1_AskQuestions.SetActive(true);
        if (page2_Evaluate != null)     page2_Evaluate.SetActive(false);
    }

    public void ShowPage2()
    {
        if (page1_AskQuestions != null) page1_AskQuestions.SetActive(false);
        if (page2_Evaluate != null)     page2_Evaluate.SetActive(true);
    }

    public struct EvaluationResult
    {
        public bool bagOk;
        public bool appearanceOk;
        public bool idOk;
        public bool docOk;

        public bool AllNormal => bagOk && appearanceOk && idOk && docOk;
    }

    public EvaluationResult GetEvaluation()
    {
        return new EvaluationResult
        {
            bagOk        = bagNormal        != null && bagNormal.isOn,
            appearanceOk = appearanceNormal != null && appearanceNormal.isOn,
            idOk         = idNormal         != null && idNormal.isOn,
            docOk        = docNormal        != null && docNormal.isOn
        };
    }

    void OnSubmit()
    {
        EvaluationResult result = GetEvaluation();

        // TODO: ต่อกับ GameManager เพื่อตัดสินว่าตรงกับ Role จริงของ currentNpc หรือไม่
        // เช่น GameManager.Instance.SubmitEvaluation(currentNpc, result);

        Debug.Log($"[Checklist] กระเป๋า:{result.bagOk} รูปร่าง:{result.appearanceOk} " +
                  $"บัตร:{result.idOk} เอกสาร:{result.docOk} สรุป: {(result.AllNormal ? "ปกติทั้งหมด" : "มีจุดผิดปกติ")}");

        Hide();
    }

    public void SetPhoto(Sprite sprite)
    {
        if (photoSlot == null) return;
        photoSlot.sprite = sprite;
        photoSlot.enabled = sprite != null;
    }

    void ResetToggles()
    {
        if (toggleBag != null)        toggleBag.isOn = false;
        if (toggleAppearance != null) toggleAppearance.isOn = false;
        if (toggleIDCard != null)     toggleIDCard.isOn = false;
        if (toggleEntryDoc != null)   toggleEntryDoc.isOn = false;

        if (bagAbnormal != null)        bagAbnormal.isOn = false;
        if (bagNormal != null)          bagNormal.isOn = false;
        if (appearanceAbnormal != null) appearanceAbnormal.isOn = false;
        if (appearanceNormal != null)   appearanceNormal.isOn = false;
        if (idAbnormal != null)         idAbnormal.isOn = false;
        if (idNormal != null)           idNormal.isOn = false;
        if (docAbnormal != null)        docAbnormal.isOn = false;
        if (docNormal != null)          docNormal.isOn = false;
    }
}