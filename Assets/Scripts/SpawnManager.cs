
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

    // NPC ที่จัดไว้สำหรับวันนี้
    public List<TodayApplicant> todayApplicants = new();

    private int currentApplicantIndex = 0;

    // =========================================================
    // เริ่มวันใหม่
    // =========================================================
    public void GenerateTodayApplicants()
    {
        todayApplicants.Clear();
        currentApplicantIndex = 0;

        if (allData == null || gameManager == null)
        {
            Debug.LogError(
                "SpawnManager: ยังไม่ได้กำหนด allData หรือ gameManager"
            );
            return;
        }

        int requestedNPCCount = Random.Range(
            allData.minNPCPerDay,
            allData.maxNPCPerDay + 1
        );

        // จำนวนเป้าหมายของ NPC ที่ผู้เล่นต้องตรวจจริง
        gameManager.npcPerDay = requestedNPCCount;

        Dictionary<DataPrefabNPC, HashSet<GameObject>>
            usedPrefabsPerNPC = new();

        // รวบรวม DPN ที่ไม่ซ้ำ
        List<DataPrefabNPC> allNPCs = GetAllAvailableNPCs();

        if (allNPCs.Count == 0)
        {
            Debug.LogError(
                "SpawnManager: ไม่มี DataPrefabNPC ที่พร้อมใช้งาน"
            );

            gameManager.npcPerDay = 0;
            return;
        }

        // จำนวน Today List ต้องไม่เกิน NPC เป้าหมายและจำนวนช่อง
        int slotCount = todayListManager != null &&
                        todayListManager.slots != null
            ? todayListManager.slots.Length
            : 0;

        int availableTodayListMax = Mathf.Min(
            allData.maxTodayList,
            requestedNPCCount,
            slotCount
        );

        int availableTodayListMin = Mathf.Min(
            allData.minTodayList,
            availableTodayListMax
        );

        int requestedTodayListCount = availableTodayListMax > 0
            ? Random.Range(
                Mathf.Max(0, availableTodayListMin),
                availableTodayListMax + 1
            )
            : 0;

        // -----------------------------------------------------
        // 1. สร้าง Today List ก่อน
        // -----------------------------------------------------
        HashSet<NPCData> usedTodayListData = new();
        HashSet<string> usedTodayListNames = new();

        List<TodayApplicant> todayListApplicants = new();

        int attempts = 0;
        int maxAttempts = 3000;

        while (todayListApplicants.Count < requestedTodayListCount &&
               attempts++ < maxAttempts)
        {
            // เลือก Role -> NPC ตาม Chance
            DataPrefabNPC source = ChooseAvailableNPC();

            if (source == null)
                break;

            NPCSpawnGroup group = ChooseSpawnGroup(source, true);

            if (group == null)
                continue;

            TodayApplicant applicant = new TodayApplicant
            {
                npcData = source,
                selectedGroup = group,
                isGood = group == source.todayGood,
                isInTodayList = true,
                isGhost = group.isGhost,
                hasEnteredToday = false
            };

            PrepareApplicantPrefab(applicant, usedPrefabsPerNPC);

            if (applicant.spawnPrefab == null ||
                applicant.displayData == null)
                continue;

            NPCData displayData = applicant.displayData;

            // ป้องกัน NPCData และชื่อซ้ำ
            if (usedTodayListData.Contains(displayData))
                continue;

            if (!string.IsNullOrWhiteSpace(displayData.npcName) &&
                usedTodayListNames.Contains(displayData.npcName))
                continue;

            // NPC จริงที่อยู่ Camp วันนี้ ไม่ควรอยู่ใน Today List
            // แต่ Ghost ยังคงสามารถใช้รูปลักษณ์ของ NPC ที่อยู่ Camp ได้
            if (!applicant.isGhost &&
                CampManager.Instance != null &&
                CampManager.Instance.IsHomeToday(displayData))
            {
                continue;
            }

            if (displayData.TodayPhotoPrefab == null)
            {
                Debug.LogWarning(
                    $"Today List: {displayData.npcName} ไม่มี TodayPhotoPrefab"
                );
                continue;
            }

            usedTodayListData.Add(displayData);

            if (!string.IsNullOrWhiteSpace(displayData.npcName))
                usedTodayListNames.Add(displayData.npcName);

            todayListApplicants.Add(applicant);
        }

        if (todayListApplicants.Count < requestedTodayListCount)
        {
            Debug.LogWarning(
                $"Today List สร้างได้ {todayListApplicants.Count}/" +
                $"{requestedTodayListCount} คน " +
                "(อาจไม่มี NPC ที่ผ่านเงื่อนไขเพียงพอ)"
            );
        }

        // เพิ่ม NPC จาก Today List เข้า Applicant ประจำวัน
        todayApplicants.AddRange(todayListApplicants);

        // เก็บ NPCData และชื่อที่ถูกใช้แล้ว
        HashSet<NPCData> usedApplicantData = new(
            todayApplicants
                .Where(x => x != null && x.displayData != null)
                .Select(x => x.displayData)
        );

        HashSet<string> usedApplicantNames = new(
            todayApplicants
                .Where(x => x != null && x.displayData != null &&
                            !string.IsNullOrWhiteSpace(x.displayData.npcName))
                .Select(x => x.displayData.npcName)
        );

        // -----------------------------------------------------
        // 2. เติม NPC ที่เหลือให้ครบจำนวนที่ต้องตรวจจริง
        // -----------------------------------------------------
        attempts = 0;

        while (todayApplicants.Count < requestedNPCCount &&
               attempts++ < maxAttempts)
        {
            DataPrefabNPC source = ChooseAvailableNPC();

            if (source == null)
                break;

            TodayApplicant applicant = CreateApplicant(source, false);

            if (applicant == null)
                continue;

            PrepareApplicantPrefab(applicant, usedPrefabsPerNPC);

            if (applicant.spawnPrefab == null ||
                applicant.displayData == null)
                continue;

            // ป้องกัน NPCData หรือชื่อซ้ำ
            if (usedApplicantData.Contains(applicant.displayData))
                continue;

            if (!string.IsNullOrWhiteSpace(applicant.displayData.npcName) &&
                usedApplicantNames.Contains(applicant.displayData.npcName))
                continue;
            
            // ผู้สมัคร NPC จริงที่อยู่ Camp จะไม่ถูกนำมานับ
            // Ghost ยังคง Spawn ได้ตามระบบเดิม
            if (!applicant.isGhost &&
                CampManager.Instance != null &&
                CampManager.Instance.IsHomeToday(applicant.displayData))
            {
                continue;
            }

            applicant.hasEnteredToday = false;
            todayApplicants.Add(applicant);
            
            usedApplicantData.Add(applicant.displayData);

            if (!string.IsNullOrWhiteSpace(applicant.displayData.npcName))
                usedApplicantNames.Add(applicant.displayData.npcName);

        }

        if (todayApplicants.Count < requestedNPCCount)
        {
            Debug.LogWarning(
                $"SpawnManager: สร้างผู้สมัครที่ตรวจได้จริง " +
                $"{todayApplicants.Count}/{requestedNPCCount} คน " +
                "อาจไม่มี NPC ที่เข้าเงื่อนไขเพียงพอ"
            );
        }

        // จำนวนจริงที่จัดไว้ได้
        gameManager.npcPerDay = todayApplicants.Count;

        // -----------------------------------------------------
        // 3. สุ่มลำดับผู้สมัคร
        //    ให้ NPC ใน Today List มาก่อน
        //    แล้วจึงตามด้วย NPC นอก Today List
        // -----------------------------------------------------
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

        // -----------------------------------------------------
        // 4. สร้างเอกสาร
        // -----------------------------------------------------
        int today = gameManager.currentDay;

        foreach (TodayApplicant applicant in todayApplicants)
        {
            if (applicant.displayData == null)
                continue;

            applicant.hasTempleDocument = true;

            applicant.templeDocument = TempleDocumentGenerator.Generate(
                applicant.displayData.templeDocumentData,
                today,
                true
            );
        }

        // -----------------------------------------------------
        // 5. สุ่ม NPC ใน Today List หนึ่งคนให้ไม่มีเอกสาร
        // -----------------------------------------------------
        List<TodayApplicant> validTodayListApplicants =
            todayApplicants
                .Where(x => x.isInTodayList)
                .ToList();

        if (validTodayListApplicants.Count > 0)
        {
            TodayApplicant noDocumentApplicant =
                validTodayListApplicants[
                    Random.Range(0, validTodayListApplicants.Count)
                ];

            noDocumentApplicant.hasTempleDocument = false;
            noDocumentApplicant.templeDocument = null;

            if (CampManager.Instance != null &&
                noDocumentApplicant.displayData != null)
            {
                CampManager.Instance.SetDocumentInCamp(
                    noDocumentApplicant.displayData,
                    false
                );
            }
        }

        // -----------------------------------------------------
        // 6. สร้าง Today List UI เพียงครั้งเดียว
        // -----------------------------------------------------
        List<NPCData> todayListData = validTodayListApplicants
            .Where(x => x.displayData != null)
            .Select(x => x.displayData)
            .Distinct()
            .ToList();

        if (todayListManager != null)
        {
            todayListManager.GenerateTodayList(todayListData);
        }

        Debug.Log(
            $"[SpawnManager] NPC ที่ต้องตรวจ: " +
            $"{todayApplicants.Count}/{requestedNPCCount} | " +
            $"Today List: {todayListData.Count}/" +
            $"{requestedTodayListCount} | Slots: {slotCount}"
        );

        foreach (TodayApplicant applicant in todayApplicants)
        {
            bool homeToday = false;

            if (applicant.displayData != null &&
                CampManager.Instance != null)
            {
                homeToday = CampManager.Instance.IsHomeToday(
                    applicant.displayData
                );
            }

            Debug.Log(
                $"[SpawnManager] NPC={applicant.displayData?.npcName}, " +
                $"HomeToday={homeToday}"
            );
        }

        if (CampManager.Instance != null)
            CampManager.Instance.PrintAllCamps();
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

        if (applicant.isGhost)
        {
            applicant.ghostTarget = applicant.displayData;

            Debug.Log(
                $"Ghost Target = {applicant.ghostTarget.npcName}"
            );
        }
        else
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

        // ข้ามผู้สมัครที่ไม่ต้อง Spawn จริง
        while (currentApplicantIndex < todayApplicants.Count)
        {
            TodayApplicant nextApplicant =
                todayApplicants[currentApplicantIndex];

            currentApplicantIndex++;

            if (nextApplicant == null)
                continue;

            if (nextApplicant.hasEnteredToday)
            {
                Debug.Log(
                    $"{nextApplicant.displayData?.npcName} เข้าวัดแล้ววันนี้ -> ข้าม"
                );
                continue;
            }

            if (!nextApplicant.isGhost &&
                nextApplicant.displayData != null &&
                CampManager.Instance != null &&
                CampManager.Instance.IsHomeToday(nextApplicant.displayData))
            {
                Debug.Log(
                    $"{nextApplicant.displayData.npcName} อยู่ Camp วันนี้ -> ข้าม"
                );
                continue;
            }

            applicant = nextApplicant;
            break;
        }

        if (applicant == null)
        {
            Debug.Log("NPC ที่ต้องตรวจวันนี้หมดแล้ว -> จบวัน");
            gameManager.EndGame();
            return;
        }

        if (applicant.spawnPrefab == null)
        {
            Debug.LogError(
                $"Applicant {applicant.displayData?.npcName} ไม่มี Spawn Prefab"
            );
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
