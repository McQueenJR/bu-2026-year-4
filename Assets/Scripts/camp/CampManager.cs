using UnityEngine;
using System.Collections.Generic;

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;

    [Header("Camp Database")]
    public CampDatabase campDatabase;

    // สถานะระหว่างเล่นเกม
    private Dictionary<NPCData, bool> aliveNPC = new();
    private Dictionary<NPCData, bool> homeTodayNPC = new();

    void Awake()
    {
        Instance = this;
        InitializeCampStatus();
    }

    // ==========================
    // สร้างสถานะเริ่มต้น
    // ==========================
    void InitializeCampStatus()
    {
        if (campDatabase == null)
        {
            Debug.LogError("Camp Database ยังไม่ได้ใส่");
            return;
        }

        aliveNPC.Clear();
        homeTodayNPC.Clear();

        foreach (CampRoom room in campDatabase.rooms)
        {
            foreach (CampResident resident in room.residents)
            {
                if (resident.npcData == null)
                    continue;

                aliveNPC[resident.npcData] = true;
                homeTodayNPC[resident.npcData] = false;
            }
        }
    }

    // ==========================
    // เริ่มวันใหม่
    // ==========================
    public void StartNewDay(int day)
    {
        Debug.Log($"===== CAMP DAY {day} =====");

        foreach (NPCData npc in new List<NPCData>(aliveNPC.Keys))
        {
            // คนตายไม่อยู่ Camp เสมอ
            if (!aliveNPC[npc])
            {
                homeTodayNPC[npc] = false;
                continue;
            }

            // เริ่มวันใหม่ สุ่มว่าอยู่ Camp วันนี้ไหม
            bool stayHome = Random.value < 0.6f;
            homeTodayNPC[npc] = stayHome;

            Debug.Log($"{npc.npcName} : {(stayHome ? "อยู่ Camp" : "ไม่อยู่ Camp")}");
        }

        PrintAllCamps();
    }

    // ==========================
    // หา Room จากเบอร์โทร
    // ==========================
    public CampRoom GetRoomByPhone(string phoneNumber)
    {
        if (campDatabase == null)
            return null;

        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room.phoneNumber == phoneNumber)
                return room;
        }

        return null;
    }

    // ==========================
    // หา Room ของ NPC
    // ==========================
    public CampRoom GetRoomOfNPC(NPCData npc)
    {
        if (campDatabase == null)
            return null;

        foreach (CampRoom room in campDatabase.rooms)
        {
            foreach (CampResident resident in room.residents)
            {
                if (resident.npcData == npc)
                    return room;
            }
        }

        return null;
    }

    // ==========================
    // คนที่อยู่ Camp วันนี้
    // ==========================
    public List<CampResident> GetResidentsAtHome(string phoneNumber)
    {
        List<CampResident> result = new();

        CampRoom room = GetRoomByPhone(phoneNumber);

        if (room == null)
            return result;

        foreach (CampResident resident in room.residents)
        {
            if (resident.npcData == null)
                continue;

            if (aliveNPC[resident.npcData] &&
                homeTodayNPC[resident.npcData])
            {
                result.Add(resident);
            }
        }

        return result;
    }

    // ==========================
    // รูมเมตของ NPC
    // ==========================
    public List<CampResident> GetRoommates(NPCData npc)
    {
        List<CampResident> roommates = new();

        CampRoom room = GetRoomOfNPC(npc);

        if (room == null)
            return roommates;

        foreach (CampResident resident in room.residents)
        {
            if (resident.npcData != npc)
                roommates.Add(resident);
        }

        return roommates;
    }

    // ==========================
    // สุ่มคนรับสาย
    // ==========================
    public CampResident GetAnsweringResident(string phoneNumber)
    {
        List<CampResident> residents = GetResidentsAtHome(phoneNumber);

        if (residents.Count == 0)
            return null;

        return residents[Random.Range(0, residents.Count)];
    }

    // ==========================
    // NPC ผ่านด่าน -> เข้า Camp วันนี้
    // ==========================
    public void EnterCampToday(NPCData npc)
    {
        if (npc == null || campDatabase == null)
            return;

        foreach (CampRoom room in campDatabase.rooms)
        {
            foreach (CampResident resident in room.residents)
            {
                // เทียบจากชื่อ NPC
                if (resident.npcData != null &&
                    resident.npcData.npcName == npc.npcName)
                {
                    homeTodayNPC[resident.npcData] = true;

                    Debug.Log($"{resident.npcData.npcName} เข้า Camp ห้อง {room.roomCode}");
                    return;
                }
            }
        }

        Debug.LogError($"หา {npc.npcName} ใน CampManager ไม่เจอ");
    }

    // ==========================
    // NPC ตาย
    // ==========================
    public void KillResident(NPCData npc)
    {
        if (npc == null)
            return;

        if (!aliveNPC.ContainsKey(npc))
            return;

        aliveNPC[npc] = false;
        homeTodayNPC[npc] = false;

        Debug.Log($"{npc.npcName} เสียชีวิตแล้ว");
    }

    // ==========================
    // เช็กสถานะ
    // ==========================
    public bool IsAlive(NPCData npc)
    {
        return aliveNPC.ContainsKey(npc) && aliveNPC[npc];
    }

    public bool IsHomeToday(NPCData npc)
    {
        if (npc == null) return false;

        foreach (NPCData key in homeTodayNPC.Keys)
        {
            if (key.npcName == npc.npcName)
                return homeTodayNPC[key];
        }

        return false;
    }

    // ==========================
    // Debug
    // ==========================
    public void PrintAllCamps()
    {
        if (campDatabase == null)
            return;

        foreach (CampRoom room in campDatabase.rooms)
        {
            Debug.Log($"===== {room.roomCode} ({room.phoneNumber}) =====");

            foreach (CampResident resident in room.residents)
            {
                if (resident.npcData == null)
                    continue;

                string alive = IsAlive(resident.npcData) ? "Alive" : "Dead";
                string home = IsHomeToday(resident.npcData) ? "อยู่ Camp" : "ไม่อยู่ Camp";

                Debug.Log($"{resident.npcData.npcName} | {alive} | {home}");
            }
        }
    }
}