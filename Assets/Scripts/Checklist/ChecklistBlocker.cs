using UnityEngine;
using UnityEngine.EventSystems;

public class ChecklistBlocker : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        ChecklistManager.Instance.CloseChecklist();   
    }
}