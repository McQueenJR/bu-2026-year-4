using UnityEngine;
using UnityEngine.EventSystems;

public class AskQuestionPanelRightClickClose : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private NPCQuestionManager npcQuestionManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (npcQuestionManager != null)
            {
                npcQuestionManager.ClosePanel();
            }
        }
    }
}