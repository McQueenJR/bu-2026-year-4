using UnityEngine;

public class TempleDocumentPopup : MonoBehaviour
{
    public static TempleDocumentPopup Instance;

    public GameObject popupPanel;
    public Transform documentContainer;

    private GameObject currentDocument;

    private void Awake()
    {
        Instance = this;
        popupPanel.SetActive(false);
    }

    public void Show(GameObject documentPrefab)
    {
        popupPanel.SetActive(true);

        if (currentDocument != null)
            Destroy(currentDocument);

        currentDocument = Instantiate(
            documentPrefab,
            documentContainer
        );

        currentDocument.transform.localPosition = Vector3.zero;
        currentDocument.transform.localRotation = Quaternion.identity;
        currentDocument.transform.localScale = Vector3.one;
    }

    public void Hide()
    {
        if (currentDocument != null)
            Destroy(currentDocument);

        popupPanel.SetActive(false);
    }
}