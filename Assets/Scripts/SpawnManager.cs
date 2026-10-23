
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    [Header("NPC Database")]
    public AllDataPrefabNPC allData;

    [Header("Spawn Point")]
    public Transform spawnPoint;
    public Transform stopPoint;

    [Header("References")]
    public GameManager gameManager;
    public TodayListManager todayListManager;
    
    
    [Header("Today List + Ghost Settings")]
    [Min(0)]
    public int todayListPlusGhostCount = 8;   // จริงจาก List + ผี รวมกัน
    
    [Header("Forgot Document")]
    [Range(0f, 1f)]
    public float forgotDocumentChance = 0.3f;

    private DailyPlan currentPlan;
    // NPC ที่จัดไว้สำหรับวันนี้
    public List<TodayApplicant> todayApplicants = new();
    
    private Dictionary<DataPrefabNPC, HashSet<GameObject>> usedPrefabsPerNPC = new();
    
    private int currentApplicantIndex = 0;
   
    // =========================================================
    // เริ่มวันใหม่
    // =========================================================
   private class DailyPlan
{
    public List<DataPrefabNPC> listDPN = new();
    public List<DataPrefabNPC> extraDPN = new();
    public List<DataPrefabNPC> campDPN = new();
    
    public List<DataPrefabNPC> spawnOrder = new();
}

public void GenerateTodayApplicants()
{
    todayApplicants.Clear();
    currentApplicantIndex = 0;

    if (allData == null || gameManager == null)
    {
        Debug.LogError("SpawnManager: ยังไม่ได้กำหนด allData หรือ gameManager");
        return;
    }

    int requestedNPCCount = Random.Range(
        allData.minNPCPerDay,
        allData.maxNPCPerDay + 1
    );

    Dictionary<DataPrefabNPC, HashSet<GameObject>> usedPrefabsPerNPC = new();

    if (GetAllAvailableNPCs().Count == 0)
    {
        Debug.LogError("SpawnManager: ไม่มี NPC ที่พร้อมใช้งาน");
        gameManager.npcPerDay = 0;
        return;
    }

    int slotCount = todayListManager != null && todayListManager.slots != null
        ? todayListManager.slots.Length : 0;
    int attempts = 0;
    const int maxAttempts = 3000;
    
    DailyPlan plan = BuildDailyPlan(requestedNPCCount, slotCount);

    currentPlan = BuildDailyPlan(requestedNPCCount, slotCount);

    HashSet<DataPrefabNPC> seen = new();

    foreach (DataPrefabNPC dpn in currentPlan.spawnOrder)
    {
        
        bool inList = currentPlan.listDPN.Contains(dpn);

        // ครั้งแรกที่ DPN ใน List โผล่ = ตัวจริง, ครั้งต่อ ๆ ไป = ผี
        bool isGhost = inList && !seen.Add(dpn);

        todayApplicants.Add(new TodayApplicant
        {
            npcData = dpn,
            isInTodayList = inList,
            isGhost = isGhost,
            hasEnteredToday = false
        });
        
    }

    gameManager.npcPerDay = todayApplicants.Count;

    // 5. ให้ NPC ใน Today List มาก่อน
    List<TodayApplicant> todayListFirst = todayApplicants
        .Where(x => x != null && x.isInTodayList)
        .OrderBy(_ => Random.value)
        .ToList();

    List<TodayApplicant> normalApplicants = todayApplicants
        .Where(x => x != null && !x.isInTodayList)
        .OrderBy(_ => Random.value)
        .ToList();

    todayApplicants.Clear();
    todayApplicants.AddRange(todayListFirst);
    todayApplicants.AddRange(normalApplicants);

   
    

    
    // 8. UI แสดงรายชื่อ Today List ทั้งหมด
    List<NPCData> todayListData = currentPlan.listDPN
        .Select(d => GetNPCDataFromPrefabData(d))
        .Where(d => d != null)
        .Distinct()
        .ToList();

    if (todayListManager != null)
        todayListManager.GenerateTodayList(todayListData);

    Debug.Log(
        $"[SpawnManager] เป้าหมาย={requestedNPCCount} | " +
        $"Spawn จริง={todayApplicants.Count} | " +
        $"Today List ทั้งหมด={todayListData.Count} | " +
        $"Ghost={GetGhostCount(todayApplicants)}"
    );

    if (CampManager.Instance != null)
    {
        CampManager.Instance.BuildRuntimeCamp();
        CampManager.Instance.PrintAllCamps();
    }

    if (CampManager.Instance != null)
    {
        CampManager.Instance.BuildRuntimeCamp();
        CampManager.Instance.PrintAllCamps();
    }
}

private DailyPlan BuildDailyPlan(int requestedNPCCount, int slotCount)
{
    DailyPlan plan = new();

    List<DataPrefabNPC> pool = GetAllAvailableNPCs()
        .OrderBy(_ => Random.value)
        .ToList();

    const int poolSize = 10;
    if (pool.Count > poolSize)
        pool = pool.Take(poolSize).ToList();

    int listCount = Random.Range(allData.minTodayList, allData.maxTodayList + 1);
    listCount = Mathf.Min(listCount, slotCount, pool.Count);

    int extraCount = requestedNPCCount - todayListPlusGhostCount;
    extraCount = Mathf.Clamp(extraCount, 0, pool.Count - listCount);

    plan.listDPN.AddRange(pool.Take(listCount));
    plan.extraDPN.AddRange(pool.Skip(listCount).Take(extraCount));
    plan.campDPN.AddRange(pool.Skip(listCount + extraCount));

    if (CampManager.Instance != null)
    {
        foreach (var dpn in plan.listDPN.Concat(plan.extraDPN))
            foreach (var data in GetAllNPCDataOfDPN(dpn))
                CampManager.Instance.SetHomeToday(data, false);

        foreach (var dpn in plan.campDPN)
            foreach (var data in GetAllNPCDataOfDPN(dpn))
                CampManager.Instance.SetHomeToday(data, true);
    }
// การลืมใบคำขอจะตัดสินตอนเลือก Prefab จริงใน ResolveApplicantPrefab
// ---------- เลือก DPN 8 อัน จากตัวใน List ----------
// DPN ซ้ำได้ (ยังไม่เลือก prefab)
    List<DataPrefabNPC> slots = new();

    if (plan.listDPN.Count > 0)
    {
        // ทุกตัวใน List ออกมาอย่างน้อย 1 ครั้ง
        slots.AddRange(plan.listDPN);

        // ที่เหลือสุ่มจาก List จนครบ 8
        while (slots.Count < todayListPlusGhostCount)
            slots.Add(plan.listDPN[Random.Range(0, plan.listDPN.Count)]);
    }

// ---------- รวมกับ Extra แล้วจัดลำดับสปาว ----------
    plan.spawnOrder.AddRange(slots);
    plan.spawnOrder.AddRange(plan.extraDPN);
    plan.spawnOrder = plan.spawnOrder.OrderBy(_ => Random.value).ToList();
// ---------- ใครไม่ได้สปาว = เข้าแคมป์ทั้งหมด ----------
    if (CampManager.Instance != null)
    {
        HashSet<DataPrefabNPC> willSpawn = new(plan.spawnOrder);

        foreach (DataPrefabNPC d in GetAllAvailableNPCs())
        {
            bool spawns = willSpawn.Contains(d);

            foreach (NPCData data in GetAllNPCDataOfDPN(d))
                CampManager.Instance.SetHomeToday(data, !spawns);
        }
    }
    Debug.Log("[Plan] ลำดับสปาว: " + string.Join(", ", plan.spawnOrder.Select(d => d.name)));
    
    return plan;
}

private List<NPCData> GetAllNPCDataOfDPN(DataPrefabNPC dpn)
{
    List<NPCData> result = new();
    if (dpn == null) return result;

    NPCSpawnGroup[] groups = { dpn.todayGood, dpn.todayBad, dpn.normalGood, dpn.normalBad };

    foreach (var group in groups)
    {
        if (group == null || group.prefabs == null) continue;

        foreach (var pc in group.prefabs)
        {
            if (pc == null || pc.prefab == null) continue;

            NPC npc = pc.prefab.GetComponent<NPC>();
            if (npc != null && npc.data != null && !result.Contains(npc.data))
                result.Add(npc.data);
        }
    }

    return result;
}


private int GetGhostCount(List<TodayApplicant> applicants)
{
    return applicants.Count(x => x != null && x.isGhost);
}


private TodayApplicant CreateGhostApplicant(
    NPCData target,
    List<TodayApplicant> selectedTodayApplicants,
    Dictionary<DataPrefabNPC, HashSet<GameObject>> usedPrefabsPerNPC)
{
    if (target == null || selectedTodayApplicants == null)
        return null;

    // หา NPC ตัวจริงที่ผีจะปลอมเป็น
    TodayApplicant original = selectedTodayApplicants.FirstOrDefault(x =>
        x != null &&
        x.displayData == target &&
        x.spawnPrefab != null
    );

    if (original == null)
    {
        Debug.LogWarning(
            $"[Ghost] ไม่พบ NPC ต้นฉบับของ {target.npcName}"
        );
        return null;
    }

    // ใช้ Prefab และข้อมูลเดียวกับ NPC ต้นฉบับ
    // แต่กำหนดสถานะ Applicant นี้ให้เป็น Ghost
    TodayApplicant ghost = new TodayApplicant
    {
        npcData = original.npcData,
        selectedGroup = original.selectedGroup,
        spawnPrefab = original.spawnPrefab,
        displayData = original.displayData,

        isGood = false,
        isInTodayList = false,
        isGhost = true,
        ghostTarget = target,
        hasEnteredToday = false
    };

    Debug.Log(
        $"[Ghost] ผีปลอมเป็น {target.npcName} | " +
        $"Prefab={ghost.spawnPrefab.name} | Ghost=True"
    );

    return ghost;
}

    

    // =========================================================
    // รวบรวม NPC ที่มี Prefab และ NPCData ใช้งานได้
    // =========================================================
    private List<DataPrefabNPC> GetAllAvailableNPCs()
    {
        List<DataPrefabNPC> result = new();

        void AddRoleNPCs(RoleGroup role)
        {
            if (role == null || role.npcs == null)
                return;

            foreach (NPCGroupChance entry in role.npcs)
            {
                if (entry == null || entry.npc == null)
                    continue;

                if (entry.chance <= 0)
                    continue;

                if (result.Contains(entry.npc))
                    continue;

                NPCData data = GetNPCDataFromPrefabData(entry.npc);

                if (data == null)
                    continue;

                if (CampManager.Instance != null &&
                    !CampManager.Instance.IsAlive(data))
                    continue;

                result.Add(entry.npc);
            }
        }

        AddRoleNPCs(allData.villager);
        AddRoleNPCs(allData.monk);
        AddRoleNPCs(allData.special);

        return result;
    }

    // =========================================================
    // เลือก Role -> NPC โดยใช้ Chance
    // =========================================================
    private DataPrefabNPC ChooseAvailableNPC()
    {
        RoleGroup role = ChooseRole();

        if (role == null)
            return null;

        return ChooseNPC(role);
    }

    private RoleGroup ChooseRole()
    {
        if (allData == null)
            return null;

        List<RoleGroup> roles = new();

        AddRoleIfAvailable(roles, allData.villager);
        AddRoleIfAvailable(roles, allData.monk);
        AddRoleIfAvailable(roles, allData.special);

        if (roles.Count == 0)
            return null;

        int totalWeight = roles.Sum(r => Mathf.Max(0, r.spawnChance));

        if (totalWeight <= 0)
            return roles[Random.Range(0, roles.Count)];

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (RoleGroup role in roles)
        {
            current += Mathf.Max(0, role.spawnChance);

            if (random < current)
                return role;
        }

        return roles[roles.Count - 1];
    }

    private void AddRoleIfAvailable(
        List<RoleGroup> roles,
        RoleGroup role)
    {
        if (role == null || role.npcs == null)
            return;

        bool hasAvailableNPC = role.npcs.Any(entry =>
        {
            if (entry == null || entry.npc == null || entry.chance <= 0)
                return false;

            NPCData data = GetNPCDataFromPrefabData(entry.npc);

            return data != null &&
                   (CampManager.Instance == null ||
                    CampManager.Instance.IsAlive(data));
        });

        if (hasAvailableNPC)
            roles.Add(role);
    }

    // =========================================================
    // เลือก NPC ภายใน Role โดยใช้ NPCGroupChance.chance
    // =========================================================
    private DataPrefabNPC ChooseNPC(RoleGroup role)
    {
        if (role == null || role.npcs == null)
            return null;

        List<NPCGroupChance> available = new();

        foreach (NPCGroupChance entry in role.npcs)
        {
            if (entry == null || entry.npc == null || entry.chance <= 0)
                continue;

            NPCData data = GetNPCDataFromPrefabData(entry.npc);

            if (data == null)
                continue;

            if (CampManager.Instance != null &&
                !CampManager.Instance.IsAlive(data))
                continue;

            available.Add(entry);
        }

        if (available.Count == 0)
            return null;

        int totalWeight = available.Sum(n => n.chance);

        if (totalWeight <= 0)
            return available[Random.Range(0, available.Count)].npc;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCGroupChance entry in available)
        {
            current += entry.chance;

            if (random < current)
                return entry.npc;
        }

        return available[available.Count - 1].npc;
    }

    // =========================================================
    // ดึง NPCData จาก Prefab
    // =========================================================
    private NPCData GetNPCDataFromPrefabData(DataPrefabNPC data)
    {
        if (data == null)
            return null;

        NPCSpawnGroup[] groups =
        {
            data.todayGood,
            data.todayBad,
            data.normalGood,
            data.normalBad
        };

        foreach (NPCSpawnGroup group in groups)
        {
            if (group == null || group.prefabs == null)
                continue;

            foreach (NPCPrefabChance prefabChance in group.prefabs)
            {
                if (prefabChance == null || prefabChance.prefab == null)
                    continue;

                NPC npc = prefabChance.prefab.GetComponent<NPC>();

                if (npc != null && npc.data != null)
                    return npc.data;
            }
        }

        return null;
    }

    // =========================================================
    // สร้าง Applicant
    // =========================================================
    private TodayApplicant CreateApplicant(
        DataPrefabNPC npc,
        bool isInTodayList)
    {
        if (npc == null)
            return null;

        NPCSpawnGroup selectedGroup = ChooseSpawnGroup(
            npc,
            isInTodayList
        );

        if (selectedGroup == null)
            return null;

        return new TodayApplicant
        {
            npcData = npc,
            selectedGroup = selectedGroup,
            isGood = selectedGroup == npc.todayGood ||
                     selectedGroup == npc.normalGood,
            isInTodayList = isInTodayList,
            isGhost = selectedGroup.isGhost,
            hasEnteredToday = false
        };
    }

    // =========================================================
    // เลือกกลุ่ม Today/Normal Good/Bad ตาม Chance
    // =========================================================
    private NPCSpawnGroup ChooseSpawnGroup(
        DataPrefabNPC npc,
        bool isInTodayList)
    {
        if (npc == null)
            return null;

        NPCSpawnGroup goodGroup = isInTodayList
            ? npc.todayGood
            : npc.normalGood;

        NPCSpawnGroup badGroup = isInTodayList
            ? npc.todayBad
            : npc.normalBad;

        List<NPCSpawnGroup> groups = new();

        if (IsGroupAvailable(goodGroup))
            groups.Add(goodGroup);

        if (IsGroupAvailable(badGroup))
            groups.Add(badGroup);

        if (groups.Count == 0)
            return null;

        int totalWeight = groups.Sum(g => Mathf.Max(0, g.chance));

        if (totalWeight <= 0)
            return groups[Random.Range(0, groups.Count)];

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCSpawnGroup group in groups)
        {
            current += Mathf.Max(0, group.chance);

            if (random < current)
                return group;
        }

        return groups[groups.Count - 1];
    }

    private bool IsGroupAvailable(NPCSpawnGroup group)
    {
        return group != null &&
               group.chance > 0 &&
               group.prefabs != null &&
               group.prefabs.Any(p =>
                   p != null &&
                   p.prefab != null &&
                   p.chance > 0);
    }

    // =========================================================
    // เตรียม Prefab ของ Applicant
    // =========================================================
    private void PrepareApplicantPrefab(
        TodayApplicant applicant,
        Dictionary<DataPrefabNPC, HashSet<GameObject>> usedPrefabsPerNPC)
    {
        if (applicant == null || applicant.npcData == null)
            return;

        NPCSpawnGroup group = applicant.selectedGroup;

        if (group == null)
        {
            Debug.LogError(
                $"Applicant ของ {applicant.npcData.name} ไม่มี Selected Group"
            );
            return;
        }

        applicant.isGhost = group.isGhost;

        DataPrefabNPC sourceNPC = applicant.npcData;

        if (!usedPrefabsPerNPC.TryGetValue(
                sourceNPC, out HashSet<GameObject> usedPrefabs))
        {
            usedPrefabs = new HashSet<GameObject>();
            usedPrefabsPerNPC[sourceNPC] = usedPrefabs;
        }

        applicant.spawnPrefab = GetRandomPrefab(group, usedPrefabs);

        if (applicant.spawnPrefab == null)
        {
            Debug.LogError(
                $"{sourceNPC.name} ไม่มี Prefab ที่สามารถใช้ได้"
            );
            return;
        }

        usedPrefabs.Add(applicant.spawnPrefab);

        NPC npc = applicant.spawnPrefab.GetComponent<NPC>();

        if (npc == null)
        {
            Debug.LogError(
                $"{applicant.spawnPrefab.name} ไม่มี Script NPC"
            );
            applicant.spawnPrefab = null;
            return;
        }

        if (npc.data == null)
        {
            Debug.LogError(
                $"{applicant.spawnPrefab.name} ไม่มี NPCData"
            );
            applicant.spawnPrefab = null;
            return;
        }

        applicant.displayData = npc.data;

        // กำหนด ghostTarget ตอนสร้าง Ghost โดยตรง
// ห้ามกำหนดให้ Ghost ปลอมเป็นตัวเอง
        if (!applicant.isGhost)
        {
            applicant.ghostTarget = null;
        }
    }

    // =========================================================
    // สุ่ม Prefab โดยใช้ NPCPrefabChance.chance
    // =========================================================
    private GameObject GetRandomPrefab(
        NPCSpawnGroup group,
        HashSet<GameObject> excludedPrefabs)
    {
        if (group == null || group.prefabs == null ||
            group.prefabs.Count == 0)
            return null;

        List<NPCPrefabChance> available = group.prefabs
            .Where(p =>
                p != null &&
                p.prefab != null &&
                p.chance > 0 &&
                !excludedPrefabs.Contains(p.prefab))
            .ToList();

        // ใช้ Prefab ในกลุ่มครบแล้ว จึงอนุญาตให้ใช้ซ้ำ
        if (available.Count == 0)
        {
            available = group.prefabs
                .Where(p =>
                    p != null &&
                    p.prefab != null &&
                    p.chance > 0)
                .ToList();
        }

        if (available.Count == 0)
            return null;

        int totalWeight = available.Sum(p => p.chance);

        if (totalWeight <= 0)
            return available[Random.Range(0, available.Count)].prefab;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (NPCPrefabChance prefab in available)
        {
            current += prefab.chance;

            if (random < current)
                return prefab.prefab;
        }

        return available[available.Count - 1].prefab;
    }

    // =========================================================
    // Spawn NPC ทีละคน
    // =========================================================
    public void SpawnNextNPC()
{
    if (EndingManager.Instance != null &&
        EndingManager.Instance.IsBadEndTriggered)
    {
        Debug.Log("BAD END ถูก trigger -> หยุด Spawn");
        return;
    }

    if (gameManager.currentNPC != null)
        return;

    TodayApplicant applicant = null;

    while (currentApplicantIndex < todayApplicants.Count)
    {
        TodayApplicant nextApplicant = todayApplicants[currentApplicantIndex];
        currentApplicantIndex++;

        if (nextApplicant == null)
            continue;

        if (nextApplicant.hasEnteredToday)
            continue;

        // ลบเงื่อนไข IsHomeToday ออกแล้ว: DPN ที่เข้าแคมป์ไม่อยู่ใน spawnOrder
        // ตั้งแต่ต้น จึงไม่ต้องเช็คซ้ำ (ผีและตัวจริงใช้ DPN เดียวกัน เช็คแล้วจะถูกข้ามผิด)

        applicant = nextApplicant;
        break;
    }

    if (applicant == null)
    {
        Debug.Log("NPC ที่ต้องตรวจวันนี้หมดแล้ว -> จบวัน");
        gameManager.EndGame();
        return;
    }

    // เลือกพรีแฟบตอนสปาวจริง
    if (!ResolveApplicantPrefab(applicant))
    {
        Debug.LogError($"Applicant {applicant.npcData?.name} เลือกพรีแฟบไม่ได้ -> ข้าม");
        SpawnNextNPC();
        return;
    }

    GameObject npc = Instantiate(
        applicant.spawnPrefab,
        spawnPoint.position,
        Quaternion.identity
    );

    NPC npcScript = npc.GetComponent<NPC>();

    if (npcScript != null)
    {
        npcScript.applicant = applicant;
        npcScript.data = applicant.displayData;

        if (npcScript.data != null)
            npcScript.data.InitializeChecklistQuestions();
    }

    gameManager.currentNPC = npc;
    gameManager.currentState = GameManager.NPCState.WalkingToCheckpoint;

    NPCMovement movement = npc.GetComponent<NPCMovement>();

    if (movement != null)
        movement.MoveTo(stopPoint.position);

    Debug.Log(
        $"[Spawn Debug] Display={applicant.displayData?.npcName} | " +
        $"Target={applicant.ghostTarget?.npcName} | " +
        $"IsGhost={applicant.isGhost} | " +
        $"Prefab={applicant.spawnPrefab.name}"
    );
}   
// // ตรวจว่าข้อมูล NPC ผิดปกติเฉพาะช่องใบคำขอหรือไม่
    private const int DocumentIndex = 3;   // ช่องใบคำขอใน correctAnswers

    private bool HasDocumentAbnormality(NPCData data)
    {
        return data != null &&
               data.correctAnswers != null &&
               data.correctAnswers.Length > DocumentIndex &&
               data.correctAnswers[DocumentIndex];
    }

    private bool HasOnlyDocumentAbnormality(NPCData data)
    {
        if (!HasDocumentAbnormality(data)) return false;

        for (int i = 0; i < data.correctAnswers.Length; i++)
        {
            if (i != DocumentIndex && data.correctAnswers[i])
                return false;
        }

        return true;
    }

    // ลืมเอกสารได้เฉพาะ NPC Data ที่ผิดปกติช่องใบคำขอ
    // ผิดปกติเฉพาะใบคำขอ: ต้องมีตัวจริงอยู่ในแคมป์
    // มีความผิดปกติอื่นร่วมด้วย: ต้องมีตัวจริงหรือบัดดี้อยู่ในแคมป์
    private bool ShouldRollForgotDocument(TodayApplicant applicant)
    {
        if (applicant == null || CampManager.Instance == null)
        if (applicant == null || !applicant.isGhost || CampManager.Instance == null)
            return false;

        NPCData real = GetNPCDataFromPrefabData(applicant.npcData);
        if (!HasDocumentAbnormality(real))
            return false;

        bool realInCamp = CampManager.Instance.IsRealInCamp(real);
        if (HasOnlyDocumentAbnormality(real))
            return realInCamp;

        return realInCamp || CampManager.Instance.HasBuddyInCamp(real);
    }
private bool ResolveApplicantPrefab(TodayApplicant a)
    {
        DataPrefabNPC dpn = a.npcData;

        // อยู่ใน List ใช้พรีแฟบชุด today, ไม่อยู่ใช้ชุด normal
        NPCSpawnGroup good = a.isInTodayList ? dpn.todayGood : dpn.normalGood;
        NPCSpawnGroup bad  = a.isInTodayList ? dpn.todayBad  : dpn.normalBad;

        // เลือกกลุ่มที่ isGhost ตรงกับสถานะของ Applicant
        List<NPCSpawnGroup> candidates = new();
        if (IsGroupAvailable(good) && good.isGhost == a.isGhost) candidates.Add(good);
        if (IsGroupAvailable(bad)  && bad.isGhost  == a.isGhost) candidates.Add(bad);

        NPCSpawnGroup group = candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : ChooseSpawnGroup(dpn, a.isInTodayList);

        if (group == null) return false;

        a.selectedGroup = group;
        a.isGood = group == dpn.todayGood || group == dpn.normalGood;

        PrepareApplicantPrefab(a, usedPrefabsPerNPC);   // ต้องย้าย dictionary เป็นตัวแปรระดับคลาส
        if (a.spawnPrefab == null || a.displayData == null) return false;

        if (a.isGhost)
            a.ghostTarget = GetNPCDataFromPrefabData(dpn);

        bool forgot;
        if (a.isGhost)
        {
            forgot = ShouldRollForgotDocument(a) &&
                     Random.value < forgotDocumentChance;
        }
        else
        {
            forgot = Random.value < forgotDocumentChance;
        }

        a.hasTempleDocument = !forgot;

        if (a.hasTempleDocument &&
            a.displayData != null &&
            a.displayData.templeDocumentData != null)
        {
            // ลายเซ็นสุจิโต: เปิดได้ถึงวันที่เจ้าอาวาสตาย (รวมวันนั้น)
            bool signatureActive =
                AbbotStatusManager.Instance == null ||
                AbbotStatusManager.Instance.ShouldShowSignature(gameManager.currentDay);

            a.templeDocument = TempleDocumentGenerator.Generate(
                a.displayData.templeDocumentData,
                gameManager.currentDay,
                signatureActive
            );
        }
        else
        {
            a.templeDocument = null;
        }

        return true;
    }
    // =========================================================
    // รีเซ็ตวันใหม่
    // =========================================================
    public void ResetToday()
    {
        todayApplicants.Clear();
        currentApplicantIndex = 0;
    }
}




