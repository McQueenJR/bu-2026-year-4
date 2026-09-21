using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Camp
{
    public int campID;
    public string campName;

    // NPC ที่อยู่ในแคมป์นี้
    public List<TodayApplicant> members = new List<TodayApplicant>();
}

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;

    [Header("Camp ทั้งหมด")]
    public List<Camp> camps = new List<Camp>();

    private void Awake()
    {
        Instance = this;
    }

    // =========================================================
    // รีเซ็ต Camp ตอนเริ่มวันใหม่
    // =========================================================
    public void ResetCamps()
    {
        foreach (Camp camp in camps)
        {
            camp.members.Clear();
        }
    }

    // =========================================================
    // หา Camp จาก ID
    // =========================================================
    public Camp GetCamp(int campID)
    {
        foreach (Camp camp in camps)
        {
            if (camp.campID == campID)
                return camp;
        }

        return null;
    }

    // =========================================================
    // แจก Camp ให้ NPC ทุกคนของวันนั้น
    // =========================================================
    public void AssignCamp(List<TodayApplicant> applicants)
    {
        if (camps.Count == 0)
        {
            Debug.LogWarning("ไม่มี Camp");
            return;
        }

        ResetCamps();

        int campIndex = 0;

        foreach (TodayApplicant applicant in applicants)
        {
            Camp camp = camps[campIndex];

            applicant.campID = camp.campID;
            applicant.isAlive = true;
            applicant.hasEnteredCamp = false;

            camp.members.Add(applicant);

            string npcName = applicant.displayData != null
                ? applicant.displayData.npcName
                : applicant.npcData.name;

            Debug.Log($"{npcName} -> Camp {camp.campName}");

            campIndex++;

            if (campIndex >= camps.Count)
                campIndex = 0;
        }
    }

    // =========================================================
    // เพิ่ม NPC เข้า Camp (ใช้ตอน NPC ผ่านด่าน)
    // =========================================================
    public void AddApplicant(TodayApplicant applicant)
    {
        if (applicant == null)
            return;

        Camp camp = GetCamp(applicant.campID);

        if (camp == null)
        {
            Debug.LogWarning("ไม่พบ Camp ID : " + applicant.campID);
            return;
        }

        if (!camp.members.Contains(applicant))
        {
            camp.members.Add(applicant);
        }

        applicant.hasEnteredCamp = true;
    }

    // =========================================================
    // ดึงสมาชิกใน Camp เดียวกัน (ไม่รวมตัวเอง)
    // =========================================================
    public List<TodayApplicant> GetCampmates(TodayApplicant applicant)
    {
        List<TodayApplicant> result = new List<TodayApplicant>();

        if (applicant == null)
            return result;

        Camp camp = GetCamp(applicant.campID);

        if (camp == null)
            return result;

        foreach (TodayApplicant member in camp.members)
        {
            if (member != applicant)
                result.Add(member);
        }

        return result;
    }

    // =========================================================
    // สุ่มคนใน Camp เดียวกัน (ไว้ใช้โทรถาม)
    // =========================================================
    public TodayApplicant GetRandomCampmate(TodayApplicant applicant)
    {
        List<TodayApplicant> mates = GetCampmates(applicant);

        if (mates.Count == 0)
            return null;

        return mates[Random.Range(0, mates.Count)];
    }

    // =========================================================
    // Debug ดูสมาชิกทุก Camp
    // =========================================================
    public void PrintAllCamps()
    {
        foreach (Camp camp in camps)
        {
            Debug.Log("===== " + camp.campName + " =====");

            foreach (TodayApplicant npc in camp.members)
            {
                if (npc == null)
                {
                    Debug.Log("NULL NPC");
                    continue;
                }

                string npcName = npc.displayData != null
                    ? npc.displayData.npcName
                    : npc.npcData.name;

                Debug.Log($"{npcName} (Camp ID : {npc.campID})");
            }
        }
    }
    
    public void PrintEnteredCamp()
    {
        foreach (Camp camp in camps)
        {
            Debug.Log($"===== {camp.campName} =====");

            foreach (TodayApplicant npc in camp.members)
            {
                if (npc.hasEnteredCamp)
                {
                    Debug.Log($"[อยู่ในเต็นท์] {npc.displayData.npcName}");
                }
                else
                {
                    Debug.Log($"[ยังไม่เข้า] {npc.displayData.npcName}");
                }
            }
        }
    }
}