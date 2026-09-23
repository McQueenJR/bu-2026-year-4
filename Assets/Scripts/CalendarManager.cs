using UnityEngine;

public class CalendarManager : MonoBehaviour
{
    [Header("Calendar Sprite")]
    public SpriteRenderer calendarRenderer;   // GameObject SpriteRenderer

    [Header("Day 1 - Day 7")]
    public Sprite[] daySprites;               // ใส่ Sprite 7 รูป

    void Awake()
    {
        if (calendarRenderer == null)
            calendarRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetDay(int day)
    {
        if (calendarRenderer == null)
        {
            Debug.LogError("Calendar SpriteRenderer ไม่ได้ใส่");
            return;
        }

        int index = day - 1;

        if (index < 0 || index >= daySprites.Length)
        {
            Debug.LogWarning("ไม่มี Sprite ของ Day " + day);
            return;
        }

        calendarRenderer.sprite = daySprites[index];

        Debug.Log("Calendar เปลี่ยนเป็น Day " + day);
    }
}