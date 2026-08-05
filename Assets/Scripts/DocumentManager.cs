using UnityEngine;
using UnityEngine.UI;

public class DocumentManager : MonoBehaviour
{
    public GameObject panel;
    public Image documentImage;

    public Sprite[] documents;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void ShowDocument(int index)
    {
        documentImage.sprite = documents[index];
    }
}