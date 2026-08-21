using UnityEngine;

public class ApplicantPhotoUI : MonoBehaviour
{
    public static ApplicantPhotoUI Instance;

    public GameObject panel;
    public Transform photoContainer;

    private GameObject currentPhoto;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowPhoto(NPCData data)
    {
        if (data == null) return;

        if (data.applicantPhotoPrefab == null) return;

        panel.SetActive(true);

        if (currentPhoto != null)
            Destroy(currentPhoto);

        currentPhoto = Instantiate(
            data.applicantPhotoPrefab,
            photoContainer
        );

        currentPhoto.transform.localPosition = Vector3.zero;
        currentPhoto.transform.localRotation = Quaternion.identity;
        currentPhoto.transform.localScale = Vector3.one;
    }

    public void ClosePanel()
    {
        if (currentPhoto != null)
            Destroy(currentPhoto);

        panel.SetActive(false);
    }
}