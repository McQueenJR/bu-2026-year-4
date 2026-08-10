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

    public void StartDialog(NPCData data)
    {
        if (data == null)
            return;

        dialogs = data.dialogs;
        currentIndex = 0;

        nameText.text = data.npcName;

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

        GameManager.Instance.DialogFinished();
    }
}