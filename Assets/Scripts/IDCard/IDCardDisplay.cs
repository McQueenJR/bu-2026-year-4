using UnityEngine;

public class IDCardDisplay : MonoBehaviour
{
    public GameObject displayPrefab;

    public void ShowCard()
    {
        IDCardPopup.Instance.Show(displayPrefab);
    }
}