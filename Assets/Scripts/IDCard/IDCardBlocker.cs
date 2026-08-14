using UnityEngine;

public class IDCardBlocker : MonoBehaviour
{
    private void OnMouseDown()
    {
        IDCardPopup.Instance.Hide();
    }
}