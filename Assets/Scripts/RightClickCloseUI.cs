using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickCloseUI : MonoBehaviour, IPointerClickHandler
{
    public GameObject panelToClose;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
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