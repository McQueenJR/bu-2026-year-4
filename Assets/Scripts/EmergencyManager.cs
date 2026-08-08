using UnityEngine;

public class EmergencyManager : MonoBehaviour
{
   // public GameObject emergencyDoor;
    public Animator doorAnimator;

    public GameManager gameManager;

    [Header("Button Visual")]
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Sprite coverClosedSprite;
    [SerializeField] private Sprite coverOpenSprite;

    private bool coverOpened = false;

    private void Start()
    {
        buttonRenderer.sprite = coverClosedSprite;
    }

    public void EmergencyButton()
    {
        // เช็คก่อนว่าปุ่มแดงกำลังทำงานอยู่ (currentState == Inspecting) เท่านั้นถึงกดปุ่มนี้ได้
        if (gameManager.currentState != GameManager.NPCState.Inspecting)
        {
            Debug.Log("ปุ่มแดงยังไม่ทำงาน กดฉุกเฉินไม่ได้");
            return;
        }

        if (gameManager.emergencyMode) return; // กันกดซ้ำหลัง active แล้ว

        if (!coverOpened)
        {
            coverOpened = true;
            buttonRenderer.sprite = coverOpenSprite;
            return;
        }

        //emergencyDoor.SetActive(true);
        gameManager.emergencyMode = true;
        doorAnimator.SetTrigger("CloseDoor");

        Debug.Log("Emergency Activated");
    }
}