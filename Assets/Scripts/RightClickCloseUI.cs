using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickCloseUI : MonoBehaviour, IPointerClickHandler
{
    public GameObject panelToClose;

    [Header("Phone Reset (Optional)")]
    public PhoneManager phoneManager;
    public bool finishPhoneCall = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // ถ้าเป็นการปิดหน้าต่างโทรศัพท์ ให้จบสายก่อน
            if (finishPhoneCall && phoneManager != null)
            {
                if (phoneManager.phoneDialer != null &&
                    phoneManager.phoneDialer.IsCalling)
                {
                    phoneManager.phoneDialer.FinishCalling();
                }
            }

            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}