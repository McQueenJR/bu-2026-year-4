using UnityEngine;
using System.Collections.Generic;

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;

    [Header("Camp Database")]
    public CampDatabase campDatabase;
    
    [Header("Runtime Debug")]
    public bool showRuntime = true;

    // สถานะระหว่างเล่นเกม
    private Dictionary<NPCData, bool> aliveNPC = new();
    private Dictionary<NPCData, bool> homeTodayNPC = new();
    private List<CampRoom> ghostEnteredRooms = new();

    private Dictionary<CampRoom, NPCData> ghostInRoom = new();

    [Header("Runtime Camp Status")]
    [SerializeField]
    private List<CampRuntimeRoom> runtimeRooms =
        new List<CampRuntimeRoom>();
    
    [Header("Daily Camp Stats")]
    public int villagersKilledToday = 0;

    
    [System.Serializable]
    public class CampRuntimeRoom
    {
        public string roomCode;
        public string phoneNumber;

        public List<CampRuntimeResident> residents =
            new List<CampRuntimeResident>();

        // Ghost ที่อยู่ในห้องนี้ตอนนี้
        public bool hasGhost;
        public NPCData ghostTarget;
    }

    [System.Serializable]
    public class CampRuntimeResident
    {
        public NPCData npcData;

        public bool isAlive;
        public bool isHomeToday;
    }
    
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
        
        villagersKilledToday = 0;
        Debug.Log($"===== CAMP DAY {day} =====");

        // NPC ที่ยังมีชีวิต
        List<NPCData> aliveList = new List<NPCData>();

        foreach (NPCData npc in aliveNPC.Keys)
        {
            if (aliveNPC[npc])
            {
                aliveList.Add(npc);
            }
            else
            {
                // NPC ที่ตายแล้ว
                homeTodayNPC[npc] = false;
            }
        }

        // จำนวน NPC ที่จะออกมาวัดวันนี้ = 9-11 ตัว
        int spawnCount = Random.Range(9, Mathf.Min(11, aliveList.Count) + 1);

        // สุ่มลำดับ NPC
        for (int i = 0; i < aliveList.Count; i++)
        {
            int randomIndex = Random.Range(i, aliveList.Count);

            NPCData temp = aliveList[i];
            aliveList[i] = aliveList[randomIndex];
            aliveList[randomIndex] = temp;
        }

        // กำหนดสถานะ
        for (int i = 0; i < aliveList.Count; i++)
        {
            NPCData npc = aliveList[i];

            bool comeToTemple = i < spawnCount;

            homeTodayNPC[npc] = !comeToTemple;

            Debug.Log(
                $"{npc.npcName} : " +
                (comeToTemple ? "ออกมาวัด" : "อยู่ Camp")
            );
        }

        Debug.Log(
            $"วันนี้ NPC มีชีวิต = {aliveList.Count} | " +
            $"ออกมาวัด = {spawnCount} | " +
            $"อยู่ Camp = {aliveList.Count - spawnCount}"
        );

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
        if (campDatabase == null || npc == null)
            return null;

        foreach (CampRoom room in campDatabase.rooms)
        {
            foreach (CampResident resident in room.residents)
            {
                if (resident.npcData == null)
                    continue;

                // เช็กจาก Reference ก่อน
                if (resident.npcData == npc)
                    return room;

                // ถ้า Reference ไม่ตรง ให้เช็กจากชื่อ
                if (resident.npcData.npcName == npc.npcName)
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
                if (resident.npcData == null)
                    continue;

                // Reference ตรงกัน หรือชื่อเดียวกัน
                if (resident.npcData == npc ||
                    resident.npcData.npcName == npc.npcName)
                {
                    // ใช้ NPCData ของ Camp เป็น key เสมอ
                    homeTodayNPC[resident.npcData] = true;

                    Debug.Log(
                        $"🏕️ {resident.npcData.npcName} เข้า Camp " +
                        $"ห้อง {room.roomCode} → 🟢 อยู่ Camp"
                    );

                    PrintAllCamps();
                    return;
                }
            }
        }

        Debug.LogError(
            $"❌ หา {npc.npcName} ใน CampManager ไม่เจอ"
        );
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

        // กันนับซ้ำ
        if (!aliveNPC[npc])
            return;

        aliveNPC[npc] = false;
        homeTodayNPC[npc] = false;

        villagersKilledToday++;

        Debug.Log(
            $"💀 {npc.npcName} เสียชีวิตแล้ว " +
            $"| ฆ่าวันนี้ = {villagersKilledToday}"
        );
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
    
    void OnGUI()
    {
        if (!showRuntime || campDatabase == null)
            return;

        int day = 0;

        if (GameManager.Instance != null)
            day = GameManager.Instance.currentDay;

        GUI.Box(
            new Rect(10, 10, 430, 700),
            $"CAMP RUNTIME  |  DAY {day}"
        );

        float y = 40;

        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room == null)
                continue;

            GUI.Label(
                new Rect(20, y, 400, 22),
                $"🏕️ {room.roomCode}    ☎ {room.phoneNumber}"
            );

            y += 23;

            foreach (CampResident resident in room.residents)
            {
                if (resident == null || resident.npcData == null)
                    continue;

                NPCData npc = resident.npcData;

                string status;

                if (!IsAlive(npc))
                {
                    status = "🔴 DEAD";
                }
                else if (IsHomeToday(npc))
                {
                    status = "🟢 อยู่ Camp";
                }
                else
                {
                    status = "⚪ ออกไปวัด";
                }

                GUI.Label(
                    new Rect(40, y, 370, 22),
                    $"{npc.npcName}   →   {status}"
                );

                y += 22;
            }

            y += 10;
        }
    }
    public void BuildRuntimeCamp()
    {
        runtimeRooms.Clear();

        if (campDatabase == null)
            return;

        foreach (CampRoom room in campDatabase.rooms)
        {
            CampRuntimeRoom runtimeRoom = new CampRuntimeRoom();

            runtimeRoom.roomCode = room.roomCode;
            runtimeRoom.phoneNumber = room.phoneNumber;

            foreach (CampResident resident in room.residents)
            {
                if (resident.npcData == null)
                    continue;

                NPCData npc = resident.npcData;

                CampRuntimeResident runtimeResident =
                    new CampRuntimeResident();

                runtimeResident.npcData = npc;
                runtimeResident.isAlive = IsAlive(npc);
                runtimeResident.isHomeToday = IsHomeToday(npc);

                runtimeRoom.residents.Add(runtimeResident);
            }

            // เช็กว่า Room นี้มี Ghost หรือไม่
            if (ghostInRoom.ContainsKey(room))
            {
                runtimeRoom.hasGhost = true;
                runtimeRoom.ghostTarget = ghostInRoom[room];
            }

            runtimeRooms.Add(runtimeRoom);
        }
    }
    // ==========================
// ตรวจสอบ Ghost ตอนท้าย
// ==========================
    
    
    public void GhostEnterCamp(NPCData ghostTarget)
    {
   
        if (campDatabase == null)
        {
            Debug.LogWarning("ไม่มี CampDatabase");
            return;
        }

        if (ghostTarget == null)
        {
            Debug.LogWarning("Ghost ไม่มี ghostTarget");
            return;
        }

        // หา Room ของ NPC ที่ Ghost ปลอมเป็น
        CampRoom room = GetRoomOfNPC(ghostTarget);

        if (room == null)
        {
            Debug.LogWarning(
                $"ไม่พบห้องของ Ghost Target: {ghostTarget.npcName}"
            );

            return;
        }

        // จำว่า Ghost เข้า Room นี้แล้ว
        if (!ghostEnteredRooms.Contains(room))
        {
            ghostEnteredRooms.Add(room);
        }

        Debug.Log(
            $"👻 Ghost เข้าห้อง {room.roomCode} แล้ว " +
            $"Target: {ghostTarget.npcName}"
        );
    }
    public void ResolveGhosts()
    {
        if (ghostEnteredRooms == null ||
            ghostEnteredRooms.Count == 0)
        {
            return;
        }

        Debug.Log("===== RESOLVE GHOSTS =====");

        foreach (CampRoom room in ghostEnteredRooms)
        {
            if (room == null)
                continue;

            Debug.Log($"👻 Ghost ตรวจห้อง {room.roomCode}");

            foreach (CampResident resident in room.residents)
            {
                if (resident.npcData == null)
                    continue;

                NPCData npc = resident.npcData;

                // ตายไปแล้ว → ข้าม
                if (!IsAlive(npc))
                    continue;

                // วันนี้ไม่ได้อยู่ Camp → ไม่โดนฆ่า
                if (!IsHomeToday(npc))
                    continue;

                // อยู่ Camp + ยังมีชีวิต = ถูก Ghost ฆ่า
                KillResident(npc);

                Debug.Log(
                    $"💀 {npc.npcName} ถูก Ghost ฆ่า " +
                    $"ในห้อง {room.roomCode}"
                );
            }
        }

        // Ghost ถูก Resolve แล้ว
        ghostEnteredRooms.Clear();

        PrintAllCamps();
    }
    public int GetTodayTempleNPCCount()
    {
        int count = 0;

        foreach (NPCData npc in aliveNPC.Keys)
        {
            if (aliveNPC[npc] && !IsHomeToday(npc))
            {
                count++;
            }
        }

        return count;
    }
}