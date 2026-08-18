using TMPro;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogPanel;

    public TMP_Text nameText;
    public TMP_Text dialogText;

    public BagManager bagManager;

    private string[] dialogs;
    private int currentIndex;


    // =====================================================
    // DIALOG TYPE
    // =====================================================

    private enum DialogType
    {
        Normal,
        Green,
        Emergency,
        Simple,
        Checklist
    }

    private DialogType currentDialogType;


    // =====================================================
    // NORMAL
    // =====================================================

    public void StartDialog(NPCData data)
    {
        if (data == null)
            return;

        currentDialogType = DialogType.Normal;

        dialogs = data.dialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // GREEN
    // =====================================================

    public void StartGreenDialog(NPCData data)
    {
        if (data == null)
            return;

        currentDialogType = DialogType.Green;

        dialogs = data.greenDialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // EMERGENCY
    // =====================================================

    public void StartEmergencyDialog(NPCData data)
    {
        if (data == null)
            return;

        if (data.emergencyDialogs == null ||
            data.emergencyDialogs.Length == 0)
        {
            Debug.LogWarning(
                "NPC " +
                data.npcName +
                " ไม่มี Emergency Dialog"
            );

            return;
        }

        currentDialogType = DialogType.Emergency;

        dialogs = data.emergencyDialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // SIMPLE
    // =====================================================

    public void StartSimpleDialog(
        string speaker,
        string[] messages)
    {
        if (messages == null ||
            messages.Length == 0)
            return;

        currentDialogType = DialogType.Simple;

        dialogs = messages;
        currentIndex = 0;

        nameText.text = speaker;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // CHECKLIST
    // =====================================================

    // ใช้สำหรับคำถาม Checklist
    // ไม่ต้องมี answer
    public void StartChecklistDialog(
        string speaker,
        string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            Debug.LogWarning(
                "Checklist ไม่มีข้อความ"
            );

            return;
        }

        currentDialogType = DialogType.Checklist;

        dialogs = new string[]
        {
            question
        };

        currentIndex = 0;

        nameText.text = speaker;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }


    // =====================================================
    // SHOW CURRENT
    // =====================================================

    private void ShowCurrentDialog()
    {
        if (dialogs == null)
            return;

        if (dialogs.Length == 0)
            return;

        if (currentIndex < 0 ||
            currentIndex >= dialogs.Length)
            return;

        dialogText.text = dialogs[currentIndex];

        Debug.Log(
            "Dialog : " +
            dialogs[currentIndex]
        );
    }


    // =====================================================
    // NEXT
    // =====================================================

    public void NextDialog()
    {
        if (dialogs == null ||
            dialogs.Length == 0)
            return;

        currentIndex++;

        if (currentIndex >= dialogs.Length)
        {
            EndDialog();
            return;
        }

        ShowCurrentDialog();
    }


    // =====================================================
    // END
    // =====================================================

    private void EndDialog()
    {
        dialogPanel.SetActive(false);

        switch (currentDialogType)
        {
            case DialogType.Normal:

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DialogFinished();
                }

                break;


            case DialogType.Green:

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GreenDialogFinished();
                }

                break;


            case DialogType.Emergency:

                // Emergency จบ
                break;


            case DialogType.Simple:

                // Simple จบ
                break;


            case DialogType.Checklist:

                // บอก ChecklistManager ว่า
                // Dialog ข้อนี้จบแล้ว
                if (ChecklistManager.Instance != null)
                {
                    ChecklistManager.Instance
                        .ChecklistDialogFinished();
                }

                break;
        }
    }


    // =====================================================
    // CHECK
    // =====================================================

    public bool IsDialogOpen()
    {
        return dialogPanel != null &&
               dialogPanel.activeSelf;
    }
}