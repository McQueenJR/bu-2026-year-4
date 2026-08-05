using TMPro;
using UnityEngine;

public class DocumentManager : MonoBehaviour
{
    public GameObject panel;

    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text villageText;

    public void ShowDocument(GameObject npc)
    {
        NPCData data = npc.GetComponent<NPCData>();

        nameText.text = data.npcName;
        ageText.text = data.age.ToString();
        villageText.text = data.village;

        panel.SetActive(true);
    }

    public void CloseDocument()
    {
        panel.SetActive(false);
    }
}