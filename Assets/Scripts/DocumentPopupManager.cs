using UnityEngine;

public class DocumentPopupManager : MonoBehaviour
{
    public static DocumentPopupManager Instance;

    public GameObject popup;
    public Transform holder;

    [Header("กันเอกสารลากออกนอกจอ")]
    public BoxCollider2D documentDragBoundary;
    
    GameObject currentDocument;

    void Awake()
    {
        Instance = this;
        popup.SetActive(false);
    }

    public void Open(GameObject documentPrefab)
    {
        popup.SetActive(true);

        if (currentDocument != null)
            Destroy(currentDocument);

        currentDocument = Instantiate(documentPrefab, holder);
        
        DocumentDisplayClick dragScript = currentDocument.GetComponent<DocumentDisplayClick>();
        if (dragScript != null && documentDragBoundary != null)
        {
            dragScript.SetDragBoundary(documentDragBoundary);
        }
    }

    public void Close()
    {
        if (currentDocument != null)
            Destroy(currentDocument);

        popup.SetActive(false);
    }
}