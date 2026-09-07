
using UnityEngine;

public class NPCAnomaly : MonoBehaviour
{
    // NPC ที่กำลังถูก Spawn และใช้งานอยู่ตอนนี้
    public static NPCAnomaly CurrentNPC { get; private set; }

    [Header("ตา")]
    [SerializeField] private SpriteRenderer eye;

    [Header("ปากปกติ")]
    [SerializeField] private GameObject normalMouth;

    [Header("ฟันแหลม")]
    [SerializeField] private GameObject sharpTeeth;

    [Header("สีตาปกติ")]
    [SerializeField] private Color normalEyeColor = Color.white;

    [Header("สีตาตอนใช้ไม้ขีด")]
    [SerializeField] private Color redEyeColor = Color.red;

    private void Awake()
    {
        // ตัวนี้คือ NPC ที่ถูก Spawn ล่าสุด
        CurrentNPC = this;
    }

    private void Start()
    {
        HideAnomaly();
    }

    public void ShowAnomaly()
    {
        // เปลี่ยนตาเป็นสีแดง
        if (eye != null)
        {
            eye.color = redEyeColor;
        }

        // ปิดปากปกติ
        if (normalMouth != null)
        {
            normalMouth.SetActive(false);
        }

        // เปิดฟันแหลม
        if (sharpTeeth != null)
        {
            sharpTeeth.SetActive(true);
        }

        Debug.Log("NPC → แสดงความผิดปกติ");
    }

    public void HideAnomaly()
    {
        // กลับตาเป็นสีปกติ
        if (eye != null)
        {
            eye.color = normalEyeColor;
        }

        // เปิดปากปกติ
        if (normalMouth != null)
        {
            normalMouth.SetActive(true);
        }

        // ปิดฟันแหลม
        if (sharpTeeth != null)
        {
            sharpTeeth.SetActive(false);
        }

        Debug.Log("NPC → ซ่อนความผิดปกติ");
    }

    private void OnDestroy()
    {
        if (CurrentNPC == this)
        {
            CurrentNPC = null;
        }
    }
}
