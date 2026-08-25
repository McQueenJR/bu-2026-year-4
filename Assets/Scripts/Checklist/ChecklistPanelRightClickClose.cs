using UnityEngine;
using UnityEngine.EventSystems;

public class ChecklistPanelRightClickClose : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ChecklistManager checklistManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        // คลิกขวา
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (checklistManager != null)
            {
                checklistManager.CloseChecklist();
            }
        }
    }
}