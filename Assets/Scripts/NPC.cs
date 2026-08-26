using UnityEngine;
using UnityEngine.EventSystems;

public enum NPCType
{
    Villager,
    Robber,
    Monk
}

public class NPC : MonoBehaviour
{
    public NPCType npcType;
    public NPCData data;

    private void OnMouseUpAsButton()
    {
        // ถ้าเมาส์อยู่บน UI (โทรศัพท์, checklist, panel อื่นๆ) 
        // ไม่ต้องเปิด question panel ของ NPC
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (NPCQuestionManager.Instance != null)
        {
            NPCQuestionManager.Instance.TryOpenQuestionPanel(gameObject);
        }
    }
}