using UnityEngine;
using UnityEngine.EventSystems;

public class PhonePanelRightClickClose : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PhoneManager phoneManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (phoneManager != null)
            {
                phoneManager.ClosePhone();
            }
            else
            {
                Debug.LogError("ไม่ได้ใส่ PhoneManager ใน PhonePanelRightClickClose");
            }
        }
    }
}