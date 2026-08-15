using UnityEngine;

public class EmergencyManager : MonoBehaviour
{
    public Animator doorAnimator;

    public GameManager gameManager;

    [Header("Button Visual")]
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Sprite coverClosedSprite;   // ฝาปิด — สถานะเริ่มต้น และหลังประตูเปิดสำเร็จ
    [SerializeField] private Sprite coverOpenSprite;     // ฝาเปิด — ตั้งแต่กด arm จนถึงประตูปิดค้างอยู่

    private bool coverOpened = false;   // สถานะฝา (arm) — ใช้เฉพาะตอนประตูเปิดอยู่
    private bool isDoorClosed = false;  // สถานะประตูจริง

    private void Start()
    {
        buttonRenderer.sprite = coverClosedSprite;
    }

    public void EmergencyButton()
    {
        // กันกดระหว่าง police sequence กำลังทำงานอยู่
        if (gameManager.isPoliceSequenceActive)
        {
            Debug.Log("กำลังอยู่ระหว่างเรียกตำรวจ กดฉุกเฉินไม่ได้ตอนนี้");
            return;
        }

        // กดได้เฉพาะตอนปุ่มแดง active
        if (gameManager.currentState != GameManager.NPCState.Inspecting)
        {
            Debug.Log("ปุ่มแดงยังไม่ทำงาน กดฉุกเฉินไม่ได้");
            return;
        }

        if (!isDoorClosed)
        {
            // ประตูเปิดอยู่ → ต้องผ่าน 2-step confirm ก่อนสั่งปิด
            if (!coverOpened)
            {
                coverOpened = true;
                buttonRenderer.sprite = coverOpenSprite;
                return;
            }

            // กดรอบสอง → ปิดประตู
            coverOpened = false;
            isDoorClosed = true;

            gameManager.emergencyMode = true;
            doorAnimator.SetTrigger("CloseDoor");

            // 🔥 เรียก Dialog ตาม NPC
            gameManager.StartEmergencyDialog();

            Debug.Log("Emergency Activated - Door Closed");
        }
        else
        {
            // ประตูปิดอยู่ → กดครั้งเดียวจบ สั่งเปิดทันที ไม่ต้อง 2-step
            isDoorClosed = false;

            gameManager.emergencyMode = false;
            doorAnimator.SetTrigger("OpenDoor");

            buttonRenderer.sprite = coverClosedSprite;

            Debug.Log("Emergency Deactivated - Door Open");
        }
    }

    // เปิดประตูแบบบังคับจากระบบอื่น (เช่นหลังโทร 191 สำเร็จ) โดยไม่ต้องผ่านการกดปุ่ม
    public void ForceOpenDoor()
    {
        isDoorClosed = false;
        coverOpened = false;

        gameManager.emergencyMode = false;
        doorAnimator.SetTrigger("OpenDoor");

        buttonRenderer.sprite = coverClosedSprite;

        Debug.Log("Door force-opened after police call");
    }
}