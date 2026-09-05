using UnityEngine;

public class XRayDetector : MonoBehaviour
{
    [Header("X-Ray System")]
    [SerializeField] private XRaySystem xraySystem;

    private NPCXRayTarget detectedTarget;


    private void Start()
    {
        if (xraySystem == null)
        {
            xraySystem = FindFirstObjectByType<XRaySystem>();
        }
    }


    // ==========================================
    // NPC เข้ามาในวงแว่น
    // ==========================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        NPCXRayTarget target =
            other.GetComponentInParent<NPCXRayTarget>();

        if (target == null)
            return;


        detectedTarget = target;


        if (xraySystem != null)
        {
            xraySystem.SetTarget(target);
        }


        Debug.Log(
            "X-Ray ตรวจพบ NPC: " +
            target.gameObject.name
        );
    }


    // ==========================================
    // NPC ออกจากวงแว่น
    // ==========================================

    private void OnTriggerExit2D(Collider2D other)
    {
        NPCXRayTarget target =
            other.GetComponentInParent<NPCXRayTarget>();

        if (target == null)
            return;


        if (target == detectedTarget)
        {
            detectedTarget = null;


            if (xraySystem != null)
            {
                xraySystem.ClearTarget(target);
            }


            Debug.Log(
                "NPC ออกจากพื้นที่ X-Ray"
            );
        }
    }
}