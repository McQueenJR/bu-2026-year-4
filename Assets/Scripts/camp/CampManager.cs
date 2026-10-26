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
        ghostEnteredRooms.Clear();
        ghostInRoom.Clear();
        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room == null) continue;

            foreach (CampResident resident in room.residents)
            {
                if (resident == null || resident.npcData == null) continue;
                resident.hasDocument = true;
            }
        }

        // รีเซ็ตทุกคนเป็น "ไม่อยู่ Camp" รอ SpawnManager มาเซ็ตทีหลัง
        foreach (NPCData npc in new List<NPCData>(homeTodayNPC.Keys))
            homeTodayNPC[npc] = false;

        Debug.Log($"===== CAMP DAY {day} =====");
        BuildRuntimeCamp();
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
        {
            Debug.LogError("📞 CampDatabase เป็น NULL");
            return null;
        }

        if (npc == null)
        {
            Debug.LogError("📞 NPCData ที่ส่งเข้ามาเป็น NULL");
            return null;
        }

        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room == null)
                continue;

            foreach (CampResident resident in room.residents)
            {
                if (resident == null || resident.npcData == null)
                    continue;

                if (resident.npcData == npc ||
                    resident.npcData.npcName == npc.npcName)
                {
                    return room;
                }
            }
        }

        Debug.LogError(
            $"📞 ❌ หา Room ของ [{npc.npcName}] ไม่เจอใน CampDatabase"
        );

        return null;
    }
    
    
// SpawnManager เรียกใช้เพื่อกำหนดสถานะแคมป์
    public void SetHomeToday(NPCData npc, bool isHome)
    {
        if (npc == null) return;

        NPCData key = FindResidentKey(npc);
        if (key == null)
        {
            Debug.LogWarning($"[Camp] SetHomeToday: ไม่พบ {npc.npcName}");
            return;
        }

        // คนตายอยู่แคมป์ไม่ได้
        homeTodayNPC[key] = aliveNPC[key] && isHome;
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

// ห้องนี้มีผีอยู่ → คนที่เพิ่งเข้ามาตายทันที
                    if (ghostEnteredRooms.Contains(room))
                    {
                        KillResident(
                            resident.npcData,
                            DeathCause.GhostTentKill,
                            $"ห้อง {room.roomCode} (เข้าตามหลังผี)"
                        );

                        Debug.Log($"💀 {resident.npcData.npcName} เข้าห้อง {room.roomCode} ที่มีผี → ตาย");
                    }

                    BuildRuntimeCamp();
                    PrintAllCamps();
                    return;
                   
                }
            }
        }

        Debug.LogError(
            $"❌ หา {npc.npcName} ใน CampManager ไม่เจอ"
        );
    }
// ตัวจริงอยู่ในแคมป์วันนี้ไหม (มีชีวิต + อยู่แคมป์)
    public bool IsRealInCamp(NPCData npc)
    {
        return npc != null && IsAlive(npc) && IsHomeToday(npc);
    }

// ในห้องของ NPC นี้ มีบัดดี้ (รูมเมต) อยู่แคมป์ตอนนี้ไหม
    public bool HasBuddyInCamp(NPCData npc)
    {
        CampRoom room = GetRoomOfNPC(npc);
        if (room == null) return false;

        foreach (CampResident r in room.residents)
        {
            if (r == null || r.npcData == null) continue;
            if (r.npcData == npc || r.npcData.npcName == npc.npcName) continue;

            if (IsAlive(r.npcData) && IsHomeToday(r.npcData))
                return true;
        }

        return false;
    }

// ลืมใบคำขอไว้ข้างในไหม (hasDocument = false คือลืม)
    public bool ForgotDocumentInCamp(NPCData npc)
    {
        return npc != null && !HasDocumentInCamp(npc);
    }
    
    
    // อ่านสถานะเอกสารในแคมป์แบบปลอดภัย (อ่านอย่างเดียว ไม่เขียนค่า)
    // คืน true  = พบ NPC ในฐานข้อมูล และ hasDocument ถูกใส่ใน out
    // คืน false = ไม่พบ NPC ในฐานข้อมูล (ต่างจาก ForgotDocumentInCamp ที่จะนับว่า "ลืม")
    public bool TryGetDocumentInCamp(NPCData npc, out bool hasDocument)
    {
        hasDocument = true;

        if (npc == null || campDatabase == null)
            return false;

        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room == null)
                continue;

            foreach (CampResident resident in room.residents)
            {
                if (resident == null || resident.npcData == null)
                    continue;

                if (resident.npcData == npc ||
                    resident.npcData.npcName == npc.npcName)
                {
                    hasDocument = resident.hasDocument;
                    return true;
                }
            }
        }

        return false;
    }
    
      // ==========================
    // NPC ตาย
    // คืน true = ตายจริงในครั้งนี้ | false = ไม่เจอในค่าย หรือตายไปแล้ว
    // ==========================
    public bool KillResident(
        NPCData npc,
        DeathCause cause = DeathCause.GhostTentKill,
        string detail = "")
    {
        if (npc == null)
            return false;

        // หา key ใน aliveNPC: reference ก่อน แล้วค่อยเทียบชื่อ
        NPCData key = FindResidentKey(npc);

        if (key == null)
        {
            Debug.LogWarning($"[Camp] KillResident: ไม่พบ {npc.npcName} ในค่าย");
            return false;
        }

        // กันนับซ้ำ
        if (!aliveNPC[key])
            return false;

        aliveNPC[key] = false;
        homeTodayNPC[key] = false;

        // นับ "ถูกผีฆ่าวันนี้" เฉพาะผีฆ่าในเต็นท์เท่านั้น
        if (cause == DeathCause.GhostTentKill)
            villagersKilledToday++;

        BuildRuntimeCamp();

        // ★ บันทึกสาเหตุการตายลงสมุดกลาง (ทิศทางเดียว Camp → DeathTracker)
        if (DeathTracker.Instance != null)
        {
            int day = GameManager.Instance != null
                ? GameManager.Instance.currentDay
                : 0;

            DeathTracker.Instance.RecordDeath(npc, cause, day, detail);
        }

        Debug.Log(
            $"💀 {npc.npcName} เสียชีวิตแล้ว ({cause}) " +
            $"| ผีฆ่าวันนี้ = {villagersKilledToday}"
        );

        return true;
    }

    private NPCData FindResidentKey(NPCData npc)
    {
        if (aliveNPC.ContainsKey(npc))
            return npc;

        foreach (NPCData k in aliveNPC.Keys)
        {
            if (k != null && k.npcName == npc.npcName)
                return k;
        }

        return null;
    }

    // ==========================
    // เช็กสถานะ
    // ==========================
    public bool IsAlive(NPCData npc)
    {
        if (npc == null)
            return false;

        // Prefab NPCData と CampDatabase の参照が異なる場合も、同じ NPC 名で照合する。
        NPCData key = FindResidentKey(npc);
        return key != null && aliveNPC.TryGetValue(key, out bool isAlive) && isAlive;
    }

    public bool IsHomeToday(NPCData npc)
    {
        if (npc == null) return false;

        // ตรวจด้วย reference ของ NPCData ก่อน
        if (homeTodayNPC.TryGetValue(npc, out bool isHome))
            return isHome;

        // ถ้าเป็น NPCData คนละ asset แต่ชื่อเดียวกัน ให้ค้นหาสำรอง
        foreach (KeyValuePair<NPCData, bool> entry in homeTodayNPC)
        {
            if (entry.Key != null &&
                entry.Key.npcName == npc.npcName)
            {
                return entry.Value;
            }
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

        ghostInRoom[room] = ghostTarget;

        Debug.Log(
            $"👻 Ghost เข้าห้อง {room.roomCode} แล้ว " +
            $"Target: {ghostTarget.npcName}"
        );

// อัปเดตสถานะ Runtime ทันที
        BuildRuntimeCamp();

// ให้ผีลงมือทันที ไม่รอจบวัน
        ResolveGhosts();
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
                KillResident(npc, DeathCause.GhostTentKill, $"ห้อง {room.roomCode}");

                Debug.Log(
                    $"💀 {npc.npcName} ถูก Ghost ฆ่า " +
                    $"ในห้อง {room.roomCode}"
                );
            }
        }

        // Ghost ถูก Resolve แล้ว
        

        PrintAllCamps();
    }
    
    
    
    
    // ==========================
// ตรวจสอบเอกสารของ NPC ใน Camp
// ==========================
    public bool HasDocumentInCamp(NPCData npc)
    {
        if (npc == null || campDatabase == null)
            return false;

        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room == null)
                continue;

            foreach (CampResident resident in room.residents)
            {
                if (resident == null ||
                    resident.npcData == null)
                    continue;

                if (resident.npcData == npc ||
                    resident.npcData.npcName == npc.npcName)
                {
                    return resident.hasDocument;
                }
            }
        }

        return false;
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
    public void SetDocumentInCamp(NPCData npc, bool hasDocument)
    {
        if (npc == null || campDatabase == null)
            return;

        foreach (CampRoom room in campDatabase.rooms)
        {
            if (room == null)
                continue;

            foreach (CampResident resident in room.residents)
            {
                if (resident == null || resident.npcData == null)
                    continue;

                if (resident.npcData == npc ||
                    resident.npcData.npcName == npc.npcName)
                {
                    resident.hasDocument = hasDocument;

                    Debug.Log(
                        $"📄 Camp Document | {resident.npcData.npcName} " +
                        $"| Has Document = {hasDocument}"
                    );

                    return;
                }
            }
        }

        Debug.LogWarning(
            $"ไม่พบ {npc.npcName} ใน CampDatabase"
        );
    }
    
    // นับชาวบ้านที่ยังมีชีวิตทั้งหมด (เงื่อนไขฉากจบ)
    public int GetAliveVillagerCount()
    {
        int count = 0;

        foreach (NPCData npc in aliveNPC.Keys)
        {
            if (aliveNPC[npc])
                count++;
        }

        return count;
    }
}