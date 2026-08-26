using UnityEngine;
using UnityEngine.EventSystems;

public class AskPanelBlocker : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("[" + Time.frameCount + "] Blocker PointerDOWN, enabled=" + enabled);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("[" + Time.frameCount + "] Blocker PointerUP, enabled=" + enabled);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[" + Time.frameCount + "] Blocker CLICK -> ปิดแผง, enabled=" + enabled);
        NPCQuestionManager.Instance.ClosePanel();
    }
}