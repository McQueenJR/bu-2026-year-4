using System.Collections.Generic;
using UnityEngine;

// จุดเดียวที่อ่านสถานะเกมเพื่อสร้างเนื้อหาหนังสือพิมพ์
// อ่านอย่างเดียว ไม่แก้สถานะใดๆ ของเกม
public static class NewspaperContentBuilder
{
    public static NewspaperRuntime Build(
        NewspaperData data,
        int day,
        NewspaperMissingDatabase missingDb,
        string dateFormat,
        int firstDateNumber)
    {
        NewspaperRuntime result = new NewspaperRuntime();

        result.day = day;
        result.dateText = FormatDate(dateFormat, day, firstDateNumber);
        result.news = BuildNews(data, day);
        result.missing = BuildMissing(data, day, missingDb);
        result.report = BuildReport(data, day);

        return result;
    }

    // =====================================================
    // บรรทัดวันที่
    // =====================================================
    private static string FormatDate(string format, int day, int firstDateNumber)
    {
        int date = firstDateNumber + day - 1;

        if (string.IsNullOrEmpty(format))
            return date.ToString();

        return format
            .Replace("{date}", date.ToString())
            .Replace("{day}", day.ToString());
    }

    // =====================================================
    // ช่อง 1 : ข่าวจากหอกระจายข่าว
    // =====================================================
    private static NewspaperStoryBlock BuildNews(NewspaperData data, int day)
    {
        if (!data.useDeathHistoryForNews)
            return data.news ?? new NewspaperStoryBlock();

        // ตั้งแต่เริ่มเกมจนถึงเมื่อวาน มีชาวบ้านตัวจริงตาย (ทุกสาเหตุ) หรือไม่
        bool anyDeath = HasAnyVillagerDeathBefore(day);

        NewspaperStoryBlock block = anyDeath ? data.newsIfAnyDeath : data.newsIfNoDeath;

        Debug.Log($"[Newspaper] Day {day} ช่อง 1 | มีชาวบ้านตายแล้ว = {anyDeath}");

        if (block == null || block.IsEmpty)
        {
            Debug.LogWarning(
                $"[Newspaper] Day {day} ยังไม่ได้ใส่ข้อความช่อง 1 ของกรณี " +
                $"{(anyDeath ? "มีคนตาย" : "ไม่มีคนตาย")} → ใช้ข้อความปกติแทน"
            );

            return data.news ?? new NewspaperStoryBlock();
        }

        return block;
    }

    private static bool HasAnyVillagerDeathBefore(int day)
    {
        if (DeathTracker.Instance == null)
            return false;

        // ทุกรายการใน DeathTracker เป็นชาวบ้านตัวจริง (ผีไม่ถูกบันทึกที่นี่)
        foreach (DeathRecord r in DeathTracker.Instance.GetRecords())
        {
            if (r != null && r.day < day)
                return true;
        }

        return false;
    }

    // =====================================================
    // ช่อง 2 : หมวดคนสูญหาย
    // ชาวบ้านที่หายสาบสูญจากปุ่มแดง "เมื่อวาน" เท่านั้น
    // =====================================================
    private static NewspaperStoryBlock BuildMissing(
        NewspaperData data,
        int day,
        NewspaperMissingDatabase missingDb)
    {
        NewspaperStoryBlock none = NewspaperStoryBlock.Of(data.noMissingText);

        if (day <= 1 || DeathTracker.Instance == null)
            return none;

        DeathRecord found = null;
        int count = 0;

        foreach (DeathRecord r in DeathTracker.Instance.GetRecords())
        {
            if (r == null || r.npcData == null)
                continue;

            if (r.cause != DeathCause.VanishedAfterReject)
                continue;

            if (r.day != day - 1)
                continue;

            count++;

            if (found == null)
                found = r;
        }

        if (found == null)
            return none;

        if (count > 1)
        {
            Debug.LogWarning(
                $"[Newspaper] Day {day} มีชาวบ้านหายสาบสูญเมื่อวาน {count} คน " +
                $"แต่หนังสือพิมพ์แสดงได้ 1 คน → แสดง {found.npcData.npcName}"
            );
        }

        Debug.Log($"[Newspaper] Day {day} ช่อง 2 | คนสูญหาย = {found.npcData.npcName}");

        if (missingDb == null)
        {
            Debug.LogWarning("[Newspaper] ไม่ได้ใส่ Missing Database → ใช้ข้อความสำรอง");
            return NewspaperStoryBlock.Of($"{found.npcData.npcName} หายตัวไป");
        }

        return missingDb.Get(found.npcData);
    }

    // =====================================================
    // ช่อง 3 : รายงานจากชาวบ้าน
    // =====================================================
    private static NewspaperStoryBlock BuildReport(NewspaperData data, int day)
    {
        if (!data.useRandomAliveVillagerEvent)
            return data.report ?? new NewspaperStoryBlock();

        if (data.villagerEvents == null || data.villagerEvents.Length == 0)
        {
            Debug.LogWarning(
                $"[Newspaper] Day {day} เปิดโหมดสุ่มเหตุการณ์ แต่ไม่มีรายการเหตุการณ์ → ใช้ข้อความปกติ"
            );

            return data.report ?? new NewspaperStoryBlock();
        }

        // เก็บเฉพาะเหตุการณ์ของคนที่ "ยังมีชีวิต" (อ่านจากสมุดการตาย ไม่ดู GameObject)
        List<NewspaperVillagerEvent> alive = new List<NewspaperVillagerEvent>();

        foreach (NewspaperVillagerEvent ev in data.villagerEvents)
        {
            if (ev == null || ev.villager == null || ev.report == null)
                continue;

            if (IsVillagerDead(ev.villager))
                continue;

            alive.Add(ev);
        }

        Debug.Log(
            $"[Newspaper] Day {day} ช่อง 3 | ชาวบ้านที่ยังมีชีวิตใน {data.villagerEvents.Length} เหตุการณ์ = {alive.Count}"
        );

        if (alive.Count > 0)
        {
            NewspaperVillagerEvent picked = alive[Random.Range(0, alive.Count)];

            Debug.Log($"[Newspaper] Day {day} ช่อง 3 | สุ่มได้เหตุการณ์ของ {picked.villager.npcName}");

            return picked.report;
        }

        // ตายหมดทุกคน → กรณีพิเศษ
        if (data.reportIfAllDead != null && !data.reportIfAllDead.IsEmpty)
            return data.reportIfAllDead;

        Debug.LogWarning(
            $"[Newspaper] Day {day} ชาวบ้านตายหมดแต่ไม่ได้ใส่ข้อความกรณีพิเศษ → ใช้ข้อความปกติ"
        );

        return data.report ?? new NewspaperStoryBlock();
    }

    private static bool IsVillagerDead(NPCData npc)
    {
        return DeathTracker.Instance != null && DeathTracker.Instance.IsDead(npc);
    }
}