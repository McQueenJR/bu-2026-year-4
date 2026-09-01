using UnityEngine;
using System;

// Helper กลาง - ใครลากได้ก็เรียกใช้ตัวนี้ร่วมกัน
// หน้าที่: บอกว่า "ตัวไหนถูกคลิกล่าสุด" ควรได้ sortingOrder สูงกว่าทุกตัวก่อนหน้า
public static class DraggableSortOrder
{
    private const int Step = 10;
    private const int MaxOrder = 100000; // เพดานกันไหลไม่มีที่สิ้นสุด (เผื่อไว้ กรณีไม่มีใครแจ้ง Close)

    private static int currentOrder = 0;
    private static int openCount = 0; // จำนวนของที่ลากได้ที่เปิดอยู่ตอนนี้

    public static event Action OnOrderOverflow;

    // เรียกทุกครั้งที่อยากเอาอะไรมาไว้หน้าสุด จะได้เลขที่สูงกว่าทุกตัวที่เคยเรียกมาก่อน
    public static int GetNextOrder()
    {
        currentOrder += Step;

        if (currentOrder >= MaxOrder)
        {
            currentOrder = Step;
            OnOrderOverflow?.Invoke();
        }

        return currentOrder;
    }

    // 👇 เรียกตอนของชิ้นนั้นถูกเปิดขึ้นมา (เช่น OpenInventory, Show)
    public static void NotifyOpened()
    {
        openCount++;
    }

    // 👇 เรียกตอนของชิ้นนั้นถูกปิด (เช่น CloseInventory, Hide)
    // ถ้าปิดหมดทุกชิ้นแล้ว (openCount กลับมา 0) ให้รีเซ็ต currentOrder ทันที
    public static void NotifyClosed()
    {
        openCount = Mathf.Max(0, openCount - 1);

        if (openCount == 0)
        {
            currentOrder = 0;
        }
    }
}

//กดเข้ามาห้ามลบนะค้าบบ มันคือจัดเลเยอร์ให้มาหน้าสุดของพวกpopup