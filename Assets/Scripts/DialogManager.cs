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

    private enum DialogType
    {
        Normal,
        Green,
        Emergency,
        Simple
    }

    private DialogType currentDialogType;

    public void StartDialog(NPCData data)
    {
        currentDialogType = DialogType.Normal;

        dialogs = data.dialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }

    public void StartGreenDialog(NPCData data)
    {
        currentDialogType = DialogType.Green;

        dialogs = data.greenDialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }

    public void StartEmergencyDialog(NPCData data)
    {
        if (data == null)
            return;

        if (data.emergencyDialogs == null ||
            data.emergencyDialogs.Length == 0)
        {
            Debug.LogWarning(
                "NPC " + data.npcName +
                " ไม่มี Emergency Dialog"
            );
            return;
        }

        dialogs = data.emergencyDialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }

    public bool IsDialogOpen()
    {
        return dialogPanel.activeSelf;
    }

    public void StartSimpleDialog(string speaker, string[] messages)
    {
        currentDialogType = DialogType.Simple;

        dialogs = messages;
        currentIndex = 0;

        nameText.text = speaker;

        dialogPanel.SetActive(true);

        ShowCurrentDialog();
    }

    public void NextDialog()
    {
        if (dialogs == null || dialogs.Length == 0)
            return;

        currentIndex++;

        if (currentIndex >= dialogs.Length)
        {
            EndDialog();
            return;
        }

        ShowCurrentDialog();
    }

    void ShowCurrentDialog()
    {
        dialogText.text = dialogs[currentIndex];
    }

    void EndDialog()
    {
        dialogPanel.SetActive(false);

        if (currentDialogType == DialogType.Normal)
        {
            GameManager.Instance.DialogFinished();
        }
        else if (currentDialogType == DialogType.Green)
        {
            GameManager.Instance.GreenDialogFinished();
        }
        else if (currentDialogType == DialogType.Emergency)
        {
            //GameManager.Instance.EmergencyDialogFinished();
        }
    }
}